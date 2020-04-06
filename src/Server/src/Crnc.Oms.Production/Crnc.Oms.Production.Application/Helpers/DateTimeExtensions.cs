using System;

namespace Crnc.Oms.Production.Application.Helpers
{
    public static class DateTimeExtensions
    {
        public static string ToStandartFormat(this DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyy");
        }
        
        public static string ToStandartFormatWithTime(this DateTime dateTime)
        {
            return dateTime.ToString("dd.MM.yyy hh:mm:ss");
        }
        
        public static string ToStandartFormat(this DateTime? dateTime)
        {
            return dateTime?.ToString("dd.MM.yyy") ?? "";
        }
        
        public static string ToStandartFormatWithTime(this DateTime? dateTime)
        {
            return dateTime?.ToString("dd.MM.yyy hh:mm:ss") ?? "";
        }
    }
}