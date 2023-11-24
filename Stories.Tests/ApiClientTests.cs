using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stories.Services;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Flurl.Http;

namespace Stories.Tests
{

    internal class ApiClientTests
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

            services.TryAddScoped(typeof(IApiClient<>), typeof(ApiClient<>));

            services.TryAddSingleton<WaitDurationProvider>((attempt) => TimeSpan.FromMilliseconds(1));

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
        public void Service_Should_Be_Created()
        {
            var client = this.serviceProvider.GetService<IApiClient<object>>();

            client.Should().NotBeNull();
        }

        [Test]
        public async Task Request_Should_Be_Sent()
        {
            var dummy = "Dummy Object";
            httpTest.ForCallsTo(this.apiUrl).RespondWithJson(dummy, 200);
            var client = this.serviceProvider.GetService<IApiClient<object>>();

            var result = await client!.Get(this.apiUrl, CancellationToken.None);

            result.Should().BeEquivalentTo(dummy);
            httpTest.ShouldHaveCalled(this.apiUrl);
        }

        [Test]
        public async Task Request_Should_Be_Retried_When_Failed()
        {
            var dummy = "Dummy Object";
            httpTest
                .ForCallsTo(this.apiUrl)
                .RespondWith("failure", 404)
                .RespondWithJson(dummy, 200);
            var client = this.serviceProvider.GetService<IApiClient<object>>();

            var result = await client!.Get(this.apiUrl, CancellationToken.None);

            result.Should().BeEquivalentTo(dummy);
            httpTest.ShouldHaveCalled(this.apiUrl).Times(2);
        }

        [Test]
        public async Task Request_Should_Fail_After_3_Attempts()
        {
            var dummy = "Dummy Object";
            httpTest
                .ForCallsTo(this.apiUrl)
                .RespondWithJson(dummy, 200);

            var client = this.serviceProvider.GetService<IApiClient<object>>();

            var result = await client!.Get($"{this.apiUrl}?xxx=s&yyy=z", CancellationToken.None);

            result.Should().BeEquivalentTo(dummy);

            httpTest.ShouldHaveCalled(this.apiUrl).Times(1);
        }

    }
}
