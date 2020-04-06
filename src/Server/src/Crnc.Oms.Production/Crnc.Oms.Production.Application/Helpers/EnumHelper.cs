using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Crnc.Oms.Production.Application.Helpers
{
    public static class EnumHelper
    {
        public static string GetDescription(Enum e)
        {
            if (e == null)
                return "";
            
            return
                e.GetType()
                .GetMember(e.ToString())
                .FirstOrDefault()
                ?.GetCustomAttribute<DescriptionAttribute>()
                ?.Description
                    ?? e.ToString();
        }
        
        public static Dictionary<int, string> ToDictionaryWithKeysAndDescriptions(Enum e)
        {
            var values = Enum.GetValues(e.GetType()).AsQueryable();
            var dictionary = new Dictionary<int,string>();
            
            foreach (var val in values)
            {
                var key = (int)val;
                var value = GetDescription((Enum)val);
                dictionary.Add(key, value);
            }

            return dictionary;
        }
        
        public static Dictionary<int, string> ToDictionaryWithKeysAndDescriptions(List<object> e)
        {
            var values = Enum.GetValues(e.GetType()).AsQueryable();
            var dictionary = new Dictionary<int,string>();
            
            foreach (var val in values)
            {
                var key = (int)val;
                var value = GetDescription((Enum)val);
                dictionary.Add(key, value);
            }

            return dictionary;
        }
    }
}
    