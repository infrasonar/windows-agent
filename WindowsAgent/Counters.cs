using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace WindowsAgent
{
    using Item = Dictionary<string, object>;
    using Cache = Dictionary<string, Dictionary<string, PerformanceCounter>>;
    
    internal class Counters {
        public static void Get(string categoryName, Dictionary<string, Dictionary<string, PerformanceCounter>> _cache)
        {
            PerformanceCounterCategory cat = new PerformanceCounterCategory(categoryName);
            string[] instances = cat.GetInstanceNames();
            bool newInstances = false;

            foreach (string key in _cache.Keys)
            {
                if (Array.IndexOf(instances, key)  == -1)
                {
                    _cache.Remove(key);
                }
            }

            foreach (string instancename in instances)
            {
                if (!_cache.ContainsKey(instancename))
                {
                    PerformanceCounter[] newCounters = cat.GetCounters(instancename);
                    _cache[instancename] = new Dictionary<string, PerformanceCounter>();
                    foreach (PerformanceCounter counter in newCounters)
                    {
                        _cache[instancename][counter.CounterName] = counter;
                        counter.NextValue();
                    }
                    if (!newInstances)
                    {
                        newInstances = true;
                    }
                }
            }

            if (newInstances)
            {
                // Console.WriteLine("new instances, wait 3000");
                Thread.Sleep(3000);
            }
        }

        public static Item[] ToItemList(Dictionary<string, string> counters, Cache _cache)
        {
            int index = 0;
            var data = new CheckResult();
            Item[] items = new Item[_cache.Count];

            foreach (string instancename in _cache.Keys)
            {
                var item = new Item
                {
                    ["name"] = instancename,
                };
                foreach (KeyValuePair<string, string> counter in counters)
                {
                    item[counter.Key] = _cache[instancename][counter.Value].NextValue();
                }
                items[index++] = item;
            }
            
            return items;
        }
    }
}