namespace Stories.Services
{
    public interface IApiClient<T> where T : class
    {
        Task<T> FetchData(string url, CancellationToken cancellationToken);
    }
}