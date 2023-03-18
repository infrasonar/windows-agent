using Newtonsoft.Json;
using System.Collections.Generic;

namespace WindowsAgent
{
    internal class CheckResult
    {
        private class Data
        {
            public string version { get; set; }
            public Dictionary<string, Dictionary<string, object>[]> data { get; set; }
        }

        private Dictionary<string, Dictionary<string, object>[]> _result;


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
            var data = new Data
            {
                version = InfraSonarAgent.GetVersion(),
                data = _result,
            };
            return JsonConvert.SerializeObject(data);
        }
    }
}
