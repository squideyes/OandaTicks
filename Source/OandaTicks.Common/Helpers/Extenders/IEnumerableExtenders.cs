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
using System.Collections.Generic;
using System.Linq;

namespace OandaTicks.Common.Helpers
{
    public static class IEnumerableExtenders
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> items) =>
            (items == null) || (!items.Any());

        public static bool HasElements<T>(this IEnumerable<T> items, int minElements = 0, 
            int maxElements = int.MaxValue, Func<T, bool> isValid = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (minElements < 0)
                throw new ArgumentOutOfRangeException(nameof(minElements));

            if (maxElements < minElements)
                throw new ArgumentOutOfRangeException(nameof(maxElements));

            var count = items.Count();

            if (count < minElements || count > maxElements)
                return false;

            if (isValid != null)
            {
                foreach (var item in items)
                {
                    if (!isValid(item))
                        return false;
                }
            }

            return true;
        }
    }
}
