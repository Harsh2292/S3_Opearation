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
using System.Windows.Shapes;
using System.Data.SqlClient;
using Dal;
using Newtonsoft.Json;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using Elasticsearch.Net;

namespace S3_Opearation
{
   
    public partial class Login : Window
    {
        
        public Login()
        {
            InitializeComponent();
            txtLoginID.Focus();           
        }
        public static string? connectionString;        
        public static void conStrSetJson()
        {
            string sMain = "";
            string json = System.IO.File.ReadAllText(@"ConStr.json");
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            dynamic array = JsonConvert.DeserializeObject(json);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            foreach (var item in array)
            {
                if (item.Name == "MainConStr")
                    sMain = item.Value;
            }
            connectionString = sMain;
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            conStrSetJson();
            string _strFlag = "";
            SqlConnection con = new SqlConnection(connectionString);
            try
            {
                con.Open();
                //SqlCommand cmd = new SqlCommand("_UserMaster_GET", con);
                SqlCommand cmd = new SqlCommand("usp_CheckUserPass", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@pUserName", txtLoginID.Text);
                cmd.Parameters.AddWithValue("@pPassword", txtPwds.Password);
                cmd.Parameters.Add("@p_flag", SqlDbType.Char, 100);
                cmd.Parameters["@p_flag"].Direction = ParameterDirection.Output;
                //SqlDataReader dr = cmd.ExecuteReader();
                int i = cmd.ExecuteNonQuery();
                //Storing the output parameters value in 3 different variables.  
                _strFlag = cmd.Parameters["@p_flag"].Value.ToString().TrimEnd();
                if (_strFlag == "Y")
                {
                    Global.UserName = txtLoginID.Text.ToString();
                    Menu menu = new Menu();
                    menu.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("User/Password is not found!", "Information", MessageBoxButton.OK);
                    txtLoginID.Focus();
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally { con.Close(); }

        }
        private void btnPWForgot_Click(object sender, RoutedEventArgs e)
        {
            Forgot();
        }
        void Forgot()
        {

        }
        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            //txtError.Text = "";
            txtLoginID.Text = "";
            txtPwds.Password = "";
            //CustomerReport rpt = new CustomerReport();
            //rpt.ShowDialog();
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
        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("About");
        }
        private void txtLoginID_GotFocus(object sender, RoutedEventArgs e)
        {
            txtLoginID.SelectAll();
        }

        private void txtLoginID_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtLoginID.Text == "")
            {
                txtLoginID.Text = "Enter Username";
                txtLoginID.Style = Application.Current.Resources["txtBox_UsrGray"] as Style;
            }
        }

        private void txtLoginID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtLoginID.Focus();
            }
            else
            {
                txtLoginID.Style = Application.Current.Resources["txtBox_UsrBlck"] as Style;
            }
        }

        private void txtLoginID_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtLoginID.SelectionLength == 0)
                txtLoginID.SelectAll();
        }
        private void txtLoginID_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (txtLoginID.SelectionLength == 0)
                txtLoginID.SelectAll();

            // further clicks will not select all
            txtLoginID.LostMouseCapture -= txtLoginID_LostMouseCapture;
        }

        private void txtLoginID_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtLoginID.LostMouseCapture += txtLoginID_LostMouseCapture;
            if (txtLoginID.Text == "")
            {
                txtLoginID.Text = "Enter Username";
                txtLoginID.Style = Application.Current.Resources["txtBox_UsrGray"] as Style;
            }
        }

        private void txtPwds_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //if (txtPwds.SelectionLength == 0)
            txtPwds.SelectAll();
        }

        private void txtPwds_LostMouseCapture(object sender, MouseEventArgs e)
        {
            //if (txtPwds.SelectionLength == 0)
            txtPwds.SelectAll();

            // further clicks will not select all
            txtPwds.LostMouseCapture -= txtPwds_LostMouseCapture;
        }

        private void txtPwds_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            txtPwds.LostMouseCapture += txtPwds_LostMouseCapture;
            if (txtPwds.Password == "")
            {
                txtPwds.Password = "12345";
                txtPwds.Style = Application.Current.Resources["pasBox_Gray"] as Style;
            }
        }

        private void txtPwds_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txtPwds.Focus();
            }
            else
            {
                //TextBox txtBox = (TextBox)sender;
                //txtvUserAddRecPassword.Text.Replace(vDefUserId, "");
                txtPwds.Style = Application.Current.Resources["pasBox_Blck"] as Style;

            }
        }     
        private void PopupPWForgot_Click(object sender, RoutedEventArgs e)
        {
            OverlayForgotPW.Visibility = Visibility.Visible;
            tbUserID.Text = "";
            tbNewPW.Password = "";
            tbConfirmPW.Password = "";
            tbUserID.Focus();
        }

        private void PWControl_KeyDown(object sender, KeyEventArgs e)
        {
            var uiElement = e.OriginalSource as UIElement;
            if (e.Key == Key.Enter && uiElement != null)
            {
                e.Handled = true;
                uiElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        private void btnPWClose_Click(object sender, RoutedEventArgs e)
        {
            tbUserID.Text = "";
            tbNewPW.Password = "";
            tbConfirmPW.Password = "";
            OverlayForgotPW.Visibility = Visibility.Hidden;
        }    
    }  
}

