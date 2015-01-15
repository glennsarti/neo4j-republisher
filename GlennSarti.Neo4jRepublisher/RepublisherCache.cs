using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;

using System.Runtime.Caching;


namespace GlennSarti.Neo4jRepublisher
{
  public class RepublisherCache
  {
    // Gets a reference to the default MemoryCache instance. 
    private static ObjectCache _memCache = MemoryCache.Default;

    public void AddItem(string cacheKeyName, object cacheItem, int ttlSeconds)
    {
      CacheItemPolicy cachePolicy = new CacheItemPolicy();
      cachePolicy.Priority = CacheItemPriority.Default;
      cachePolicy.SlidingExpiration = System.TimeSpan.FromSeconds(ttlSeconds);
      cachePolicy.RemovedCallback = new CacheEntryRemovedCallback(this.CachedItemRemovedCallback);
      //policy.ChangeMonitors.Add(new HostFileChangeMonitor(FilePath));

      // Add inside cache 
      _memCache.Set(cacheKeyName, cacheItem, cachePolicy);
    }

    public object GetItem(string cacheKeyName)
    {
      return _memCache[cacheKeyName] as object;
    }

    public bool ContainsKeyName(string cacheKeyName)
    {
      return _memCache.Contains(cacheKeyName);
    }

    public void RemoveItem(string cacheKeyName)
    {
      if (ContainsKeyName(cacheKeyName))
      {
        _memCache.Remove(cacheKeyName);
      }
    }

    private void CachedItemRemovedCallback(CacheEntryRemovedArguments arguments)
    {
      // Log these values from arguments list 
      // string strLog = string.Concat("Item removed from cache. Reason: ", arguments.RemovedReason.ToString(), "  Key-Name: ", arguments.CacheItem.Key);
      // Debug.WriteLine(strLog);
    }
  }
}
