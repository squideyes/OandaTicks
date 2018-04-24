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

using Newtonsoft.Json;
using OandaTicks.Common.Helpers;
using OandaTicks.Common.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OandaTicks.Library
{
    internal class AssetsRest
    {
        private enum InstrumentKind
        {
            Currency = 1,
            CFD = 2,
            Metal = 3
        }

        private class JsonInstrument
        {
            [JsonProperty(PropertyName = "name")]
            public string Symbol { get; set; }

            [JsonProperty(PropertyName = "type")]
            public InstrumentKind Kind { get; set; }

            [JsonProperty(PropertyName = "displayPrecision")]
            public int Precision { get; set; }

            [JsonProperty(PropertyName = "pipLocation")]
            public int PipLocation { get; set; }

            [JsonProperty(PropertyName = "marginRate")]
            public double MarginRate { get; set; }

            [JsonProperty(PropertyName = "minimumTradeSize")]
            public int MinOrderUnits { get; set; }

            [JsonProperty(PropertyName = "maximumOrderUnits")]
            public int MaxOrderUnits { get; set; }

            [JsonProperty(PropertyName = "maximumPositionSize")]
            public int MaxPositionUnits { get; set; }
        }

        private class JsonInstruments
        {
            [JsonProperty(PropertyName = "instruments")]
            public List<JsonInstrument> Instruments { get; set; }
        }

        private static AssetKind ToAssetKind(InstrumentKind kind)
        {
            switch (kind)
            {
                case InstrumentKind.Currency:
                    return AssetKind.Forex;
                case InstrumentKind.CFD:
                    return AssetKind.CFD;
                case InstrumentKind.Metal:
                    return AssetKind.Metal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        public static async Task UpdateAssetLimitsAsync(Context context, HttpClient rest)
        {
            var symbols = string.Join(",", WellKnown.Assets.Keys.Select(
                s => SymbolMap.SymbolToInstrument[s]));

            var data = await context.GetAsync<JsonInstruments>(
                rest, $"instruments?instruments={symbols}");

            var q = from i in data.Instruments
                    select new
                    {
                        Symbol = i.Symbol.Replace("_", "").ToEnum<Symbol>(),
                        MarginRate = i.MarginRate,
                        MinOrderUnits = i.MinOrderUnits,
                        MaxOrderUnits = i.MaxOrderUnits,
                        MaxPositionUnits = i.MaxPositionUnits,
                        Kind = i.Kind,
                        Precision = i.Precision,
                        PipLocation = i.PipLocation
                    };

            var instruments = q.ToDictionary(i => i.Symbol);

            foreach (var asset in WellKnown.Assets.Values)
            {
                var i = instruments[asset.Symbol];

                if (asset.Kind != ToAssetKind(i.Kind))
                    throw new ArgumentOutOfRangeException(nameof(i.Kind));

                if (asset.Precision != i.Precision)
                    throw new ArgumentOutOfRangeException(nameof(i.Precision));

                if (asset.PipLocation != i.PipLocation)
                    throw new ArgumentOutOfRangeException(nameof(i.PipLocation));

                asset.SetLimits(i.MarginRate, i.MinOrderUnits,
                    i.MaxOrderUnits, i.MaxPositionUnits);
            }
        }
    }
}
