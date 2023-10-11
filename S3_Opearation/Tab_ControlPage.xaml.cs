using Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace S3_Opearation
{
    /// <summary>
    /// Interaction logic for Tab_ControlPage.xaml
    /// </summary>
    public partial class Tab_ControlPage : Page
    {
        public Tab_ControlPage()
        {
            InitializeComponent();
        }

        private void myCurPage_Navigated(object sender, NavigationEventArgs e)
        {
            myCurPage.NavigationService.RemoveBackEntry();
        }

    
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (Global.Search_permission == true)
            {
                Search.IsEnabled = true;
            }
            else
            {
                Search.IsEnabled = false;
            }

            if (Global.Upload_permission == true)
            {
                Upload.IsEnabled = true;
            }
            else
            {
                Upload.IsEnabled = false;
            }
        }

        private void ListViewItem_MouseDoubleClick_0(object sender, MouseButtonEventArgs e)
        {
            myCurPage.Content = new Upload();
        }

        private void ListViewItem_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            myCurPage.Content = new MainWindow();
        }

        private void ListViewItem_MouseDoubleClick_2(object sender, MouseButtonEventArgs e)
        {
            myCurPage.Content = new Object_List();
        }
    }
}
