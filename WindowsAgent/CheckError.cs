using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAgent
{
    internal class CheckError
    {
        private class ChkError
        {
            [JsonProperty(PropertyName = "severity")]
            public string Severity { get; set; }

            [JsonProperty(PropertyName = "message")]
            public string Message { get; set; }
        }

        private class Result
        {
            [JsonProperty(PropertyName = "version")]
            public string Version { get; set; }

            [JsonProperty(PropertyName = "error")]
            public ChkError Err { get; set; }
        }


        static public string GetData(string message)
        {
            var data = new Result
            {
                Version = InfraSonarAgent.GetVersion(),
                Err = new ChkError
                {
                    Severity = "MEDIUM",
                    Message = message,
                },
            };
            return JsonConvert.SerializeObject(data);
        }
    }
}
