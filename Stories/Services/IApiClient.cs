namespace Stories.Services
{
    public interface IApiClient<T> where T : class
    {
        Task<T> Get(string url, CancellationToken cancellationToken);
    }
}