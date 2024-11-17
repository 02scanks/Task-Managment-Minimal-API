using MinimalTaskManagingAPI.Interfaces;

namespace MinimalTaskManagingAPI.Services
{
    public class CacheKeyService : ICacheKeyService
    {
        public List<string> AllUserKeys { get; set; } = new List<string>();
        public List<string> AllTaskKeys { get; set; } = new List<string>();

        public void InvalidateStoredUserKeys()
        {
            AllUserKeys.Clear();
        }

        public void AddKeyToStoredUserKeys(string key) 
        {
            AllUserKeys.Add(key);
        }

        public void AddKeyToStoredTaskKeys(string key)
        {
            AllTaskKeys.Add(key);
        }

        public void InvalidStoredTaskKeys()
        {
            AllUserKeys.Clear();
        }
    }
}
