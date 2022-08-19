using System;
using System.Linq;
using System.Text.RegularExpressions;
using DirectoryComparer.Objects;

namespace DirectoryComparer.Services
{
    public static class ValueGetter
    {
        public static string GetValue(this CompareResult result, string fieldName)
        {
            var regex = new Regex(@"[ ]{1}", RegexOptions.None);
            fieldName = regex.Replace(fieldName, @"");
            var prop = typeof(CompareResult).GetProperties().ToList().SingleOrDefault(p => p.Name == fieldName);
            var returnVal = string.Empty;
            if (prop != null)
            {
                var val = prop.GetValue(result, null);
                var dt = DateTime.MinValue;

                if (val == null || (DateTime.TryParse(val.ToString(), out dt) && dt == DateTime.MinValue))
                    returnVal = string.Empty;
                else
                    returnVal = val.ToString();
            }

            return returnVal;
        }
    }
}