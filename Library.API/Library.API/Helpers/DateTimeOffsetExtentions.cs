using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Helpers
{
    public static class DateTimeOffsetExtentions
    {
        public static int GetCurrentAge(this DateTimeOffset dateTimeOffset)
        {
            var currentDate = DateTime.UtcNow;
            int age = currentDate.Year - dateTimeOffset.Year;

            if (currentDate < dateTimeOffset.AddYears(age))
                age--;

            return age;
        }

        public static int GetCurrentAge(this DateTimeOffset dateTimeOffset, DateTimeOffset? dateOfDeath)
        {
            var currentDate = DateTime.UtcNow;

            if (dateOfDeath != null)
                currentDate = dateOfDeath.Value.UtcDateTime;

            int age = currentDate.Year - dateTimeOffset.Year;

            if (currentDate < dateTimeOffset.AddYears(age))
                age--;

            return age;
        }
    }
}
