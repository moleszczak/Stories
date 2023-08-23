namespace Stories.Model
{
    public class StoryDto
    {
        public string? Title { get; set; }

        public string? By { get; set; }

        public int CommentsCount { get; set; }

        public int Score { get; set; }

        public int Time { get; set; }

        public string? Url { get; set; }

        public string? Type { get; set; }
    }
}
