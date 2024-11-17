namespace MinimalTaskManagingAPI.Interfaces
{
    public interface ICacheKeyService
    {
        void InvalidateStoredUserKeys();

        void AddKeyToStoredUserKeys(string key);

        void AddKeyToStoredTaskKeys(string key);

        void InvalidStoredTaskKeys();
    }
}
