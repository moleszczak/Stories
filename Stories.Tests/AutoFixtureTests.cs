using FluentAssertions;
using Stories.Services;
using AutoFixture.NUnit3;
using Moq;
using Flurl.Http;

namespace Stories.Tests
{
    internal class AutoFixtureTests
    {
        [Test]
        [AutoDomainData]
        public async Task When_GetBestStories_Should_Get_Valid_Number_Of_StoryIds([Frozen] Mock<IApiClient<IEnumerable<int>>> apiClient, StoryApiClient srv, int[] beststoryIds)
        {
            var half = beststoryIds.Length / 2 + 1;

            apiClient
                .Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(beststoryIds);

            var x = await srv.FetchStories(half, CancellationToken.None);

            x.Should().HaveCountGreaterThanOrEqualTo(half);
            x.Should().BeEquivalentTo(beststoryIds.Take(half));
        }

        [Test]
        [AutoDomainData]
        public async Task When_GetBestStories_Fail_Should_Throw_Exception([Frozen] Mock<IApiClient<IEnumerable<int>>> apiClient, StoryApiClient srv)
        {
            apiClient
                .Setup(c => c.Get(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            var action = async () => await srv.FetchStories(1, CancellationToken.None);

            await action.Should().ThrowAsync<Exception>();
        }
    }
}
