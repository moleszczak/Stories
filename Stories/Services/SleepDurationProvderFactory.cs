namespace Stories.Services
{
    public delegate TimeSpan SleepDurationProvider(int retryNum);

    public class SleepDurationProvderFactory : ISleepDurationProvderFactory
    {
        public Func<int, TimeSpan> ForItemRetyries()
        {
            return (n) => TimeSpan.FromSeconds(Math.Pow(2, n));
        }
    }
}
