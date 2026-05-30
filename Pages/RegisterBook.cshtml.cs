using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LibratyManagementSystem.Models;
using LibratyManagementSystem.Services;

namespace LibratyManagementSystem.Pages
{
    public class RegisterBookModel : PageModel
    {
        private readonly LibraryService _libraryService;

        public RegisterBookModel(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        [BindProperty]
        [Required(ErrorMessage = "Book Title is required.")]
        [Display(Name = "Book Title")]
        public string Title { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Author name is required.")]
        public string Author { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Publisher details are required.")]
        public string Publisher { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Classification letter is required.")]
        [RegularExpression(@"^[a-zA-Z]$", ErrorMessage = "Classification must be a single letter (e.g. S for Science, L for Literature).")]
        public string Classification { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Book Genre is required.")]
        public string Genre { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Number of book copies is required.")]
        [Range(1, 10, ErrorMessage = "A maximum of 10 copies are allowed to be registered per book number.")]
        [Display(Name = "Total Physical Copies")]
        public int TotalCopies { get; set; } = 1;

        [BindProperty]
        [Required(ErrorMessage = "Reference copies count is required.")]
        [Display(Name = "Reference Only Copies")]
        public int ReferenceCopies { get; set; } = 0;

        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (ReferenceCopies > TotalCopies)
            {
                ErrorMessage = "Validation Error: Reference copies count cannot exceed the total physical copies registered.";
                return Page();
            }

            try
            {
                char classChar = Classification[0];
                var registeredBook = _libraryService.RegisterBook(Title, Author, Publisher, classChar, Genre, TotalCopies, ReferenceCopies);
                
                SuccessMessage = $"SUCCESSFULLY REGISTERED! Created title '{registeredBook.Title}' with unique Accession Number {registeredBook.AccessionNumber}. {TotalCopies} copies generated ({TotalCopies - ReferenceCopies} borrowable, {ReferenceCopies} reference).";
                
                // Clear fields for new registration
                Title = string.Empty;
                Author = string.Empty;
                Publisher = string.Empty;
                Classification = string.Empty;
                Genre = string.Empty;
                TotalCopies = 1;
                ReferenceCopies = 0;
                
                ModelState.Clear();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred during book copy generation: {ex.Message}";
            }

            return Page();
        }
    }
}
