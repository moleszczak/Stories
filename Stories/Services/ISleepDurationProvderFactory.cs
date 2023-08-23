namespace Stories.Services
{
    public interface ISleepDurationProvderFactory
    {
        Func<int, TimeSpan> ForItemRetyries();
    }
}
