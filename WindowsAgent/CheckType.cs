using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsAgent
{
    internal class CheckType
    {
        private string _name;

        private Dictionary<string, object> _data;
        public CheckType(string name)
        {
            _name = name;
            _data = new Dictionary<string, object>();
        }

        public void Set(string name, object value)
        {
            _data[name] = value;
        }

        public string Name()
        {
            return _name;
        }

        public Dictionary<string, object> Data()
        {
            return _data;
        }
    }
}
