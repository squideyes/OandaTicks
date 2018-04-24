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

using OandaTicks.Common.Helpers;
using System;
using System.Linq;
using System.Xml.Linq;

namespace OandaTicks.Common.Trading
{
    public class Asset
    {
        public class AssetLimits
        {
            public double MarginRate { get; internal set; }
            public MinMax<int> OrderUnits { get; internal set; }
            public MinMax<int> PositionUnits { get; internal set; }
        }

        private static XDocument doc = XDocument.Parse(Properties.Resources.Assets);

        public Asset(Symbol symbol)
        {
            if (!symbol.IsEnumValue())
                throw new ArgumentOutOfRangeException(nameof(symbol));

            var asset = (from a in doc.Element("assets").Elements("asset")
                         where a.Attribute("symbol").Value == symbol.ToString()
                         select new
                         {
                             Kind = a.Attribute("kind").Value.ToEnum<AssetKind>(),
                             Precision = (int)a.Attribute("precision"),
                             PipLocation = (int)a.Attribute("pipLocation"),
                             IsMajor = (bool)a.Attribute("isMajor"),
                             BaseCurrency = a.Attribute("baseCurrency").Value.ToEnum<Currency>(),
                             QuoteCurrency = a.Attribute("quoteCurrency").Value.ToEnum<Currency>()
                         })
                         .First();

            Symbol = symbol;
            Kind = asset.Kind;
            Precision = asset.Precision;
            PipLocation = asset.PipLocation;
            IsMajor = asset.IsMajor;
            BaseCurrency = asset.BaseCurrency;
            QuoteCurrency = asset.QuoteCurrency;

            Factor = Math.Pow(10.0, Precision - 1);
            TickValue = Math.Pow(10.0, -Precision);
        }

        public Symbol Symbol { get; }
        public AssetKind Kind { get; }
        public double TickValue { get; }
        public double Factor { get; }
        public int Precision { get; }
        public int PipLocation { get; }
        public bool IsMajor { get; }
        public Currency BaseCurrency { get; }
        public Currency QuoteCurrency { get; }

        public AssetLimits Limits { get; private set; }

        public double Round(double value) => Math.Round(value, Precision);

        public void SetLimits(
            double marginRate, int minUnits, int maxUnits, int maxPosition)
        {
            if (marginRate <= 0.0 || marginRate > 1.0)
                throw new ArgumentOutOfRangeException(nameof(marginRate));

            if (minUnits < 1)
                throw new ArgumentOutOfRangeException(nameof(minUnits));

            if (maxUnits < minUnits)
                throw new ArgumentOutOfRangeException(nameof(maxUnits));

            if (maxPosition < 0)
                throw new ArgumentOutOfRangeException(nameof(maxPosition));

            Limits = new AssetLimits()
            {
                MarginRate = marginRate,
                OrderUnits = new MinMax<int>(minUnits, maxUnits),
                PositionUnits = new MinMax<int>(0, maxPosition)
            };
        }

        public override string ToString() => Symbol.ToString();
    }
}
