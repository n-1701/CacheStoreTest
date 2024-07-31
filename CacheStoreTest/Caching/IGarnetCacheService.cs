namespace CacheStoreTest.Caching
{
    public interface IGarnetCacheService : ICacheService
    {
        public Task PushMessage(string key, string value);
        public Task<string[]> GetAllMessages(string key);
        public Task DeleteKey(string key);
    }
}
