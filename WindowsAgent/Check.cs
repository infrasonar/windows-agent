using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Diagnostics;
using System.Net.Http;

namespace WindowsAgent
{
    internal abstract class Check
    {
        private static Random _r = new Random();
        private Task _task;
        private int _interval;

        public void Start()
        {
            _interval = Config.GetCheckInterval(this.Key(), this.DefaultInterval());
            
            Config.WriteCheckInterval(this.Key(), _interval);

            if (_interval > 0)
            {
                _task = Task.Run(() => CheckTask());
            }
            else
            {
                Logger.Write(string.Format("check {0} is disabled", this.Key()), EventLogEntryType.Information, EventId.CheckDisabled);
            }
            
        }

        private async Task SendToHub(string body)
        {

            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", Config.GetAuthorization());
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");

            string url = string.Format("{0}/asset/{1}/collector/{2}/check/{3}", Config.GetApiUrl(), Config.GetAssetId(), InfraSonarAgent.CollectorKey, this.Key());

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            try
            {
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
                Logger.Write(string.Format("Failed to upload check data ({0}): {1}", this.Key(), ex.Message), EventLogEntryType.Error, EventId.UploadFailed);
            }
        }

        public async Task CheckTask()
        {
            int initialWait = _r.Next(5, _interval * 60);
            await Task.Delay(TimeSpan.FromSeconds(initialWait));

            // repeatedly wait a while and DoWork():
            while (true)
            {
                new Thread(() =>
                {
                    try
                    {
                        string body = Run().GetData();
                        Task.Run(async () => await SendToHub(body)).Wait();
                    }
                    catch (Exception ex)
                    {
                        Logger.Write(string.Format("Failed to run check ({0}): {1}", this.Key(), ex.Message), EventLogEntryType.Error, EventId.UploadFailed);
                    }                                      
                }).Start();               
                await Task.Delay(TimeSpan.FromMinutes(_interval));
            }
        }

        public abstract string Key();  // Key of the check
        public abstract int DefaultInterval();  // Interval in minutes
        public abstract bool CanRun();  // Is it possible to run this check on this machine
        public abstract CheckResult Run();  //  Must return the check result data
    }
}
