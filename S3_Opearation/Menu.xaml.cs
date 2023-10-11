using Dal;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();
        }

        DataSet dsMstr;
        private void PackIcon_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            myCurPage.Content = null;
        }

        private void myCurPage_Navigated(object sender, NavigationEventArgs e)
        {
            myCurPage.NavigationService.RemoveBackEntry();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void PopupLogout_Click(object sender, RoutedEventArgs e)
        {
          MessageBox.Show("Are you sure you want to Logout?");
          Login login = new Login();
          login.Show();
          this.Close();
            //Application.Current.Shutdown();
        }

        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Visible;
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
        }

        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
            ButtonCloseMenu.Visibility = Visibility.Visible;
        }

        private void allControl_KeyDown(object sender, KeyEventArgs e)
        {
            var uiElement = e.OriginalSource as UIElement;
            if (e.Key == Key.Enter && uiElement != null)
            {
                e.Handled = true;
                uiElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            tbUsrCd.Text = Global.UserName.ToUpper();

            try
            {
                dsMstr = new Rule_Mstr_DAL().getRuleMstr("", 0, 1);
                if (dsMstr.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in dsMstr.Tables[0].Rows)
                    {
                        switch (dr["vRuleTyp"].ToString())
                        {
                            case "User_Page":
                                {
                                    if (Convert.ToBoolean(dr["bIsActive"]))
                                    {
                                        S3.Text = dr["sStr1"].ToString();
                                    }
                                    break;
                                }

                            case "Gen_Page":
                                {
                                    if (Convert.ToBoolean(dr["bIsActive"]))
                                    {
                                        Genral_page.Text = dr["sStr1"].ToString();
                                    }
                                    break;
                                }

                            case "S3_Bucket":
                                {
                                    if (Convert.ToBoolean(dr["bIsActive"]))
                                    {
                                        User_page.Text = dr["sStr3"].ToString();
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            new ComUtils().User_Global_Data();

        }
       

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            myCurPage.Content = new Tab_ControlPage();
        }

        private void ListViewItem_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            myCurPage.Content = new Rule_Master();
        }

        private void ListViewItem_MouseDoubleClick_2(object sender, MouseButtonEventArgs e)
        {
            myCurPage.Content = new User_Create();
        }
    }
}
