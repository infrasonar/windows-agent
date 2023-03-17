using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAgent
{
    internal class Announce
    {

        private class ContainerId
        {
            public ulong containerId { get; set; }
        }

        private class AssetId
        {
            public ulong assetId { get; set; }
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                using (HttpResponseMessage resp = await client.SendAsync(request))
                {
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        string json = await resp.Content.ReadAsStringAsync();
                        ContainerId res = JsonConvert.DeserializeObject<ContainerId>(json);
                        containerId = res.containerId;
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

                using (HttpResponseMessage resp = await client.SendAsync(request))
                {
                    if (resp.StatusCode == HttpStatusCode.Created)
                    {
                        string json = await resp.Content.ReadAsStringAsync();
                        AssetId res = JsonConvert.DeserializeObject<AssetId>(json);
                        assetId = res.assetId;
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
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                
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
                Logger.Write(String.Format("Failed to set collector: {0}", ex.Message), EventLogEntryType.Error, EventId.AssignCollectorFailed);
            }

            try
            {
                string body = JsonConvert.SerializeObject(new { kind = InfraSonarAgent.AssetKind });
                string url = string.Format("{0}/asset/{1}/kind", Config.GetApiUrl(), assetId);
                var method = new HttpMethod("PATCH");
                HttpRequestMessage request = new HttpRequestMessage(method, url);
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");

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
