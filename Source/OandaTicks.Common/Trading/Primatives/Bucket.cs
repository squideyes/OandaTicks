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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OandaTicks.Common.Trading
{
    public struct Bucket
    {
        internal Bucket(double price, long liquidity)
        {
            Price = price;
            Liquidity = liquidity;
        }

        public double Price { get; set; }
        public long Liquidity { get; set; }

        public bool IsValid(Asset asset) =>
            TryValidate(asset, new List<ValidationResult>());

        public void Validate(Asset asset)
        {
            var results = new List<ValidationResult>();

            if (!TryValidate(asset, results))
                throw new Common.Helpers.ValidationException(results);
        }

        public bool TryValidate(Asset asset, List<ValidationResult> results)
        {
            results.AddIfInvalid(Price > 0.0,
                nameof(Price), "a double that is greater than zero");

            results.AddIfInvalid(Price == asset.Round(Price),
                nameof(Price), $"a double rounded to {asset.Precision} digits");

            results.AddIfInvalid(Liquidity.InRange(1, int.MaxValue),
                nameof(Liquidity), "a positve integer");

            return results.Count == 0;
        }

        public override string ToString() => $"{Liquidity:N0} at {Price}";
    }
}
