using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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

        // need a list to store the results in
        // we will just replace the whole list for now when results come in
        // will worry about ObservableCollections later
        private List<BookSearchItem> _results;
        public List<BookSearchItem> Results
        {
            get { return _results; }
            set { _results = value; RaisePropertyChanged(() => Results); }
        }

        private async void Update()
        {
            // a new search term has been set so let's ask google for results
            // set our public IsBusy property (to fire our bounded events)

            IsBusy = true;

            // use the Book service to search and when a result comes back
            // set the VM's Results property above to the items in the result that came back
            try
            {
                await _booksService.StartSearchAsync(SearchTerm).ContinueWith((task) => HandleSearchResult(task));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "BooksViewModel:Update:Exception: Something went wrong in StartSearchAsync.");
            }
            finally
            {
                Debug.WriteLine("BooksViewModel:Update: finally - in the try catch...got here!");
            }
        }

        private void HandleSearchResult(Task<BookSearchResult> task)
        {
            if (task.Status == TaskStatus.RanToCompletion
                && task.Result != null)
            {
                Debug.WriteLine("Hey, Made it to HandleSearchResult using .ContinueWith!");

                // Set bindable ViewModel properties with data returned from Search
                Results = task.Result.Items;
                IsBusy = false;
            }
            else
            {
                Debug.WriteLine("Ooopps! Made it back to BooksViewModel but BookSearchResult was null!");
                IsBusy = false;
            }
        }
    }
}
