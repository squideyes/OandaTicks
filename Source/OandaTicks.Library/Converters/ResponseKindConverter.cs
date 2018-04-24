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
using System;
using System.Reflection;

namespace OandaTicks.Library
{
    public class ReponseKindConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
            throw new NotImplementedException();

        public override bool CanConvert(Type objectType) =>
            typeof(ResponseKind).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            switch (reader.Value)
            {
                case "HEARTBEAT":
                    return ResponseKind.Heartbeat;
                case "PRICE":
                    return ResponseKind.Price;
                default:
                    throw new ArgumentOutOfRangeException("Kind");
            }
        }
    }
}
