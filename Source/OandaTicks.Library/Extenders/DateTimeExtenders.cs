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

using System;

namespace OandaTicks.Library
{
    public static class DateTimeExtenders
    {
        private static readonly long oneMinuteTicks =
            TimeSpan.FromMinutes(1).Ticks;

        internal static DateTime ToChunkOn(this DateTime value) =>
            new DateTime(value.Ticks - (value.Ticks % oneMinuteTicks), value.Kind);

        internal static bool IsTickOnEst(this DateTime value)
        {
            if (value.Kind != DateTimeKind.Unspecified)
                throw new ArgumentOutOfRangeException(nameof(value));

            switch (value.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    return value.Hour < 17;
                case DayOfWeek.Saturday:
                    return false;
                case DayOfWeek.Sunday:
                    return value.Hour >= 17;
                default:
                    return true;
            }
        }
    }
}
