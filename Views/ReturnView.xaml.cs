using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SarasaviLibrary.Data;
using SarasaviLibrary.Models;

namespace SarasaviLibrary.Views
{
    public partial class ReturnView : UserControl
    {
        public ReturnView()
        {
            InitializeComponent();
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            ReservationAlert.Visibility = Visibility.Collapsed;
            string fullCopyNumber = TxtCopyNumber.Text.Trim();
            if (string.IsNullOrEmpty(fullCopyNumber)) return;

            using (var context = new LibraryDbContext())
            {
                var allCopies = context.Copies.Include(c => c.Book).ToList();
                var copy = allCopies.FirstOrDefault(c => c.FullCopyNumber.Equals(fullCopyNumber, StringComparison.OrdinalIgnoreCase));

                if (copy == null)
                {
                    TxtFeedback.Text = "Copy not found.";
                    TxtFeedback.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                var activeLoan = context.Loans.FirstOrDefault(l => l.CopyId == copy.Id && l.ActualReturnDate == null);
                
                if (activeLoan == null)
                {
                    TxtFeedback.Text = "This copy is not currently loaned out.";
                    TxtFeedback.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                // Process Return
                activeLoan.ActualReturnDate = DateTime.Now;
                copy.Status = CopyStatus.Available;

                // Check Reservations for this TITLE
                var oldestReservation = context.Reservations
                    .Include(r => r.User)
                    .Where(r => r.BookId == copy.BookId)
                    .OrderBy(r => r.ReservationDate)
                    .FirstOrDefault();

                if (oldestReservation != null)
                {
                    // Update copy status to reserved
                    copy.Status = CopyStatus.Reserved;
                    
                    // Alert Librarian
                    TxtReservationAlert.Text = $"Please put this book on one side! It is reserved by {oldestReservation.User.Name} ({oldestReservation.User.UserNumber}). Notification sent to the user.";
                    ReservationAlert.Visibility = Visibility.Visible;
                    
                    // Delete reservation as it is fulfilled
                    context.Reservations.Remove(oldestReservation);
                }

                context.SaveChanges();

                TxtFeedback.Text = "Book returned successfully.";
                TxtFeedback.Foreground = System.Windows.Media.Brushes.Green;
                TxtCopyNumber.Clear();
            }
        }
    }
}
