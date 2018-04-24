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
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace OandaTicks.Library
{
    internal abstract class StreamSession<T> where T : StreamResponse
    {
        private class JsonErrorResponse
        {
            [JsonProperty(PropertyName = "errorMessage")]
            public string Message { get; set; }
        }

        private WebResponse response;
        private WebRequest request;

        public event EventHandler<GenericArgs<T>> OnData;
        public event EventHandler<GenericArgs<StreamStatus>> OnStatus;

        protected StreamSession(StreamKind kind, Context context, Uri uri)
        {
            Kind = kind;

            Context = context ??
                throw new ArgumentNullException(nameof(context));

            request = WebRequest.Create(uri);

            request.Method = "GET";

            request.Headers[HttpRequestHeader.Authorization] =
                "Bearer " + context.AccessToken;
        }

        public StreamKind Kind { get; }

        protected Context Context { get; }

        public WebExceptionStatus? ErrorStatus { get; private set; }
        public string ErrorMessage { get; private set; }

        protected abstract T ParseJson(string json);

        private void RaiseStatus(StreamStatus status) =>
            OnStatus?.Invoke(this, new GenericArgs<StreamStatus>(status));

        public async Task ConnectAsync()
        {
            try
            {
                RaiseStatus(StreamStatus.Connecting);

                response = request.GetResponse();

                await Task.Factory.StartNew(() =>
                {
                    var stream = response.GetResponseStream();

                    using (var reader = new StreamReader(stream))
                    {
                        RaiseStatus(StreamStatus.Connected);

                        while (true)
                        {
                            OnData?.Invoke(this, new GenericArgs<T>(
                                ParseJson(reader.ReadLine())));
                        }
                    }
                });
            }
            catch (WebException error)
            {
                if (error.Status == WebExceptionStatus.RequestCanceled)
                    return;

                var message = error.Status.ToString();

                response = error.Response;

                if (response != null)
                {
                    var stream = response.GetResponseStream();

                    using (var reader = new StreamReader(stream))
                    {
                        var json = await reader.ReadToEndAsync();

                        message = JsonConvert.DeserializeObject<JsonErrorResponse>(json).Message;
                    }
                }

                ErrorStatus = error.Status;
                ErrorMessage = message;

                RaiseStatus(StreamStatus.WebError);
            }
            finally
            {
                RaiseStatus(StreamStatus.Disconnected);
            }
        }

        public void Disconnect()
        {
            RaiseStatus(StreamStatus.Disconnecting);

            if (request != null)
            {
                request.Abort();
                request = null;
            }

            if (response != null)
            {
                response.Close();
                response = null;
            }
        }
    }
}
