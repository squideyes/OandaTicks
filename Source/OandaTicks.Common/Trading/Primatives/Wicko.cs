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

namespace OandaTicks.Common.Trading
{
    public class Wicko
    {
        public Asset Asset { get; internal set; }
        public DateTime OpenOn { get; internal set; }
        public DateTime CloseOn { get; internal set; }
        public double OpenPrice { get; internal set; }
        public double HighPrice { get; internal set; }
        public double LowPrice { get; internal set; }
        public double ClosePrice { get; internal set; }

        public Trend Trend
        {
            get
            {
                if (OpenPrice < ClosePrice)
                    return Trend.Rising;
                else if (OpenPrice > ClosePrice)
                    return Trend.Falling;
                else
                    return Trend.NoDelta;
            }
        }

        public double Spread =>
            Math.Abs(Asset.Round(ClosePrice - OpenPrice));
    }
}
