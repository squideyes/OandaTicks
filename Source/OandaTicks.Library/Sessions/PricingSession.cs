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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;

namespace OandaTicks.Library
{
    internal class PricingSession : StreamSession<StreamResponse>
    {
        private enum JsonPriceKind
        {
            Price,
            Heartbeat
        }

        private class JsonPriceBucket
        {
            [JsonProperty(PropertyName = "price")]
            public double Price { get; set; }

            [JsonProperty(PropertyName = "liquidity")]
            public int Liquidity { get; set; }
        }

        private class JsonPrice
        {
            [JsonProperty(PropertyName = "type")]
            public JsonPriceKind Kind { get; set; }

            [JsonProperty(PropertyName = "time")]
            public DateTime DateTimeUtc { get; set; }

            [JsonProperty(PropertyName = "tradeable")]
            public bool IsTradeable { get; set; }

            [JsonProperty(PropertyName = "instrument")]
            public string BrokerSymbol { get; set; }

            [JsonProperty(PropertyName = "bids")]
            public List<JsonPriceBucket> Bids { get; set; }

            [JsonProperty(PropertyName = "asks")]
            public List<JsonPriceBucket> Asks { get; set; }
        }

        public PricingSession(Context context, List<Asset> assets)
            : base(StreamKind.Pricing, context, GetUri(context, assets))
        {
        }

        protected override StreamResponse ParseJson(string json)
        {
            var data = JsonConvert.DeserializeObject<JsonPrice>(json);

            if (data.Kind == JsonPriceKind.Price
                && data.Bids.Count > 0 && data.Asks.Count > 0)
            {
                var tickOnEst = data.DateTimeUtc.ToEstFromUtc();

                if (!tickOnEst.IsTickOnEst())
                    return GetHeartbeatResponse(data.DateTimeUtc);

                var bidAsk = new BidAsk()
                {
                    TickOnEst = tickOnEst,
                    Asset = WellKnown.Assets[
                        SymbolMap.InstrumentToSymbol[data.BrokerSymbol]],
                    IsTradable = data.IsTradeable,
                    Bid = new Bucket()
                    {
                        Price = data.Bids[0].Price,
                        Liquidity = data.Bids[0].Liquidity
                    },
                    Ask = new Bucket()
                    {
                        Price = data.Asks[0].Price,
                        Liquidity = data.Asks[0].Liquidity
                    }
                };

                var results = new List<ValidationResult>();

                if (bidAsk.TryValidate(results))
                {
                    return new PriceResponse()
                    {
                        Kind = ResponseKind.Price,
                        DateTimeUtc = data.DateTimeUtc,
                        BidAsk = bidAsk
                    };
                }
                else
                {
                    return new BadPriceResponse()
                    {
                        Kind = ResponseKind.BadPrice,
                        DateTimeUtc = data.DateTimeUtc,
                        Json = json,
                        Results = results
                    };
                }
            }
            else if(data.Kind != JsonPriceKind.Heartbeat)
            {
            }

            return GetHeartbeatResponse(data.DateTimeUtc);
        }

        private HeartbeatResponse GetHeartbeatResponse(DateTime dateTimeUtc)
        {
            return new HeartbeatResponse()
            {
                Kind = ResponseKind.Heartbeat,
                DateTimeUtc = dateTimeUtc
            };
        }

        private static Uri GetUri(Context context, List<Asset> assets)
        {
            if (!assets.HasElements())
                throw new ArgumentOutOfRangeException(nameof(assets));

            var symbols = WebUtility.UrlEncode(string.Join(",",
                assets.Select(a => SymbolMap.SymbolToInstrument[a.Symbol])));

            return context.GetStreamUri($"pricing/stream?instruments={symbols}");
        }
    }
}
