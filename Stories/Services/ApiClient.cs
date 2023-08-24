using Flurl.Http;
using Polly;
using Polly.Retry;

namespace Stories.Services
{
    public class ApiClient<T> : IApiClient<T>
        where T : class
    {
        private static SemaphoreSlim semaphor = new SemaphoreSlim(5, 5);

        private readonly ILogger<ApiClient<T>> logger;
        private readonly AsyncRetryPolicy policy;
        private readonly int maxRetries = 2;

        public ApiClient(ILogger<ApiClient<T>> logger, WaitDurationProvider delayProvider)
        {
            this.logger = logger;
            this.policy = Policy
                .Handle<FlurlHttpException>()
                .WaitAndRetryAsync(maxRetries, i => delayProvider(i), (e, s, p, x) =>
                {
                    this.logger.LogWarning("Request failed.");
                    if (p < maxRetries)
                    {
                        this.logger.LogInformation("Next retry {0} in {1}", p, s.TotalSeconds);
                    }
                });
        }

        public async Task<T> FetchData(string url, CancellationToken cancellationToken)
        {
            this.logger.LogDebug("Preparing request {url}", url);

            using IFlurlClient client = new FlurlClient();

            var request = url.WithClient(client);

            this.logger.LogDebug("Request to {url} ready to send.", url);

            try
            {
                await semaphor.WaitAsync(cancellationToken);

                this.logger.LogInformation("Sending request to {0}", request.Url);

                var item = await this.policy.ExecuteAsync(() => request.GetJsonAsync<T>(cancellationToken));

                this.logger.LogInformation("Sending request to {0} [SUCCESS].", request.Url);

                return item;
            }
            catch (FlurlHttpException ex)
            {
                this.logger.LogError(ex, "Sending request to {0} [FAILURE].", request.Url);
                throw;
            }
            finally
            {
                semaphor.Release();
            }
        }      
    }
}
