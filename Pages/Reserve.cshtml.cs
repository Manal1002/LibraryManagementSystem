using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LibratyManagementSystem.Services;

namespace LibratyManagementSystem.Pages
{
    public class ReserveModel : PageModel
    {
        private readonly LibraryService _libraryService;

        public ReserveModel(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        [BindProperty]
        [Required(ErrorMessage = "Book Accession Number is required (e.g. S0001).")]
        [Display(Name = "Book Accession Code (X 9999)")]
        public string AccessionNumber { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Borrower Card ID is required.")]
        [Display(Name = "Reserver Card ID Number")]
        public string UserNumber { get; set; }

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet(string accessionNum)
        {
            if (!string.IsNullOrEmpty(accessionNum))
            {
                AccessionNumber = accessionNum;
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string error = "";
            bool success = _libraryService.ReserveBook(AccessionNumber, UserNumber, out error);

            if (success)
            {
                var book = _libraryService.GetBooks().FirstOrDefault(b => b.AccessionNumber.Equals(AccessionNumber, StringComparison.OrdinalIgnoreCase));
                var user = _libraryService.GetUsers().FirstOrDefault(u => u.UserNumber.Equals(UserNumber, StringComparison.OrdinalIgnoreCase));
                
                int queuePos = _libraryService.GetReservations().Count(r => 
                    r.BookAccessionNumber.Equals(AccessionNumber, StringComparison.OrdinalIgnoreCase) && 
                    r.Status == "Pending"
                );

                SuccessMessage = $"RESERVATION LOGGED SUCCESSFULLY! Reserved '{book?.Title}' for member {user?.Name} ({UserNumber}). Current queue position for this book title: #{queuePos}.";
                
                // Clear fields
                AccessionNumber = string.Empty;
                UserNumber = string.Empty;
                ModelState.Clear();
            }
            else
            {
                ErrorMessage = $"RESERVATION BLOCKED: {error}";
            }

            return Page();
        }
    }
}
