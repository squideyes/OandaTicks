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
using System.Text.RegularExpressions;
using OandaTicks.Common.Helpers;

namespace OandaTicks.Library
{
    public class Context
    {
        private Regex accountIdRegex =
            new Regex(@"\d{3}-\d{3}-\d{7}-\d{3}", RegexOptions.Compiled);

        public Context(Environ environ, string accountId, string accessToken)
        {
            if (!environ.IsEnumValue())
                throw new ArgumentOutOfRangeException(nameof(environ));

            if (string.IsNullOrWhiteSpace(accountId))
                throw new ArgumentOutOfRangeException(nameof(accountId));

            if (!accountIdRegex.IsMatch(accountId))
                throw new ArgumentOutOfRangeException(nameof(accountId));

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentOutOfRangeException(nameof(accessToken));

            if (accessToken.Length != 65)
                throw new ArgumentOutOfRangeException(nameof(accessToken));

            var guids = accessToken.Split('-');

            if (guids.Length != 2 || !guids[0].IsGuid() || !guids[1].IsGuid())
                throw new ArgumentOutOfRangeException(nameof(accessToken));

            Environ = environ;
            AccountId = accountId;
            AccessToken = accessToken;
        }

        public Environ Environ { get; }
        public string AccountId { get; }
        public string AccessToken { get; }
    }
}
