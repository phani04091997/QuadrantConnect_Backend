namespace H1bConnect_Backend.Repository.IServices
{
    public interface ICounterService
    {
        Task<int> GetNextSequenceValue(string collectionName);
        Task DecrementCounter(string collectionName);
    }
}
