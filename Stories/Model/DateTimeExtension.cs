namespace Stories.Model
{
    public static class DateTimeExtension
    {
        public static DateTime ToDateTime(this int unixTimestamp) 
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimestamp).ToLocalTime();
            return dateTime;
        }
    }
}
