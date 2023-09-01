namespace Stories.Model
{
    public class StoryDto
    {
        public string? Title { get; set; }

        public string? PostedBy { get; set; }

        public int CommentsCount { get; set; }

        public int Score { get; set; }

        public DateTime Time { get; set; }

        public string? Uri { get; set; }
    }
}
