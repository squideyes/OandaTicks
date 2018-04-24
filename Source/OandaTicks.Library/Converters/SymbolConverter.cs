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

namespace OandaTicks.Library
{
    public class SymbolConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType == typeof(string);

        public override void WriteJson(
            JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(SymbolMap.SymbolToInstrument[(Symbol)value]);
        }

        public override object ReadJson(JsonReader reader, 
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ((string)reader.Value).ToEnum<Symbol>();
        }
    }
}
