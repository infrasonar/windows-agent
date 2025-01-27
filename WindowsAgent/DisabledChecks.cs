using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsAgent
{
    internal class DisabledChecks
    {
        private static readonly List<string> _checks = new List<string>();
        private static readonly long _maxAge = Config.DisabledChecksCacheAge();
        static private long _age = 0;
        static SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private class RespDisabledCheck
        {
            [JsonProperty(PropertyName = "collector")]
            public string Collector { get; set; }
            [JsonProperty(PropertyName = "check")]
            public string Check { get; set; }
        }

        private class RespDisabledChecks
        {
            [JsonProperty(PropertyName = "disabledChecks")]
            public RespDisabledCheck[] DisabledChecks { get; set; }
    }

        private static async Task UpdateList()
        {
            _checks.Clear();

            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", Config.GetAuthorization());
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");

            try
            {
                string url = string.Format("{0}/asset/{1}?field=disabledChecks", Config.GetApiUrl(), Config.GetAssetId());
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                using (var resp = await client.SendAsync(request))
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        string json = await resp.Content.ReadAsStringAsync();
                        
                        foreach (RespDisabledCheck dc in JsonConvert.DeserializeObject<RespDisabledChecks>(json).DisabledChecks)
                        {
                            if (dc.Collector == InfraSonarAgent.CollectorKey) {
                                _checks.Add(dc.Check);
                            }
                        }
                    }
                }                
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed to get disabled checks: {0}", ex.Message));                
            }
        }

        public static async Task<bool> IsDisabled(string check)
        {
            await _lock.WaitAsync();
            try
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (now - _age > _maxAge)
                {
                     await UpdateList();
                    _age = now;
                    Logger.Write("Updated disabled checks cache", System.Diagnostics.EventLogEntryType.Information, EventId.None);
                }
                return _checks.Contains(check);
            }
            catch
            {
                return false;
            }
            finally
            {
                _lock.Release();
            }               
        }
    }
}
