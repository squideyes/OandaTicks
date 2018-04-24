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
using OandaTicks.Common.Trading;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OandaTicks.Library
{
    public class OandaClient
    {
        public class StatusArgs : EventArgs
        {
            internal StatusArgs(StreamKind kind, StreamStatus status)
            {
                Kind = kind;
                Status = status;
            }

            public StreamKind Kind { get; }
            public StreamStatus Status { get; }
        }

        public class HeartbeatArgs : EventArgs
        {
            internal HeartbeatArgs(StreamKind kind, DateTime dateTimeUtc)
            {
                Kind = kind;
                DateTimeUtc = dateTimeUtc;
            }

            public StreamKind Kind { get; }
            public DateTime DateTimeUtc { get; }
        }

        public class BidAskArgs : EventArgs
        {
            internal BidAskArgs(BidAsk bidAsk)
            {
                BidAsk = bidAsk;
            }

            public BidAsk BidAsk { get; }
        }

        public class BadPriceArgs
        {
            internal BadPriceArgs(string json, List<ValidationResult> results)
            {
                Json = json;
                Results = results;
            }

            public string Json { get; }
            public List<ValidationResult> Results { get; }

            public override string ToString() => $"{Results.Count} ValidationResults";
        }

        public class WebErrorArgs : EventArgs
        {
            internal WebErrorArgs(StreamKind kind,
                WebExceptionStatus status, string message)
            {
                Kind = kind;
                Status = status;
                Message = message;
            }

            public StreamKind Kind { get; }
            public WebExceptionStatus Status { get; }
            public string Message { get;  }
        }

        private PricingSession pricingSession = null;
        private DateTime? lastPricingHeartbeat = null;

        private Chunker chunker;
        private Context context;
        private HttpClient rest;

        public event EventHandler<HeartbeatArgs> OnHeartbeat;
        public event EventHandler<GenericArgs<Chunk>> OnChunk;
        public event EventHandler<BidAskArgs> OnBidAsk;
        public event EventHandler<StatusArgs> OnStatus;
        public event EventHandler<BadPriceArgs> OnBadPrice;
        public event EventHandler<WebErrorArgs> OnWebError;

        public OandaClient(Context context, int timeoutSeconds = 30)
        {
            this.context = context ??
                throw new ArgumentNullException(nameof(context));

            if (timeoutSeconds < 5)
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds));

            rest = HttpHelper.GetHttpClient(context, timeoutSeconds);
        }

        public async Task ConnectAsync(List<Symbol> symbols)
        {
            if (!symbols.HasElements())
                throw new ArgumentOutOfRangeException(nameof(symbols));

            chunker = new Chunker(
                symbols.Select(s => WellKnown.Assets[s]).ToList());

            chunker.OnChunk += OnChunk;

            InitPricingSession(symbols);

            await pricingSession.ConnectAsync();
        }

        public void Disconnect()
        {
            if (pricingSession != null)
            {
                pricingSession.Disconnect();

                pricingSession = null;
            }
        }

        private void HandleOnStatus(object sender, GenericArgs<StreamStatus> e)
        {
            var session = (StreamSession<StreamResponse>)sender;

            OnStatus?.Invoke(this, new StatusArgs(session.Kind, e.Data));

            if (e.Data == StreamStatus.WebError)
            {
                OnWebError?.Invoke(this, new WebErrorArgs(session.Kind,
                    session.ErrorStatus.Value, session.ErrorMessage));
            }
        }

        private void InitPricingSession(List<Symbol> symbols)
        {
            var assets = symbols.Select(s => WellKnown.Assets[s]).ToList();

            pricingSession = new PricingSession(context, assets);

            pricingSession.OnStatus += HandleOnStatus;

            pricingSession.OnData += (s, e) =>
            {
                UpdatePricingHeartbeat(e.Data.DateTimeUtc);

                switch (e.Data.Kind)
                {
                    case ResponseKind.BadPrice:
                        OnBadPrice?.Invoke(this, Lambda.Funcify((BadPriceResponse)e.Data,
                            d => new BadPriceArgs(d.Json, d.Results)));
                        break;
                    case ResponseKind.Price:
                        var bidAsk = ((PriceResponse)e.Data).BidAsk;
                        OnBidAsk?.Invoke(this, new BidAskArgs(bidAsk));
                        chunker.HandleBidAsk(bidAsk);
                        break;
                    case ResponseKind.Heartbeat:
                        OnHeartbeat?.Invoke(this, new HeartbeatArgs(
                            StreamKind.Pricing, e.Data.DateTimeUtc));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(e.Data.Kind));
                }
            };
        }

        private void UpdatePricingHeartbeat(DateTime heartbeat)
        {
            if (heartbeat > lastPricingHeartbeat)
                lastPricingHeartbeat = heartbeat;
        }

        public async Task UpdateAssetLimitsAsync() =>
            await AssetsRest.UpdateAssetLimitsAsync(context, rest);
    }
}
