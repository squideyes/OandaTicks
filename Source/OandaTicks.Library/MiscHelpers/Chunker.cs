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
using System.Linq;

namespace OandaTicks.Library
{
    internal class Chunker
    {
        private Dictionary<Symbol, Chunk> chunks;
        private HashSet<Symbol> skipFirsts;

        public event EventHandler<GenericArgs<Chunk>> OnChunk;

        public Chunker(List<Asset> assets)
        {
            chunks = assets.Select(a => a.Symbol)
                .ToDictionary(s => s, s => new Chunk(s));

            skipFirsts = new HashSet<Symbol>(assets.Select(a => a.Symbol));
        }

        public void HandleBidAsk(BidAsk bidAsk)
        {
            var chunk = chunks[bidAsk.Asset.Symbol];

            var chunkOn = bidAsk.TickOnEst.ToChunkOn();

            if (chunk.ChunkOn.HasValue && chunkOn != chunk.ChunkOn)
            {
                if (skipFirsts.Contains(bidAsk.Asset.Symbol))
                    skipFirsts.Remove(bidAsk.Asset.Symbol);
                else
                    OnChunk?.Invoke(this, new GenericArgs<Chunk>(chunk));

                chunk.Clear();

                chunk.ChunkOn = chunkOn;
            }

            chunks[bidAsk.Asset.Symbol].Add(bidAsk);
        }
    }
}
