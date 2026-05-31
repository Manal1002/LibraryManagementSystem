using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SarasaviLibrary.Data;
using SarasaviLibrary.Models;

namespace SarasaviLibrary.Views
{
    public partial class UserRegistrationView : UserControl
    {
        public UserRegistrationView()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtName.Text) || string.IsNullOrWhiteSpace(TxtNIC.Text) || 
                string.IsNullOrWhiteSpace(TxtAddress.Text) || CboSex.SelectedItem == null)
            {
                TxtFeedback.Text = "Please fill all fields.";
                TxtFeedback.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            using (var context = new LibraryDbContext())
            {
                bool isVisitor = RdoVisitor.IsChecked ?? false;
                string prefix = isVisitor ? "V" : "M";

                // Generate User Number
                int nextNumber = 1;
                var latestUser = context.Users.Where(u => u.IsVisitor == isVisitor)
                                              .OrderByDescending(u => u.Id)
                                              .FirstOrDefault();
                
                if (latestUser != null && latestUser.UserNumber.Length > 1)
                {
                    if (int.TryParse(latestUser.UserNumber.Substring(1), out int lastNum))
                    {
                        nextNumber = lastNum + 1;
                    }
                }

                string userNumber = $"{prefix}{nextNumber:D3}";

                var user = new User
                {
                    UserNumber = userNumber,
                    Name = TxtName.Text,
                    Sex = ((ComboBoxItem)CboSex.SelectedItem).Content.ToString(),
                    NIC = TxtNIC.Text,
                    Address = TxtAddress.Text,
                    IsVisitor = isVisitor
                };

                context.Users.Add(user);
                context.SaveChanges();

                TxtFeedback.Text = $"User registered successfully! User Number: {userNumber}";
                TxtFeedback.Foreground = System.Windows.Media.Brushes.Green;

                // Clear form
                TxtName.Clear();
                TxtNIC.Clear();
                TxtAddress.Clear();
                CboSex.SelectedIndex = -1;
                RdoMember.IsChecked = true;
            }
        }
    }
}
