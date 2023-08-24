using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stories.Configuration;
using Stories.Services;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;

namespace Stories.Tests
{
    public class StoryApiClientTests
    {
        private string apiUrl = "https://dummy.com";
        private HttpTest _httpTest;
        private ServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            _httpTest = new HttpTest();

            var services = new ServiceCollection();
            
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            var mockConfiguration = Substitute.For<IStoriesApiConfiguration>();
            mockConfiguration.Url.Returns(this.apiUrl);

            services.TryAddSingleton(mockConfiguration);
            services.TryAddScoped<IStoryApiClient, StoryApiClient>();
            services.TryAddSingleton<WaitDurationProvider>((attempt) => TimeSpan.FromMilliseconds(1));
            services.TryAddSingleton<CacheExpirationProvider>(() => TimeSpan.FromSeconds(10));

            services.TryAddScoped(typeof(IApiClient<IEnumerable<int>>), typeof(ApiClient<IEnumerable<int>>));
            services.TryDecorate(typeof(IApiClient<IEnumerable<int>>), typeof(ApiClientWithCache<IEnumerable<int>>));

            var mockMemoryCache = Substitute.For<IMemoryCache>();
            services.TryAddSingleton(mockMemoryCache);

            this.serviceProvider = services.BuildServiceProvider();
        }

        [TearDown]
        public void DisposeHttpTest()
        {
            _httpTest.Dispose();
        }

        [Test]
        public async Task Should_Return_BeststoriesIds_Success()
        {
            int[] ids = new[] { 1, 2, 3 };
            string url = $"{apiUrl}/beststories.json";
            _httpTest
                .ForCallsTo(url)
                .RespondWithJson(ids);

            var client = this.serviceProvider.GetService<IStoryApiClient>();
            var response = await client!.Fetch(3, CancellationToken.None);

            response.Should().HaveCount(3);
            _httpTest.ShouldHaveCalled(url);
        }

        [Test]
        public async Task Should_Return_BeststoriesIds_And_Handle_Request_Failure()
        {
            int[] ids = new[] { 4, 5, 6 };
            string url = $"{apiUrl}/beststories.json";
            _httpTest
                .ForCallsTo(url)
                .RespondWith("Request failed", 404)
                .RespondWithJson(ids);

            var client = this.serviceProvider.GetService<IStoryApiClient>();
            var response = await client!.Fetch(3, CancellationToken.None);

            response.Should().HaveCount(3);
            _httpTest.ShouldHaveCalled(url).Times(2);
        }
    }
}