namespace CacheStoreTest.Caching
{
    using System;
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null);

       
    }
}
