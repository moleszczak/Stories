using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stories.Services;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;

namespace Stories.Tests
{
    internal class ApiClientWithCacheTests
    {
        private string apiUrl = "https://dummy.com";
        private HttpTest httpTest;
        private ServiceProvider serviceProvider;
        private IMemoryCache mockMemoryCache;

        [SetUp]
        public void Setup()
        {
            httpTest = new HttpTest();

            var services = new ServiceCollection();

            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            services.TryAddScoped(typeof(IApiClient<object>), typeof(ApiClient<object>));
            services.Decorate(typeof(IApiClient<>), typeof(ApiClientWithCache<>));

            services.TryAddSingleton<WaitDurationProvider>((attempt) => TimeSpan.FromMilliseconds(1));
            services.TryAddSingleton<CacheExpirationProvider>(() => TimeSpan.FromSeconds(10));

            mockMemoryCache = Substitute.For<IMemoryCache>();
            services.TryAddSingleton(mockMemoryCache);

            this.serviceProvider = services.BuildServiceProvider();
        }

        [TearDown]
        public void DisposeHttpTest()
        {
            httpTest.Dispose();
        }

        [Test]
        public async Task Request_Shouldnt_Be_Sent_And_Return_Data_From_Cache()
        {
            var dummy = "Dummy Object";
            httpTest
                .ForCallsTo(this.apiUrl)
                .RespondWithJson(dummy, 200);

            this.mockMemoryCache
                .TryGetValue(this.apiUrl, out Arg.Any<object>())
                .Returns(x =>
                {
                    x[1] = dummy;
                    return true;
                });

            var client = this.serviceProvider.GetService<IApiClient<object>>();

            var result = await client!.FetchBestStoriesIds(this.apiUrl, CancellationToken.None);

            result.Should().BeEquivalentTo(dummy);

            httpTest.ShouldNotHaveCalled(this.apiUrl);
        }

        [Test]
        public async Task Request_Should_Be_Sent_And_Key_Not_Found_In_Cache()
        {
            var dummy = "Dummy Object";
            httpTest
                .ForCallsTo(this.apiUrl)
                .RespondWithJson(dummy, 200);

            this.mockMemoryCache
                .TryGetValue(this.apiUrl, out Arg.Any<object>())
                .Returns(false);

            var client = this.serviceProvider.GetService<IApiClient<object>>();

            var result = await client!.FetchBestStoriesIds(this.apiUrl, CancellationToken.None);

            result.Should().BeEquivalentTo(dummy);

            httpTest.ShouldHaveCalled(this.apiUrl).Times(1);
            this.mockMemoryCache.Received().TryGetValue(this.apiUrl, out Arg.Any<object>());
        }
    }
}
