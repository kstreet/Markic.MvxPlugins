using System;

namespace NetworkAsync.SampleApp.Core.Services
{
    public class BooksService
        : IBooksService
    {
        private readonly ISimpleRestService _simpleRestService;

        public BooksService(ISimpleRestService simpleRestService)
        {
            _simpleRestService = simpleRestService;
        }

        public void StartSearchAsync(string whatFor, Action<BookSearchResult> success, Action<Exception> error)
        {
            // URL address to the service you are calling
            string address = string.Format("https://www.googleapis.com/books/v1/volumes?q={0}",
                                            Uri.EscapeDataString(whatFor));

            // can convert JSON returned from a REST service into C# with
            // http://json2csharp.com - paste all JSON results into it and get C# classes back
            // that is how VolumeInfo.cs and ImageLinks.cs were created
            _simpleRestService.MakeRequest<BookSearchResult>(address,
                "GET", success, error);
        }
    }
}
