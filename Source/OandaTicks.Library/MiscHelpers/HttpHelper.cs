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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace OandaTicks.Library
{
    internal static class HttpHelper
    {
        public static HttpClient GetHttpClient(Context context, int timeoutSeconds)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var handler = new HttpClientHandler()
            {
                AutomaticDecompression =
                    DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var client = new HttpClient(handler);

            client.DefaultRequestHeaders.Add("X-Accept-Datetime-Format", "RFC3339");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", context.AccessToken);

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

            return client;
        }
    }
}
