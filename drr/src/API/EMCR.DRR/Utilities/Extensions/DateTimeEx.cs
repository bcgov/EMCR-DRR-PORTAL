namespace EMCR.DRR.API.Utilities.Extensions
{
    public static class DateTimeEx
    {
        private static string PSTTimeZoneName = GetPSTTimeZoneId();
        private static TimeZoneInfo PSTTimeZone = TimeZoneInfo.FindSystemTimeZoneById(PSTTimeZoneName);

        public static DateTime ToPST(this DateTime date)
        {
            if (date.Kind == DateTimeKind.Unspecified) date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date, GetPSTTimeZoneId());
        }

        public static DateTime FromUnspecifiedPstToUtc(this DateTime date)
        {
            //convert from Unspecified PST to UTC
            if (date.Kind != DateTimeKind.Unspecified) date = DateTime.SpecifyKind(date, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(date, GetPSTTimeZoneId(), GetUTCTimeZone());
        }

        public static TimeZoneInfo GetPstTimeZone() => PSTTimeZone;

        private static string GetPSTTimeZoneId() => Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => "Pacific Standard Time",
            PlatformID.Unix => "America/Vancouver",
            _ => throw new NotSupportedException()
        };

        private static string GetUTCTimeZone() => Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => "UTC",
            PlatformID.Unix => "Etc/UTC",
            _ => throw new NotSupportedException()
        };
    }
}
