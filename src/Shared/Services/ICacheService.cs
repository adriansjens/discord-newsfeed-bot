using StackExchange.Redis;

namespace Shared.Services
{
    public interface ICacheService
    {
        string Get(string key);
        void Set(string key, string value);
    }

    public class CacheService : ICacheService
    {
        private IDatabase _cache;

        public CacheService(string connectionString)
        {
            _cache = ConnectionMultiplexer.Connect(connectionString).GetDatabase();
        }

        public string Get(string key)
        {
            return _cache.StringGet(key);
        }

        public void Set(string key, string value)
        {
            _cache.StringSet(key, value);
        }
    }
}
