using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAgent
{
    
    internal class CheckResult
    {
        private Dictionary<string, Dictionary<string, object>> _result;

        public CheckResult()
        {
            _result = new Dictionary<string, Dictionary<string, object>>();
        }

        public void Add(CheckType type)
        {
            _result[type.Name()] = type.Data();
        }
    }
}
