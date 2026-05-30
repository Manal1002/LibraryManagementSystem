using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LibratyManagementSystem.Services;

namespace LibratyManagementSystem.Pages
{
    public class InquiryModel : PageModel
    {
        private readonly LibraryService _libraryService;

        public InquiryModel(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        public List<InquiryResultViewModel> SearchResults { get; set; } = new List<InquiryResultViewModel>();

        public void OnGet()
        {
            SearchResults = _libraryService.SearchCatalog(SearchQuery);
        }
    }
}
