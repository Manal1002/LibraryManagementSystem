using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LibratyManagementSystem.Models;
using LibratyManagementSystem.Services;

namespace LibratyManagementSystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly LibraryService _libraryService;

        public IndexModel(LibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        // Stats
        public int TotalBooks { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCopies { get; set; }
        public int ActiveLoansCount { get; set; }
        public int PendingReservationsCount { get; set; }

        // Dashboard Lists
        public List<LoanDetailsViewModel> RecentLoans { get; set; } = new List<LoanDetailsViewModel>();
        public List<ReservationDetailsViewModel> ActiveReservations { get; set; } = new List<ReservationDetailsViewModel>();

        public void OnGet()
        {
            var books = _libraryService.GetBooks();
            var users = _libraryService.GetUsers();
            var loans = _libraryService.GetLoans();
            var reservations = _libraryService.GetReservations();

            TotalBooks = books.Count;
            TotalUsers = users.Count;
            TotalCopies = books.Sum(b => b.Copies.Count);
            ActiveLoansCount = loans.Count(l => l.Status == "Active");
            PendingReservationsCount = reservations.Count(r => r.Status == "Pending");

            // Populate Recent Loans (Joined with User and Book Details)
            RecentLoans = loans
                .OrderByDescending(l => l.IssueDate)
                .Take(5)
                .Select(l => {
                    var user = users.FirstOrDefault(u => u.UserNumber == l.UserNumber);
                    
                    // Find copy and book title
                    BookCopy copy = null;
                    Book book = null;
                    foreach (var b in books)
                    {
                        copy = b.Copies.FirstOrDefault(c => c.CopyNumber == l.CopyNumber);
                        if (copy != null)
                        {
                            book = b;
                            break;
                        }
                    }

                    return new LoanDetailsViewModel
                    {
                        LoanId = l.LoanId,
                        CopyNumber = l.CopyNumber,
                        BookTitle = book?.Title ?? "Unknown Title",
                        MemberName = user?.Name ?? "Unknown Member",
                        MemberNumber = l.UserNumber,
                        IssueDate = l.IssueDate,
                        DueDate = l.DueDate,
                        ReturnDate = l.ReturnDate,
                        Status = l.Status
                    };
                }).ToList();

            // Populate Active Reservations
            ActiveReservations = reservations
                .Where(r => r.Status == "Pending")
                .OrderByDescending(r => r.ReservedDate)
                .Take(5)
                .Select(r => {
                    var user = users.FirstOrDefault(u => u.UserNumber == r.UserNumber);
                    var book = books.FirstOrDefault(b => b.AccessionNumber == r.BookAccessionNumber);

                    return new ReservationDetailsViewModel
                    {
                        ReservationId = r.ReservationId,
                        BookAccession = r.BookAccessionNumber,
                        BookTitle = book?.Title ?? "Unknown Book",
                        MemberName = user?.Name ?? "Unknown Member",
                        MemberNumber = r.UserNumber,
                        ReservedDate = r.ReservedDate,
                        Status = r.Status
                    };
                }).ToList();
        }
    }

    public class LoanDetailsViewModel
    {
        public string LoanId { get; set; }
        public string CopyNumber { get; set; }
        public string BookTitle { get; set; }
        public string MemberName { get; set; }
        public string MemberNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; }
    }

    public class ReservationDetailsViewModel
    {
        public string ReservationId { get; set; }
        public string BookAccession { get; set; }
        public string BookTitle { get; set; }
        public string MemberName { get; set; }
        public string MemberNumber { get; set; }
        public DateTime ReservedDate { get; set; }
        public string Status { get; set; }
    }
}
