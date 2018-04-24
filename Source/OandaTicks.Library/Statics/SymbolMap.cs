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

using OandaTicks.Common.Trading;
using OandaTicks.Common.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace OandaTicks.Library
{
    public static class SymbolMap
    {
        static SymbolMap()
        {
            var doc = XDocument.Parse(Properties.Resources.SymbolMap);

            var q = from s in doc.Element("symbolMap").Elements("map")
                    select new
                    {
                        Symbol = s.Attribute("symbol").Value.ToEnum<Symbol>(),
                        Instrument = s.Attribute("instrument").Value
                    };

            foreach (var map in q)
            {
                SymbolToInstrument.Add(map.Symbol, map.Instrument);
                InstrumentToSymbol.Add(map.Instrument, map.Symbol);
            }
        }

        public static Dictionary<Symbol, string> SymbolToInstrument { get; } =
            new Dictionary<Symbol, string>();

        public static Dictionary<string, Symbol> InstrumentToSymbol { get; } =
            new Dictionary<string, Symbol>();
    }
}
