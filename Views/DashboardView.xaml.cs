using System.Linq;
using System.Windows.Controls;
using SarasaviLibrary.Data;

namespace SarasaviLibrary.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
            LoadStats();
        }

        private void LoadStats()
        {
            using (var context = new LibraryDbContext())
            {
                TxtTotalBooks.Text = context.Books.Count().ToString();
                TxtTotalMembers.Text = context.Users.Count(u => !u.IsVisitor).ToString();
                TxtBooksOnLoan.Text = context.Loans.Count(l => l.ActualReturnDate == null).ToString();
            }
        }
    }
}
