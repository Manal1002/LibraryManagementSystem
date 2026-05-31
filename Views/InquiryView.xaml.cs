using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using SarasaviLibrary.Data;

namespace SarasaviLibrary.Views
{
    public partial class InquiryView : UserControl
    {
        public InquiryView()
        {
            InitializeComponent();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string query = TxtSearchQuery.Text.Trim().ToLower();
            
            using (var context = new LibraryDbContext())
            {
                var copies = context.Copies
                    .Include(c => c.Book)
                    .AsEnumerable() // Client side evaluation for BookNumber string manipulation in real query this should be optimized
                    .Where(c => c.Book.Title.ToLower().Contains(query) || 
                                c.Book.BookNumber.ToLower().Contains(query))
                    .ToList();
                    
                DgResults.ItemsSource = copies;
            }
        }
    }
}
