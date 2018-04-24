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
    public class WickoFeed
    {
        public class WickoArgs : EventArgs
        {
            public WickoArgs(Wicko wicko)
            {
                Wicko = wicko;
            }

            public Wicko Wicko { get; }
        }

        private bool firstTick = true;
        private Wicko lastWicko = null;

        private DateTime openOn;
        private DateTime closeOn;
        private double openPrice;
        private double highPrice;
        private double lowPrice;
        private double closePrice;
        private double wickoSize;
        private PriceToUse rateToUse;

        public event EventHandler<WickoArgs> OnWicko;

        public WickoFeed(Asset asset, double wickoPips)
        {
            Asset = asset ?? throw new ArgumentNullException(nameof(asset));

            wickoSize = wickoPips.PipsToPrice(asset);
        }

        public Asset Asset { get; }

        private Wicko GetNewWicko(
            double openPrice, double highPrice, double lowPrice, double closePrice)
        {
            return new Wicko()
            {
                Asset = Asset,
                OpenOn = openOn,
                CloseOn = closeOn,
                OpenPrice = openPrice,
                HighPrice = highPrice,
                LowPrice = lowPrice,
                ClosePrice = closePrice
            };
        }

        private void Rising()
        {
            double limit;

            while (closePrice > (limit = Asset.Round(openPrice + wickoSize)))
            {
                var wicko = GetNewWicko(
                    openPrice, limit, lowPrice, limit);

                lastWicko = wicko;

                OnWicko?.Invoke(this, new WickoArgs(wicko));

                openOn = closeOn;
                openPrice = limit;
                lowPrice = limit;
            }
        }

        private void Falling()
        {
            double limit;

            while (closePrice < (limit = Asset.Round(openPrice - wickoSize)))
            {
                var wicko = GetNewWicko(
                    openPrice, highPrice, limit, limit);

                lastWicko = wicko;

                OnWicko?.Invoke(this, new WickoArgs(wicko));

                openOn = closeOn;
                openPrice = limit;
                highPrice = limit;
            }
        }

        internal Wicko OpenWicko
        {
            get
            {
                return GetNewWicko(
                    openPrice, highPrice, lowPrice, closePrice);
            }
        }

        public void HandleTick(BidAsk bidAsk)
        {
            if (firstTick)
            {
                firstTick = false;

                openOn = bidAsk.TickOnEst;
                closeOn = bidAsk.TickOnEst;
                openPrice = bidAsk.MidPrice;
                highPrice = bidAsk.MidPrice;
                lowPrice = bidAsk.MidPrice;
                closePrice = bidAsk.MidPrice;
            }
            else
            {
                closeOn = bidAsk.TickOnEst;

                if (bidAsk.MidPrice> highPrice)
                    highPrice = bidAsk.MidPrice;

                if (bidAsk.MidPrice < lowPrice)
                    lowPrice = bidAsk.MidPrice;

                closePrice = bidAsk.MidPrice;

                if (closePrice > openPrice)
                {
                    if (lastWicko == null ||
                        (lastWicko.Trend == Trend.Rising))
                    {
                        Rising();

                        return;
                    }

                    var limit = Asset.Round(lastWicko.OpenPrice + wickoSize);

                    if (closePrice > limit)
                    {
                        var wicko = GetNewWicko(
                            lastWicko.OpenPrice, limit, lowPrice, limit);

                        lastWicko = wicko;

                        OnWicko?.Invoke(this, new WickoArgs(wicko));

                        openOn = closeOn;
                        openPrice = limit;
                        lowPrice = limit;

                        Rising();
                    }
                }
                else if (closePrice < openPrice)
                {
                    if (lastWicko == null ||
                        (lastWicko.Trend == Trend.Falling))
                    {
                        Falling();

                        return;
                    }

                    var limit = Asset.Round(lastWicko.OpenPrice - wickoSize);

                    if (closePrice < limit)
                    {
                        var wicko = GetNewWicko(
                            lastWicko.OpenPrice, highPrice, limit, limit);

                        lastWicko = wicko;

                        OnWicko?.Invoke(this, new WickoArgs(wicko));

                        openOn = closeOn;
                        openPrice = limit;
                        highPrice = limit;

                        Falling();
                    }
                }
            }
        }
    }
}
