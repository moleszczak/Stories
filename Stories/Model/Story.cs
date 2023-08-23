namespace Stories.Model
{
    public class Story
    {
        public string? By { get; set; }

        public int Id { get; set; }

        public int[]? Kids { get; set; }

        public int Score { get; set; }

        public int Time { get; set; }

        public string? Title { get; set; }

        public string? Url { get; set; }

        public string? Type { get; set; }
    }
}
