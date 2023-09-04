using Microsoft.Extensions.Caching.Memory;

namespace Stories.Services
{
    public class ApiClientWithCache<T> : IApiClient<T>
        where T : class
    {
        private readonly IApiClient<T> apiClient;
        private ILogger<ApiClientWithCache<T>> logger;
        private readonly IMemoryCache cache;
        private readonly CacheExpirationProvider cacheExpirationProvider;

        public ApiClientWithCache(IApiClient<T> apiClient, ILogger<ApiClientWithCache<T>> logger, IMemoryCache cache, CacheExpirationProvider cacheExpirationProvider)
        {
            this.apiClient = apiClient;
            this.logger = logger;
            this.cache = cache;
            this.cacheExpirationProvider = cacheExpirationProvider;
        }

        public async Task<T> FetchBestStoriesIds(string url, CancellationToken cancellationToken)
        {
            if (this.cache.TryGetValue<T>(url, out var item))
            {
                this.logger.LogDebug("Response {url} found in cache.", url);
                return item;
            }
            else
            {
                item = await this.apiClient.FetchBestStoriesIds(url, cancellationToken);

                this.PutToCache(url, item);

                return item;
            }
        }

        private void PutToCache(string cacheKey, T item)
        {
            this.cache.Set(cacheKey, item, cacheExpirationProvider());
        }
    }
}
