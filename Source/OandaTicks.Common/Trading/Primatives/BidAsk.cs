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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OandaTicks.Common.Trading
{
    public class BidAsk
    {
        public Asset Asset { get; set; }
        public bool IsTradable { get; set; }
        public DateTime TickOnEst { get; set; }
        public Bucket Bid { get; set; }
        public Bucket Ask { get; set; }
        public double MidPrice { get; set; }

        public bool IsValid() => TryValidate(new List<ValidationResult>());

        public void Validate(Asset asset)
        {
            var results = new List<ValidationResult>();

            if (!TryValidate(results))
                throw new Common.Helpers.ValidationException(results);
        }

        public bool TryValidate(List<ValidationResult> results)
        {
            results.AddIfInvalid(Asset != null,
                nameof(Asset), "a non-null Asset");

            results.AddIfInvalid(TickOnEst.Kind == DateTimeKind.Unspecified,
                nameof(TickOnEst), "an EST (unspecified) date/time");

            Bid.TryValidate(Asset, results);

            Ask.TryValidate(Asset, results);

            return results.Count == 0;
        }

        public override string ToString() =>
            $"{Asset}, {TickOnEst.ToText()}, Bid: {Bid}, Ask: {Ask}";

        public string ToCsvString() =>
            $"{Asset},{TickOnEst.ToText()},{Bid.Price},{Ask.Price}";
    }
}
