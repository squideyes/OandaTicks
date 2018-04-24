// Copyright 2017 Louis S. Berman.
//
// This file is part of OandaTicks.
//
// OandaTicks is free software: you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OandaTicks is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OandaTicks.  If not, see <http://www.gnu.org/licenses/>.

using NodaTime;
using System;

namespace OandaTicks.Common.Helpers
{
    public static class DateTimeHelpers
    {
        private static readonly DateTimeZone estZone =
            DateTimeZoneProviders.Tzdb.GetZoneOrNull("America/New_York");

        public static DateTime ToUtcFromEst(this DateTime value)
        {
            if (value.Kind != DateTimeKind.Unspecified)
                throw new ArgumentOutOfRangeException(nameof(value));

            return LocalDateTime.FromDateTime(value)
                .InZoneLeniently(estZone).ToDateTimeUtc();
        }

        public static DateTime ToEstFromUtc(this DateTime value)
        {
            if (value.Kind != DateTimeKind.Utc)
                throw new ArgumentOutOfRangeException(nameof(value));

            return Instant.FromDateTimeUtc(value)
                .InZone(estZone).ToDateTimeUnspecified();
        }
    }
}
