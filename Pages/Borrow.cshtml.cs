using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LibratyManagementSystem.Services;

namespace LibratyManagementSystem.Pages
{
    public class BorrowModel : PageModel
    {
        private readonly LibraryService _libraryService;

        public BorrowModel(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        [BindProperty]
        [Required(ErrorMessage = "Book Copy Number is required (e.g. S0001-2).")]
        [Display(Name = "Copy Registration Number")]
        public string CopyNumber { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "User Card Number is required (e.g. MEM0001).")]
        [Display(Name = "Borrower Card ID Number")]
        public string UserNumber { get; set; }

        public string SuccessMessage { get; set; }
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

            string error = "";
            bool success = _libraryService.BorrowBook(CopyNumber, UserNumber, out error);

            if (success)
            {
                var user = _libraryService.GetUsers().FirstOrDefault(u => u.UserNumber.Equals(UserNumber, StringComparison.OrdinalIgnoreCase));
                DateTime dueDate = DateTime.Now.AddDays(14);
                SuccessMessage = $"LOAN CONFIRMED SUCCESSFULLY! Copy '{CopyNumber}' is checked out to {user?.Name} ({UserNumber}). Due date: {dueDate:yyyy-MM-dd} (exactly two weeks from today).";
                
                // Clear fields on success
                CopyNumber = string.Empty;
                UserNumber = string.Empty;
                ModelState.Clear();
            }
            else
            {
                ErrorMessage = $"LOAN REJECTED: {error}";
            }

            return Page();
        }
    }
}
