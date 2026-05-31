using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SarasaviLibrary.Data;
using SarasaviLibrary.Models;

namespace SarasaviLibrary.Views
{
    public partial class LoanView : UserControl
    {
        private User _currentUser;

        public LoanView()
        {
            InitializeComponent();
        }

        private void BtnCheckUser_Click(object sender, RoutedEventArgs e)
        {
            string userNum = TxtUserNumber.Text.Trim();
            if (string.IsNullOrEmpty(userNum)) return;

            using (var context = new LibraryDbContext())
            {
                _currentUser = context.Users.Include(u => u.Loans).FirstOrDefault(u => u.UserNumber == userNum);
                
                if (_currentUser == null)
                {
                    TxtUserStatus.Text = "User not found.";
                    TxtUserStatus.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                if (_currentUser.IsVisitor)
                {
                    TxtUserStatus.Text = $"User {_currentUser.Name} is a Visitor and cannot borrow books.";
                    TxtUserStatus.Foreground = System.Windows.Media.Brushes.Red;
                    _currentUser = null;
                    return;
                }

                int activeLoans = _currentUser.Loans.Count(l => l.ActualReturnDate == null);
                bool hasOverdue = _currentUser.Loans.Any(l => l.ActualReturnDate == null && l.ExpectedReturnDate < DateTime.Now);

                TxtUserStatus.Foreground = hasOverdue ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Green;
                TxtUserStatus.Text = $"Name: {_currentUser.Name} | Active Loans: {activeLoans}/5\nOverdue Books: {(hasOverdue ? "YES - Cannot borrow" : "No")}";
                
                if (activeLoans >= 5 || hasOverdue)
                {
                    _currentUser = null; // Block loaning
                }
            }
        }

        private void BtnLoan_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                TxtFeedback.Text = "Please verify a valid user first who is eligible to borrow.";
                TxtFeedback.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            string fullCopyNumber = TxtCopyNumber.Text.Trim();
            if (string.IsNullOrEmpty(fullCopyNumber)) return;

            using (var context = new LibraryDbContext())
            {
                // We have to evaluate on client side because FullCopyNumber is a computed property.
                // In a production app, we would query by Book classification, book number, and copy number separately.
                var allCopies = context.Copies.Include(c => c.Book).ToList();
                var copy = allCopies.FirstOrDefault(c => c.FullCopyNumber.Equals(fullCopyNumber, StringComparison.OrdinalIgnoreCase));

                if (copy == null)
                {
                    TxtFeedback.Text = "Copy not found.";
                    TxtFeedback.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                if (copy.IsReferenceOnly)
                {
                    TxtFeedback.Text = "This copy is for Reference Only and cannot be borrowed.";
                    TxtFeedback.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                if (copy.Status != CopyStatus.Available)
                {
                    TxtFeedback.Text = $"Copy is currently {copy.Status}.";
                    TxtFeedback.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                // Confirm Loan
                var loan = new Loan
                {
                    UserId = _currentUser.Id,
                    CopyId = copy.Id,
                    BorrowDate = DateTime.Now,
                    ExpectedReturnDate = DateTime.Now.AddDays(14) // 2 weeks
                };

                copy.Status = CopyStatus.Loaned;
                context.Loans.Add(loan);
                context.SaveChanges();

                TxtFeedback.Text = $"Loan successful! Expected Return Date: {loan.ExpectedReturnDate:yyyy-MM-dd}";
                TxtFeedback.Foreground = System.Windows.Media.Brushes.Green;
                
                TxtCopyNumber.Clear();
                _currentUser = null; // Reset for next transaction
                TxtUserStatus.Text = "";
                TxtUserNumber.Clear();
            }
        }
    }
}
