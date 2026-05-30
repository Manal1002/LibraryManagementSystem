using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LibratyManagementSystem.Models;
using LibratyManagementSystem.Services;

namespace LibratyManagementSystem.Pages
{
    public class RegisterUserModel : PageModel
    {
        private readonly LibraryService _libraryService;

        public RegisterUserModel(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        [BindProperty]
        [Display(Name = "User Card Number (Optional)")]
        public string UserNumber { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Full Name is required.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Sex selection is required.")]
        public string Sex { get; set; } = "Male";

        [BindProperty]
        [Required(ErrorMessage = "NIC number is required.")]
        [Display(Name = "NIC Number / Identification Card")]
        public string NIC { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Postal address is required.")]
        public string Address { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "User Account Type is required.")]
        [Display(Name = "Account Type / Designation")]
        public string UserType { get; set; } = "Member"; // "Member" or "Visitor"

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

            try
            {
                var user = new User
                {
                    UserNumber = UserNumber,
                    Name = Name,
                    Sex = Sex,
                    NIC = NIC,
                    Address = Address,
                    UserType = UserType
                };

                var registered = _libraryService.RegisterUser(user);
                
                SuccessMessage = $"USER REGISTERED SUCCESSFULLY! Captured details for '{registered.Name}' as a Registered {registered.UserType}. Assigned Card ID: {registered.UserNumber}.";
                
                // Clear fields
                UserNumber = string.Empty;
                Name = string.Empty;
                Sex = "Male";
                NIC = string.Empty;
                Address = string.Empty;
                UserType = "Member";

                ModelState.Clear();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred during borrower registration: {ex.Message}";
            }

            return Page();
        }
    }
}
