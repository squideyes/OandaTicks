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
using System.Linq;
using System.Runtime.Serialization;

namespace OandaTicks.Library
{
    public static class EnumExtenders
    {      
        public static Enum ToEnum(this Enum enumeration, string value)
        {
            var enumType = enumeration.GetType();
     
            var found = enumType.GetMembers()

            .Select(x => new
            {
                Member = x,
                Attribute = x.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                    .OfType<EnumMemberAttribute>().FirstOrDefault()
            })
            .FirstOrDefault(x => x.Attribute?.Value == value);

            if (found != null)
                enumeration = (Enum)Enum.Parse(enumType, found.Member.Name);

            return enumeration;
        }

        public static string ToJson(this Enum enumeration)
        {
            var enumType = enumeration.GetType();
            
            var info = enumType.GetMember(enumeration.ToString());

            var attributes = info[0].GetCustomAttributes(
                typeof(EnumMemberAttribute), false);

            return ((EnumMemberAttribute)attributes[0]).Value;
        }
    }
}
