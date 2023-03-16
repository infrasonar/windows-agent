using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Diagnostics;

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
            if (_interval > 0)
            {
                _task = Task.Run(() => CheckTask());
            }
            else
            {
                Logger.Write(string.Format("check {0} is disabled", this.Key()), EventLogEntryType.Information, EventId.CheckDisabled);
            }
            
        }

        private void _sendToHub(string checkKey, string body)
        {            
            var cli = new WebClient();
            
            cli.Encoding = Encoding.UTF8;

            cli.Headers.Add(HttpRequestHeader.ContentType, "application/json");            
            cli.Headers.Add(HttpRequestHeader.Authorization, Config.GetAuthorization());
            cli.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");

            string url = string.Format("{0}/asset/{1}/collector/{2}/check/{3}", Config.GetApiUrl(), InfraSonarAgent.GetAssetId(), InfraSonarAgent.CollectorKey, checkKey);
            try
            {
                string response = cli.UploadString(url, body);                
            }
            catch (Exception ex)
            {
                Logger.Write(string.Format("Failed to upload check data ({0}): {1}", checkKey, ex.Message), EventLogEntryType.Error, EventId.UploadFailed);
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

                    }
                    catch (Exception ex)
                    {

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
