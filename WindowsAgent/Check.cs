using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsAgent
{
    internal abstract class Check
    {
        private static readonly Random _r = new Random();
        private int _interval;

        public void Start()
        {
            _interval = Config.GetCheckInterval(this.Key(), this.DefaultInterval());

            Config.WriteCheckInterval(this.Key(), _interval);

            if (_interval > 0)
            {
                _ = Task.Run(() => CheckTask());
            }
            else
            {
                Logger.Write(string.Format("Check {0} is disabled", this.Key()), EventLogEntryType.Information, EventId.None);
            }
        }

        private async Task SendToHub(string body)
        {

            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", Config.GetAuthorization());
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");

            string url = string.Format("{0}/asset/{1}/collector/{2}/check/{3}", Config.GetApiUrl(), Config.GetAssetId(), InfraSonarAgent.CollectorKey, this.Key());

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            try
            {
                using (var resp = await client.SendAsync(request))
                {
                    if (resp.StatusCode != HttpStatusCode.NoContent)
                    {
                        string msg = await resp.Content.ReadAsStringAsync();
                        throw new Exception(msg);
                    }
                }
                if (Config.IsDebug())
                {
                    Logger.Write(string.Format("Successfully send check to hub ({0})", this.Key()), EventLogEntryType.Information, EventId.None);
                }
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Failed to upload check data ({0}): {1}", this.Key(), ex.Message), EventLogEntryType.Error, EventId.UploadFailed);
            }
        }

        public async Task CheckTask()
        {
            if (!Config.IsLocalOnly())
            {
                int initialWait = _r.Next(59, _interval * 60);
                await Task.Delay(TimeSpan.FromSeconds(initialWait));
            }

            while (true)
            {
                new Thread(() =>
                {
                    try
                    {
                        string body = Run().GetData();
                        if (Config.IsLocalOnly())
                        {
                            string tempPath = Path.GetTempPath();
                            string fileName = Path.Combine(tempPath, string.Format("infrasonar-{0}-{1}.json", InfraSonarAgent.CollectorKey, this.Key()));

                            File.WriteAllText(fileName, body);
                            Logger.Write(string.Format("Data for check written to file: {0}", fileName), EventLogEntryType.Information, EventId.None);
                        }
                        else
                        {
                            Task.Run(async () => await SendToHub(body)).Wait();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (Config.IsDebug())
                        {
                            Logger.Write(string.Format("Failed to run check ({0}): {1} {2}", this.Key(), ex.Message, ex.StackTrace), EventLogEntryType.Error, EventId.CheckError);
                        }
                        else
                        {
                            Logger.Write(string.Format("Failed to run check ({0}): {1}", this.Key(), ex.Message), EventLogEntryType.Error, EventId.CheckError);
                        }
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
