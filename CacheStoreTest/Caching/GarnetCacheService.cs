
using Garnet.client;
using System.Text.Json;
using System.Xml.Linq;

namespace CacheStoreTest.Caching
{
    public class GarnetCacheService : IGarnetCacheService
    {
        static readonly string address = "127.0.0.1";
        static readonly int port = 6379;
        readonly GarnetClient garnetClient;
        
        public GarnetCacheService()
        {
            garnetClient = new GarnetClient(address, port);
            garnetClient.ConnectAsync();
        }

        public async Task DeleteKey(string key)
        {
            await garnetClient.KeyDeleteAsync(key);
        }

        public async Task<string[]> GetAllMessages(string key)
        {
            return await garnetClient.ListRangeAsync(key, 0, -1);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var cachedData = await garnetClient.StringGetAsync(key);
            if (string.IsNullOrEmpty(cachedData))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cachedData);
        }

        public async Task PushMessage(string key, string value)
        {
            var serializedData = JsonSerializer.Serialize(value);
            await garnetClient.ListRightPushAsync(key, value);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null)
        {
            var serializedData = JsonSerializer.Serialize(value);
            await garnetClient.StringSetAsync(key, serializedData);
        }
    }
}
