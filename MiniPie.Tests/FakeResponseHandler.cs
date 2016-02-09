using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MiniPie.Tests
{
    public class FakeResponseHandler : DelegatingHandler
    {
        private readonly Dictionary<Uri, KeyValuePair<HttpResponseMessage, int>> _fakeResponses =
            new Dictionary<Uri, KeyValuePair<HttpResponseMessage, int>>();

        public void AddFakeResponse(Uri uri, HttpResponseMessage responseMessage)
        {
            _fakeResponses.Add(uri, new KeyValuePair<HttpResponseMessage, int>(responseMessage, 0));
        }

        public void SetFakeResponse(Uri uri, HttpResponseMessage responseMessage)
        {
            _fakeResponses[uri] = new KeyValuePair<HttpResponseMessage, int>(responseMessage, 0);
        }

        public void CheckCallCount(Uri uri, int target)
        {
            var actualCount = _fakeResponses[uri].Value;
            Assert.Equal(actualCount, target);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            System.Threading.CancellationToken cancellationToken)
        {
            if (_fakeResponses.ContainsKey(request.RequestUri))
            {
                var response = _fakeResponses[request.RequestUri];
                response = new KeyValuePair<HttpResponseMessage, int>(response.Key, response.Value + 1);
                _fakeResponses[request.RequestUri] = response;
                return Task.FromResult(response.Key);
            }
            else
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) {RequestMessage = request});
            }
        }
    }
}