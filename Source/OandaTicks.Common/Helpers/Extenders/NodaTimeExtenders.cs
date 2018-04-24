// This file is part of OandaV20.

// OandaV20 is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// OandaV20 is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with OandaV20.  If not, see <http://www.gnu.org/licenses/>.

// This file is part of OandaV20.

// OandaV20 is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// OandaV20 is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with OandaV20.  If not, see <http://www.gnu.org/licenses/>.
using NodaTime;
using System;

namespace OandaV20.Common.Helpers
{
    public static class DateTimeHelpers
    {
        private static readonly DateTimeZone eastern =
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("America/New_York");

        private static readonly DateTimeZone utcTimeZone =
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("UTC");

        public static DateTime ToUtcFromEastern(this DateTime value)
        {
            if (value.Kind != DateTimeKind.Unspecified)
                throw new ArgumentOutOfRangeException(nameof(value));

            return LocalDateTime.FromDateTime(value)
                .InZoneLeniently(eastern).ToDateTimeUtc();
        }

        public static DateTime ToEasternFromUtc(this DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
                throw new ArgumentOutOfRangeException(nameof(value));

            return Instant.FromDateTimeUtc(value)
                .InZone(eastern).ToDateTimeUnspecified();
        }
    }
}
