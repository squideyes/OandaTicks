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

namespace OandaTicks.Common.Helpers
{
    public static class IComparableExtenders
    {
        public static bool InRange<T>(this T value, T minValue, T maxValue, bool inclusive = true) 
            where T : IComparable<T>
        {
            if (inclusive)
            {
                if (value.CompareTo(minValue) < 0 || value.CompareTo(maxValue) > 0)
                    return false;
            }
            else
            {
                if (value.CompareTo(minValue) <= 0 || value.CompareTo(maxValue) >= 0)
                    return false;
            }

            return true;
        }
    }
}
