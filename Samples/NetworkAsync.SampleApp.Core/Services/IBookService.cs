using System;

namespace NetworkAsync.SampleApp.Core.Services
{
    public interface IBooksService
    {
        void StartSearchAsync(string whatFor, Action<BookSearchResult> success, Action<Exception> error);
    }
}
