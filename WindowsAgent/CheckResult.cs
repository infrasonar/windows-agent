using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace WindowsAgent
{
    internal class CheckResult
    {
        private class Result
        {
            [JsonProperty(PropertyName = "version")]
            public string Version { get; set; }

            [JsonProperty(PropertyName = "data")]
            public Dictionary<string, Dictionary<string, object>[]> Data { get; set; }
        }

        private readonly Dictionary<string, Dictionary<string, object>[]> _result;

        public CheckResult()
        {
            _result = new Dictionary<string, Dictionary<string, object>[]>();
        }

        public void AddType(string name, Dictionary<string, object>[] items)
        {
            _result[name] = items;
        }

        public string GetData()
        {
            var data = new Result
            {
                Version = InfraSonarAgent.GetVersion(),
                Data = _result,
            };
            return JsonConvert.SerializeObject(data);
        }
        // Can be used to convert a string to a name (must be ascii compatible)
        public static string ConvertUnicodeStringToAscii(string text)
        {
            if (text is null)
            {
                return null;
            }
            var sb = new StringBuilder();
            foreach (char c in text)
            {
                int unicode = c;
                if (unicode < 128)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append('?');
                }
            }
            return sb.ToString();
        }
    }
}
