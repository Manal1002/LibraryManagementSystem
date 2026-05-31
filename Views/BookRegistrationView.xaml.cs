using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SarasaviLibrary.Data;
using SarasaviLibrary.Models;

namespace SarasaviLibrary.Views
{
    public partial class BookRegistrationView : UserControl
    {
        public BookRegistrationView()
        {
            InitializeComponent();
            LoadBooks();
        }

        private void LoadBooks()
        {
            using (var context = new LibraryDbContext())
            {
                CboBooks.ItemsSource = context.Books.ToList();
            }
        }

        private void BtnRegisterBook_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTitle.Text) || string.IsNullOrWhiteSpace(TxtPublisher.Text) || CboClassification.SelectedItem == null)
            {
                MessageBox.Show("Please fill all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            char classification = ((ComboBoxItem)CboClassification.SelectedItem).Tag.ToString()[0];

            using (var context = new LibraryDbContext())
            {
                // Generate next number for this classification
                int nextNumber = 1;
                var latestBook = context.Books.Where(b => b.Classification == classification).OrderByDescending(b => b.Number).FirstOrDefault();
                if (latestBook != null)
                {
                    nextNumber = latestBook.Number + 1;
                }

                var book = new Book
                {
                    Title = TxtTitle.Text,
                    Publisher = TxtPublisher.Text,
                    Classification = classification,
                    Number = nextNumber
                };

                context.Books.Add(book);
                context.SaveChanges();
                
                MessageBox.Show($"Book registered successfully!\nBook Number: {book.BookNumber}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Clear fields
                TxtTitle.Clear();
                TxtPublisher.Clear();
                CboClassification.SelectedIndex = -1;
                
                LoadBooks();
            }
        }

        private void BtnAddCopy_Click(object sender, RoutedEventArgs e)
        {
            if (CboBooks.SelectedItem == null)
            {
                TxtFeedback.Text = "Please select a book.";
                return;
            }

            var selectedBook = (Book)CboBooks.SelectedItem;

            using (var context = new LibraryDbContext())
            {
                var bookCopies = context.Copies.Where(c => c.BookId == selectedBook.Id).ToList();
                if (bookCopies.Count >= 10)
                {
                    TxtFeedback.Text = "Maximum 10 copies allowed per book.";
                    return;
                }

                int nextCopyNumber = 1;
                if (bookCopies.Any())
                {
                    nextCopyNumber = bookCopies.Max(c => c.CopyNumber) + 1;
                }

                var copy = new Copy
                {
                    BookId = selectedBook.Id,
                    CopyNumber = nextCopyNumber,
                    IsReferenceOnly = ChkIsReference.IsChecked ?? false,
                    Status = CopyStatus.Available
                };

                context.Copies.Add(copy);
                context.SaveChanges();
                
                // Need to explicitly load the Book to get the BookNumber for the message
                context.Entry(copy).Reference(c => c.Book).Load();

                TxtFeedback.Foreground = System.Windows.Media.Brushes.Green;
                TxtFeedback.Text = $"Copy added successfully! Copy Number: {copy.FullCopyNumber}";
                ChkIsReference.IsChecked = false;
            }
        }
    }
}
