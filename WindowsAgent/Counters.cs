using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace WindowsAgent
{
    using Item = Dictionary<string, object>;
    using Cache = Dictionary<string, Dictionary<string, PerformanceCounter>>;
    
    internal class Counters {
        public static void Get(string categoryName, Cache _cache)
        {
            PerformanceCounterCategory cat = new PerformanceCounterCategory(categoryName);
            string[] instances = cat.GetInstanceNames();
            bool newInstances = false;

            foreach (string key in _cache.Keys)
            {
                if (Array.IndexOf(instances, key) == -1)
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
                    newInstances = true;
                }
            }

            if (newInstances)
            {
                // We need a sleep when new instances were found;
                // This ensures we have counters over a small time window;
                Thread.Sleep(3000);
            }
        }

        public static void GetSingle(string categoryName, Cache _cache)
        {
            PerformanceCounterCategory cat = new PerformanceCounterCategory(categoryName);
            string instancename = categoryName.ToLower();

            if (!_cache.ContainsKey(instancename))
            {
                PerformanceCounter[] newCounters = cat.GetCounters();
                _cache[instancename] = new Dictionary<string, PerformanceCounter>();
                foreach (PerformanceCounter counter in newCounters)
                {
                    _cache[instancename][counter.CounterName] = counter;
                    counter.NextValue();
                }

                // We need a sleep when new instances were found;
                // This ensures we have counters over a small time window;
                Thread.Sleep(3000);
            }
        }

        public static Item[] ToItemList(Dictionary<string, string> counters, Cache _cache)
        {
            int index = 0;
            var hasTotal = _cache.ContainsKey("_Total");
            Item[] items = new Item[_cache.Count - (hasTotal ? 1 : 0)];

            foreach (string instancename in _cache.Keys)
            {
                if (instancename != "_Total")
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
            }

            return items;
        }

        public static Item[] ToItemListTotal(Dictionary<string, string> counters, Cache _cache)
        {
            Item[] items = new Item[1];

            var item = new Item
            {
                ["name"] = "total",
            };
            foreach (KeyValuePair<string, string> counter in counters)
            {
                item[counter.Key] = _cache["_Total"][counter.Value].NextValue();
            }
            items[0] = item;

            return items;
        }
    }
}