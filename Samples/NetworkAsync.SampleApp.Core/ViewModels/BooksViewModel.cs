using System.Collections.Generic;
using Cirrious.MvvmCross.ViewModels;
using NetworkAsync.SampleApp.Core.Services;

namespace NetworkAsync.SampleApp.Core.ViewModels
{
    public class BooksViewModel 
        : MvxViewModel
    {
        // let's use our BookService that calls google with a search to execute
        private readonly IBooksService _booksService;

        // let IoC inject a BooksService for me
        public BooksViewModel(IBooksService booksService)
        {
            _booksService = booksService;
        }

        // a simple flag to tell us if we are ALREADY making a network request
        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; RaisePropertyChanged(() => IsBusy); }
        }

        // need a way to specifc a search term to send to Google books API
        private string _searchTerm;
        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                _searchTerm = value;
                RaisePropertyChanged(() => SearchTerm);
                Update();
            }
        }

        private void Update()
        {
            // a new search term has been set to let's ask google for results
            // set our public IsBusy property (to fire our bounded events)

            IsBusy = true;

            // use the Book service to search and when a result comes back
            // set the Results property above to the items in the result that came back
            // for rigth now we are ignoring errors
            _booksService.StartSearchAsync(SearchTerm,
                result =>
                {
                    IsBusy = false;
                    Results = result.items;
                },
                error =>
                {
                    IsBusy = false;
                });
        }

        // need a list to store the results in
        // we will just replace the whole list for now when results come in
        // will worry about ObservableCollections later
        private List<BookSearchItem> _results;
        public List<BookSearchItem> Results
        {
            get { return _results; }
            set { _results = value; RaisePropertyChanged(() => Results); }
        }
    }

}
