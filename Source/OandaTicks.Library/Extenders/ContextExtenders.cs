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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace OandaTicks.Library
{
    internal static class ContextExtenders
    {
        private class RequestError
        {
            [JsonProperty(PropertyName = "errorCode")]
            public string Code { get; set; }

            [JsonProperty(PropertyName = "errorMessage")]
            public string Message { get; set; }
        }

        public static Uri GetRestUri(
            this Context context, string format, params object[] args)
        {
            var baseUri = new Uri(context.GetBaseUri(Endpoint.Rest),
                $"accounts/{WebUtility.UrlEncode(context.AccountId)}/");

            return new Uri(baseUri, string.Format(format, args));
        }

        public static Uri GetStreamUri(
            this Context context, string format, params object[] args)
        {
            var baseUri = new Uri(context.GetBaseUri(Endpoint.Stream),
                $"accounts/{WebUtility.UrlEncode(context.AccountId)}/");

            return new Uri(baseUri, string.Format(format, args));
        }

        private static Uri GetBaseUri(this Context context, Endpoint endpoint)
        { 
            switch (context.Environ)
            {
                case Environ.Practice:
                    switch (endpoint)
                    {
                        case Endpoint.Rest:
                            return new Uri("https://api-fxpractice.oanda.com/v3/");
                        case Endpoint.Stream:
                            return new Uri("https://stream-fxpractice.oanda.com/v3/");
                        default:
                            throw new ArgumentOutOfRangeException(nameof(endpoint));
                    }
                case Environ.Trade:
                    switch (endpoint)
                    {
                        case Endpoint.Rest:
                            return new Uri("https://api-fxtrade.oanda.com/v3/");
                        case Endpoint.Stream:
                            return new Uri("https://stream-fxtrade.oanda.com/v3/");
                        default:
                            throw new ArgumentOutOfRangeException(nameof(endpoint));
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(context));
            }
        }

        public static async Task<T> GetAsync<T>(
            this Context context, HttpClient client, string format, params object[] args)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (context == null)
                throw new ArgumentNullException(nameof(client));

            if (string.IsNullOrWhiteSpace(format))
                throw new ArgumentOutOfRangeException(nameof(format));

            var uri = context.GetRestUri(format, args);

            var response = await client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(
                    await response.Content.ReadAsStringAsync());
            }
            else
            {
                var error = JsonConvert.DeserializeObject<RequestError>(
                    await response.Content.ReadAsStringAsync());

                throw new RequestException(error.Code, error.Message);
            }
        }
    }
}
