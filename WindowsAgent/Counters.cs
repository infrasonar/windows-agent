using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace WindowsAgent
{
    using Cache = Dictionary<string, Dictionary<string, PerformanceCounter>>;
    using CacheItem = Dictionary<string, PerformanceCounter>;

    internal class Counters {

        private static CacheItem GetCountersForItem(PerformanceCounterCategory cat, string instance, string[] counterNames)
        {
            var countersItem = new CacheItem();
            foreach (PerformanceCounter counter in cat.GetCounters(instance))
            {
                if (counterNames.Contains(counter.CounterName))
                {
                    counter.NextValue();
                    countersItem[counter.CounterName] = counter;
                }
            }

            return countersItem;
        }

        public static void Get(string categoryName, string[] counterNames, Cache _cache)
        {
            PerformanceCounterCategory cat = new PerformanceCounterCategory(categoryName);
            string[] instances = cat.GetInstanceNames();
            bool newInstances = false;

            foreach (string instance in instances)
            {
                if (!_cache.ContainsKey(instance))
                {
                    try
                    {
                        _cache[instance] = GetCountersForItem(cat, instance, counterNames);
                    }
                    catch
                    {
                        string e = string.Format("Failed to retrieve initial counter values for {0} : {1}", categoryName, instance);
                        Logger.Write(e, EventLogEntryType.Warning, EventId.InitRegistry);
                    }
                }
            }

            if (newInstances)
            {
                // We need a sleep when new instances were found;
                // This ensures we have counters over a small time window;
                Thread.Sleep(3000);

                // retrieve instance names again
                instances = cat.GetInstanceNames();
            }

            // cleanup
            foreach (string key in _cache.Keys.ToList())
            {
                if (Array.IndexOf(instances, key) == -1)
                {
                    _cache.Remove(key);
                }
            }
        }

        public static void GetSingle(string categoryName, string[] counterNames, Cache _cache)
        {
            PerformanceCounterCategory cat = new PerformanceCounterCategory(categoryName);
            string instance = categoryName.ToLower();

            if (!_cache.ContainsKey(instance))
            {
                _cache[instance] = new CacheItem();
                foreach (PerformanceCounter counter in cat.GetCounters())
                {
                    if (counterNames.Contains(counter.CounterName))
                    {
                        _cache[instance][counter.CounterName] = counter;
                        counter.NextValue();
                    }
                }
                // We need a sleep when new instances were found;
                // This ensures we have counters over a small time window;
                Thread.Sleep(3000);
            }
        }
    }
}