using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAgent
{
    internal class Announce
    {
        private class RespContainerId
        {
            [JsonProperty(PropertyName = "containerId")]
            public ulong ContainerId { get; set; }
        }

        private class RespAssetId
        {
            [JsonProperty(PropertyName = "assetId")]
            public ulong AssetId { get; set; }
        }

        private class RespAssetname
        {
            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }

        public async static Task JoinAsset()
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", Config.GetAuthorization());
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");

            try
            {
                string url = string.Format("{0}/asset/{1}?field=namae", Config.GetApiUrl(), Config.GetAssetId());
                var request = new HttpRequestMessage(HttpMethod.Get, url);

                using (var resp = await client.SendAsync(request))
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        string json = await resp.Content.ReadAsStringAsync();
                        Config.SetAssetName(JsonConvert.DeserializeObject<RespAssetname>(json).Name);
                    }
                    else
                    {
                        string msg = await resp.Content.ReadAsStringAsync();
                        throw new Exception(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Failed to get asset name: {0}", ex.Message));
            }
        }

        public async static Task CreateAsset()
        {
            ulong containerId, assetId;
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", Config.GetAuthorization());
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");

            try
            {
                string url = string.Format("{0}/container/id", Config.GetApiUrl());
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                using (var resp = await client.SendAsync(request))
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        string json = await resp.Content.ReadAsStringAsync();
                        containerId = JsonConvert.DeserializeObject<RespContainerId>(json).ContainerId;
                    }
                    else
                    {
                        string msg = await resp.Content.ReadAsStringAsync();
                        throw new Exception(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Failed to read container Id: {0}", ex.Message));
            }

            try
            {
                string body = JsonConvert.SerializeObject(new { name = Config.GetAssetName() });
                string url = string.Format("{0}/container/{1}/asset", Config.GetApiUrl(), containerId);
                var request = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };

                using (var resp = await client.SendAsync(request))
                {
                    if (resp.StatusCode == HttpStatusCode.Created)
                    {
                        string json = await resp.Content.ReadAsStringAsync();
                        assetId = JsonConvert.DeserializeObject<RespAssetId>(json).AssetId;
                    }
                    else
                    {
                        string msg = await resp.Content.ReadAsStringAsync();
                        throw new Exception(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Failed to create asset: {0}", ex.Message));
            }

            try
            {
                string url = string.Format("{0}/asset/{1}/collector/{2}", Config.GetApiUrl(), assetId, InfraSonarAgent.CollectorKey);
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                using (var resp = await client.SendAsync(request))
                {
                    if (resp.StatusCode != HttpStatusCode.NoContent)
                    {
                        string msg = await resp.Content.ReadAsStringAsync();
                        throw new Exception(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(String.Format("Failed to set collector: {0}", ex.Message), EventLogEntryType.Error, EventId.AssignCollectorFailed);
            }

            try
            {
                string body = JsonConvert.SerializeObject(new { kind = InfraSonarAgent.AssetKind });
                string url = string.Format("{0}/asset/{1}/kind", Config.GetApiUrl(), assetId);
                var method = new HttpMethod("PATCH");
                var request = new HttpRequestMessage(method, url)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json")
                };

                using (HttpResponseMessage resp = await client.SendAsync(request))
                {
                    if (resp.StatusCode != HttpStatusCode.NoContent)
                    {
                        string msg = await resp.Content.ReadAsStringAsync();
                        throw new Exception(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(String.Format("Failed to set kind: {0}", ex.Message), EventLogEntryType.Error, EventId.AssignKindFailed);
            }

            Config.SetAssetId(assetId);
        }
    }
}
