using System;
using System.Threading.Tasks;

namespace NetworkAsync.SampleApp.Core.Services
{
    public interface IBooksService
    {
        Task<BookSearchResult> StartSearchAsync(string whatFor);
    }
}
