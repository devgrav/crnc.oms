using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Crnc.Oms.Sales.Application.Helpers
{
    public static class EnumHelper
    {
        public static string GetDescription<T>(T e) where T : IConvertible
        {
            if (e is Enum)
            {
                Type type = e.GetType();
                Array values = System.Enum.GetValues(type);

                foreach (int val in values)
                {
                    if (val == e.ToInt32(CultureInfo.InvariantCulture))
                    {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttribute = memInfo[0]
                            .GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() as DescriptionAttribute;

                        if (descriptionAttribute != null)
                        {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return string.Empty;
        }
        
        public static Dictionary<int, string> ToDictionaryWithKeysAndDescriptions<T>(T e) where T : Enum, IConvertible
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToDictionary(e => e.ToInt32(CultureInfo.InvariantCulture), GetDescription);
        }
    }
}
    