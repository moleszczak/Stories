namespace Stories.Services
{
    public delegate TimeSpan WaitDurationProvider(int retryNum);

    public delegate TimeSpan CacheExpirationProvider();
}
