namespace CacheStoreTest.Caching
{
    using Microsoft.Extensions.Caching.Distributed;
    using System.Text.Json;
    using System.Threading.Tasks;

    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDistributedCache cache;

        public RedisCacheService(IDistributedCache cache)
        {
            this.cache = cache;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var cachedData = await cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedData))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedData);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow ?? TimeSpan.FromMinutes(5)
            };

            var serializedData = JsonSerializer.Serialize(value);
            await cache.SetStringAsync(key, serializedData, options);
        }
    }

}
