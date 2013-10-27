using System;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Plugins.NetworkAsync.Rest;

namespace NetworkAsync.SampleApp.Core.Services
{
    public class BooksService
        : IBooksService
    {
        private readonly IMvxJsonRestClient _jsonRestClient;

        public BooksService(IMvxJsonRestClient jsonRestClient)
        {
            _jsonRestClient = jsonRestClient;
        }

        public async Task<BookSearchResult> StartSearchAsync(string whatFor)
        {
            // URL address to the service you are calling
            var route = string.Format("https://www.googleapis.com/books/v1/volumes?q={0}",
                                                Uri.EscapeDataString(whatFor));

            var ct = new CancellationToken();

            var response = await ReceiveAsync<BookSearchResult>(route, MvxVerbs.Get, ct);

            return response.Result;

        }
        private Task<MvxDecodedRestResponse<TResult>> ReceiveAsync<TResult>(string url, string verb, CancellationToken ct)
        {
            var request = new MvxJsonRestRequest<object>(url, verb);
            
            // TODO: For a "GET" request I had to make sure to wipe out the ContentType header with null
            // TODO: or else I would get an exception:
            // System.Net.ProtocolViolationException: A request with this method cannot have a request body.
            // This Exception happened in MvxRestClient.ProcessResponseTextAsync in its var task =
            // more info here: http://stackoverflow.com/questions/2480258/why-am-i-getting-protocolviolationexception-when-trying-to-use-httpwebrequest
            
            request.ContentType = null;

            return _jsonRestClient.MakeRequestForAsync<TResult>(request, ct);
        }
    }
}
