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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OandaTicks.Library
{
    public class Chunk : IEnumerable<BidAsk>
    {
        private List<BidAsk> bidAsks = new List<BidAsk>();

        internal Chunk(Symbol symbol)
        {
            Symbol = symbol;
        }

        public Symbol Symbol { get; }

        public DateTime? ChunkOn { get; internal set; }

        public int Count => bidAsks.Count;

        internal BidAsk this[int index] => bidAsks[index];

        public string FileName
        {
            get
            {
                var sb = new StringBuilder();

                sb.Append(BrokerKind.OANDA.ToString().ToUpper());
                sb.Append('_');
                sb.Append(Symbol);
                sb.Append("_RAW_");
                sb.Append(this[0].TickOnEst.ToString("yyyy_MM_dd_HH_mm"));
                sb.Append("_EST.csv");

                return sb.ToString();
            }
        }

        internal void Clear() => bidAsks.Clear();

        internal void Add(BidAsk bidAsk)
        {
            if (ChunkOn.HasValue && bidAsk.TickOnEst.ToChunkOn() != ChunkOn)
                throw new ArgumentOutOfRangeException(nameof(bidAsk));

            bidAsks.Add(bidAsk);

            if (!ChunkOn.HasValue)
                ChunkOn = bidAsk.TickOnEst.ToChunkOn();
        }

        private string GetCsv()
        {
            var sb = new StringBuilder();

            foreach (var bidAsk in bidAsks)
                sb.AppendLine(bidAsk.ToCsvString());

            return sb.ToString();
        }

        public string Save(string basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath))
                throw new ArgumentOutOfRangeException(nameof(basePath));

            if(basePath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                throw new ArgumentOutOfRangeException(nameof(basePath));

            var sb = new StringBuilder();

            var chunkOn = this[0].TickOnEst.ToChunkOn();

            sb.Append("RawChunks"); 
            sb.Append(Path.DirectorySeparatorChar);
            sb.Append(Symbol);
            sb.Append(Path.DirectorySeparatorChar);
            sb.Append(chunkOn.Year);
            sb.Append(Path.DirectorySeparatorChar);
            sb.Append(chunkOn.Month.ToString("00"));
            sb.Append(Path.DirectorySeparatorChar);
            sb.Append(chunkOn.Day.ToString("00"));
            sb.Append(Path.DirectorySeparatorChar);
            sb.Append(FileName);

            var fileName = Path.Combine(basePath, sb.ToString());

            fileName.EnsurePathExists();

            using (var writer = new StreamWriter(fileName))
                writer.Write(GetCsv());

            return fileName;
        }

        public override string ToString() => FileName;

        public IEnumerator<BidAsk> GetEnumerator() => bidAsks.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
