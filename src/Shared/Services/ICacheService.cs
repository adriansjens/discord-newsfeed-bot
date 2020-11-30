using StackExchange.Redis;

namespace Shared.Services
{
    public interface ICacheService
    {
        string Get(string key);
        void Set(string key, string value);
        void Dispose();
    }

    public class CacheService : ICacheService
    {
        private static ConnectionMultiplexer _connection;
        private IDatabase _cache;

        public CacheService(string connectionString)
        {
            _connection = ConnectionMultiplexer.Connect(connectionString); 
            _cache = _connection.GetDatabase();
        }

        public string Get(string key)
        {
            return _cache.StringGet(key);
        }

        public void Set(string key, string value)
        {
            _cache.StringSet(key, value);
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
