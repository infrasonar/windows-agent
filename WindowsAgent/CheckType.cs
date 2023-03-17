using System.Collections.Generic;

namespace WindowsAgent
{
    internal class CheckType
    {
        private readonly string _name;

        private readonly Dictionary<string, Dictionary<string, object>[]> _data;
        public CheckType(string name)
        {
            _name = name;
            _data = new Dictionary<string, Dictionary<string, object>[]>();
        }

        public void Set(string name, Dictionary<string, object>[] items)
        {
            _data[name] = items;
        }

        public string Name()
        {
            return _name;
        }

        public Dictionary<string, Dictionary<string, object>[]> Data()
        {
            return _data;
        }
    }
}
