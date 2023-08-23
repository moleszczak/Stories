namespace Stories.Configuration
{
    public class StoriesApiConfiguration : IStoriesApiConfiguration
    {
        public const string SectionName = "StoriesApi";

        public string? Url { get; set; }
    }
}
