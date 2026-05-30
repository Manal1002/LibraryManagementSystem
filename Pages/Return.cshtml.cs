using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LibratyManagementSystem.Services;

namespace LibratyManagementSystem.Pages
{
    public class ReturnModel : PageModel
    {
        private readonly LibraryService _libraryService;

        public ReturnModel(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        [BindProperty]
        [Required(ErrorMessage = "Book Copy Number is required to process return.")]
        [Display(Name = "Copy Registration Number")]
        public string CopyNumber { get; set; }

        public string SuccessMessage { get; set; }
        public string ReservationAlert { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet(string copyNum)
        {
            if (!string.IsNullOrEmpty(copyNum))
            {
                CopyNumber = copyNum;
            }
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            string alert = "";
            bool success = _libraryService.ReturnBook(CopyNumber, out alert);

            if (success)
            {
                if (alert.StartsWith("RESERVATION TRIGGERED", StringComparison.OrdinalIgnoreCase))
                {
                    ReservationAlert = alert;
                }
                else
                {
                    SuccessMessage = alert;
                }

                // Clear fields
                CopyNumber = string.Empty;
                ModelState.Clear();
            }
            else
            {
                ErrorMessage = $"RETURN PROCESSING ERROR: {alert}";
            }

            return Page();
        }
    }
}
