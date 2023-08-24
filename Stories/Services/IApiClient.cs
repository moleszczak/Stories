namespace Stories.Services
{
    public interface IApiClient<T> where T : class, new()
    {
        Task<T> FetchData(string url, CancellationToken cancellationToken);
    }
}