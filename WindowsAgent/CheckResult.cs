using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
            Data data = new Data
            {
                version = InfraSonarAgent.GetVersion(),
                data = _result,
            };            
            return JsonConvert.SerializeObject(data);
        }
    }
}
