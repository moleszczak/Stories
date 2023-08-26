﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stories.Configuration;
using Stories.Services;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using Stories.Model;
using Microsoft.Extensions.Caching.Memory;

namespace Stories.Tests
{
    public class StoryDetailsClientTests
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

            services.TryAddSingleton((sp) => mockConfiguration);
            services.TryAddScoped<IStoryDetailsApiClient, StoryDetailsApiClient>();
            services.TryAddSingleton<WaitDurationProvider>((attempt) => TimeSpan.FromMilliseconds(1));
            services.TryAddSingleton<CacheExpirationProvider>(() => TimeSpan.FromSeconds(10));

            services.TryAddScoped(typeof(IApiClient<Story>), typeof(ApiClient<Story>));
            services.TryDecorate(typeof(IApiClient<Story>), typeof(ApiClientWithCache<Story>));

            var mockMemoryCache = Substitute.For<IMemoryCache>();
            services.TryAddSingleton(mockMemoryCache);

            services.AddAutoMapper(typeof(MappingConfiguration));

            this.serviceProvider = services.BuildServiceProvider();
        }

        [TearDown]
        public void DisposeHttpTest()
        {
            _httpTest.Dispose();
        }

        [Test]
        public async Task Should_Return_StoryDetails_Success()
        {
            string urlPattern = $"{this.apiUrl}/item/*";

            _httpTest
                .ForCallsTo(urlPattern)
                .RespondWithJson(SampleStories[1]);

            var client = this.serviceProvider.GetRequiredService<IStoryDetailsApiClient>();

            var response = await client.GetDetails(new[] { 1 }, CancellationToken.None).ToListAsync();

            response.Should().BeEquivalentTo(new[] { SampleStoriesDto[1] });
        }

        [Test]
        public async Task Should_Return_StoryDetails_After3Retries()
        {
            string urlPattern = $"{this.apiUrl}/item/*";

            _httpTest
                .ForCallsTo(urlPattern)
                .RespondWith("Client failure", 404)
                .RespondWithJson(SampleStories[1])
                .RespondWith("Server failure", 500)
                .RespondWithJson(SampleStories[2]);

            var client = this.serviceProvider.GetRequiredService<IStoryDetailsApiClient>();

            var response = await client.GetDetails(new[] { 1, 2 }, CancellationToken.None).ToListAsync();

            response.Should().BeEquivalentTo(SampleStoriesDto.Values);

            _httpTest.ShouldHaveCalled(urlPattern).Times(4);
        }


        private static Dictionary<int, Story> SampleStories = new Dictionary<int, Story>
        {
            {
                1,
                new Story()
                {
                    Id = 1,
                    Title = "story 1",
                    By = "XXX",
                    Type = "story",
                }
            },
            {
                2,
                new Story()
                {
                    Id = 2,
                    Title = "story 2",
                    By = "YYY",
                    Type = "story",
                    Kids = new[] { 10001, 10002, 10003 },
                }
            },
        };

        private static Dictionary<int, StoryDto> SampleStoriesDto = new Dictionary<int, StoryDto>
        {
            {
                1,
                new StoryDto()
                {
                    Title = "story 1",
                    By = "XXX",
                    Type = "story",
                    CommentsCount = 0,
                }
            },
            {
                2,
                new StoryDto()
                {
                    Title = "story 2",
                    By = "YYY",
                    Type = "story",
                    CommentsCount = 3,
                }
            },
        };
    }
}
