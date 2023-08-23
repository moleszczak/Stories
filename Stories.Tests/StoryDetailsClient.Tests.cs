using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Stories.Configuration;
using Stories.Services;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FluentAssertions;
using Stories.Model;

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

			var response = await client.GetDetails(1, CancellationToken.None);

			response.Should().BeEquivalentTo(SampleStories[1]);
		}

		private static Dictionary<int, Story> SampleStories = new Dictionary<int, Story>
		{
			{ 
				1, 
				new Story() 
				{
					Id = 1,
					By = "XXX",
					Type = "story",
				} 
			},
		};
	}
}
