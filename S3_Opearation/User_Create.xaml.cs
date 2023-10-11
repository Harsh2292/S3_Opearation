using Dal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
    /// Interaction logic for User_Create.xaml
    /// </summary>
    public partial class User_Create : Page
    {
        DataSet dsMstr;
        int use_id = 0;
        public User_Create()
        {
            InitializeComponent();
            Show_Datagrid();
        }

        private void edit_Click(object sender, RoutedEventArgs e)
        {
            Update_User();

            if (User_List.SelectedItem != null)
            {
                DataRowView rowView = User_List.SelectedItem as DataRowView;
                if (rowView != null)
                {
                    DataRow row = rowView.Row;
                    Is_Active.IsChecked = (bool)row["bIsActive"];
                    Upload_permission.IsChecked = (bool)row["Upload_File"];
                    Search_permission.IsChecked = (bool)row["Search_File"];
                    Move_permission.IsChecked = (bool)row["Move_File"];
                    Delete_permission.IsChecked = (bool)row["Delete_File"];
                    Download_permission.IsChecked = (bool)row["Download_File"];
                }
            }
            btnSaveCaption.Text = "Update";
            // Refresh the DataGrid to reflect the changes in the DataTable
            User_List.Items.Refresh();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            user_Details_Save();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            clear();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(null);
        }

        private void clear()
        {
            user_id.Text = string.Empty;
            user_name.Text = string.Empty;
            password.Text = string.Empty;
            Is_Active.IsChecked = false;
            Upload_permission.IsChecked = false;
            Delete_permission.IsChecked = false;
            Download_permission.IsChecked = false;
            Search_permission.IsChecked = false;
            Move_permission.IsChecked = false;
            btnSaveCaption.Text = "Save";
        }
        private void Update_User()
        {
            var selectedItem = User_List.SelectedItem as DataRowView;
            if (selectedItem != null)
            {
                user_master user = new user_master();
                // Extract the data from the selected item
                var userId = selectedItem["iUserID"].ToString();
                var companyId = selectedItem["iCompanyID"].ToString();
                var userName = selectedItem["vUserName"].ToString();
                var userpass = selectedItem["vUserPass"].ToString();

                // Set the data to the TextBoxes
                User_Id.Text = userId;
                user_id.Text = companyId;
                user_name.Text = userName;
                password.Text = userpass;
            }
        }

        private void Show_Datagrid()
        {
            dsMstr = new User_Mstr_Dal().getUserMstr("", 0, 1);
            List<user_master> items = new List<user_master>();

            if (dsMstr.Tables[0].Rows.Count > 0)
            {
                User_List.ItemsSource = dsMstr.Tables[0].DefaultView;
            }
            else
            {
                MessageBox.Show("NO DATA FOUND!!!!!!!!!");
            }
        }

        private void user_Details_Save()
        {
            if (dsMstr.Tables[0].Rows.Count > 0 && btnSaveCaption.Text == "Update")
            {
                dsMstr.Tables[0].AcceptChanges();
                DataTable dtuserMas = new DataTable();
                dtuserMas.Columns.Add("iUserID", typeof(int));
                dtuserMas.Columns.Add("iCompanyID", typeof(int));
                dtuserMas.Columns.Add("vUserName", typeof(string));
                dtuserMas.Columns.Add("vUserPass", typeof(string));
                dtuserMas.Columns.Add("tUserDeactiveDate", typeof(DateTime));
                dtuserMas.Columns.Add("bIsActive", typeof(bool));
                dtuserMas.Columns.Add("iInsertUserID", typeof(int));
                dtuserMas.Columns.Add("vInsertIPAddress", typeof(string));
                dtuserMas.Columns.Add("tInsertTransactionDate", typeof(DateTime));
                dtuserMas.Columns.Add("iUpdateUserID", typeof(int));
                dtuserMas.Columns.Add("vUpdateIPAddress", typeof(string));
                dtuserMas.Columns.Add("tUpdateTransactionDate", typeof(DateTime));
                dtuserMas.Columns.Add("vAccessGroupRule", typeof(string));
                dtuserMas.Columns.Add("Download_File", typeof(string));
                dtuserMas.Columns.Add("Delete_File", typeof(bool));
                dtuserMas.Columns.Add("Upload_File", typeof(bool));
                dtuserMas.Columns.Add("Search_File", typeof(bool));
                dtuserMas.Columns.Add("Move_File", typeof(bool));

                // Populate the DataTable with data
                foreach (DataRow _dr in dsMstr.Tables[0].Rows)
                {
                    DataRow row = dtuserMas.NewRow();
                    row["iUserID"] = User_Id.Text;
                    row["iCompanyID"] = user_id.Text;
                    row["vUserName"] = user_name.Text.ToString();
                    row["vUserPass"] = password.Text.ToString();
                    row["tUserDeactiveDate"] = _dr["tUserDeactiveDate"];
                    if (Is_Active.IsChecked == true)
                    {
                        row["bIsActive"] = 1;
                        Global.User_Active = true;
                    }
                    else
                    {
                        row["bIsActive"] = 0;
                        Global.User_Active = false;
                    }
                    row["iInsertUserID"] = _dr["iInsertUserID"];
                    row["vInsertIPAddress"] = _dr["vInsertIPAddress"];
                    row["tInsertTransactionDate"] = _dr["tInsertTransactionDate"];
                    row["iUpdateUserID"] = _dr["iUpdateUserID"];
                    row["vUpdateIPAddress"] = _dr["vUpdateIPAddress"];
                    row["tUpdateTransactionDate"] = _dr["tUpdateTransactionDate"];
                    row["vAccessGroupRule"] = _dr["vAccessGroupRule"];
                    if (Delete_permission.IsChecked == true)
                    {
                        row["Delete_File"] = 1;
                        Global.Delete_permission = true;
                    }
                    else
                    {
                        row["Delete_File"] = 0;
                        Global.Delete_permission = false;

                    }
                    if (Download_permission.IsChecked == true)
                    {
                        row["Download_File"] = 1;
                        Global.Download_permission = true;
                    }
                    else
                    {
                        row["Download_File"] = 0;
                        Global.Download_permission = false;

                    }
                    if (Upload_permission.IsChecked == true)
                    {
                        row["Upload_File"] = 1;
                        Global.Upload_permission = true;
                    }
                    else
                    {
                        row["Upload_File"] = 0;
                        Global.Upload_permission = false;

                    }
                    if (Search_permission.IsChecked == true)
                    {
                        row["Search_File"] = 1;
                        Global.Search_permission = true;
                    }
                    else
                    {
                        row["Search_File"] = 0;
                        Global.Search_permission = false;
                    }
                    if (Move_permission.IsChecked == true)
                    {
                        row["Move_File"] = 1;
                        Global.Move_permission = true;
                    }
                    else
                    {
                        row["Move_File"] = 0;
                        Global.Move_permission = false;
                    }
                    dtuserMas.Rows.Add(row);
                }
                Hashtable _ht = new User_Mstr_Dal().updUserMstr(dtuserMas);
                btnSaveCaption.Text = "Save";
                clear();
                if (_ht != null)
                {
                    MessageBox.Show("Records updated successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                Show_Datagrid();
            }
            else if (btnSaveCaption.Text == "Save")
            {
                // Define connection string and stored procedure name

                // Create datatable to hold user data
                DataTable dtuserMas = new DataTable();
                dtuserMas.Columns.Add("iUserID", typeof(int));
                dtuserMas.Columns.Add("iCompanyID", typeof(int));
                dtuserMas.Columns.Add("vUserName", typeof(string));
                dtuserMas.Columns.Add("vUserPass", typeof(string));
                dtuserMas.Columns.Add("tUserDeactiveDate", typeof(DateTime));
                dtuserMas.Columns.Add("bIsActive", typeof(bool));
                dtuserMas.Columns.Add("iInsertUserID", typeof(int));
                dtuserMas.Columns.Add("vInsertIPAddress", typeof(string));
                dtuserMas.Columns.Add("tInsertTransactionDate", typeof(DateTime));
                dtuserMas.Columns.Add("iUpdateUserID", typeof(int));
                dtuserMas.Columns.Add("vUpdateIPAddress", typeof(string));
                dtuserMas.Columns.Add("tUpdateTransactionDate", typeof(DateTime));
                dtuserMas.Columns.Add("vAccessGroupRule", typeof(string));
                dtuserMas.Columns.Add("Download_File", typeof(bool));
                dtuserMas.Columns.Add("Delete_File", typeof(bool));
                dtuserMas.Columns.Add("Upload_File", typeof(bool));
                dtuserMas.Columns.Add("Search_File", typeof(bool));
                dtuserMas.Columns.Add("Move_File", typeof(bool));

                // Add user data to the datatable
                DataRow row = dtuserMas.NewRow();
                row["iUserID"] = 0;
                row["iCompanyID"] = user_id.Text;
                row["vUserName"] = user_name.Text.ToString();
                row["vUserPass"] = password.Text.ToString();
                row["tUserDeactiveDate"] = DBNull.Value;
                if (Is_Active.IsChecked == true)
                {
                    row["bIsActive"] = 1;
                    Global.User_Active = true;
                }
                else
                {
                    row["bIsActive"] = 0;
                    Global.User_Active = false;
                }
                row["iInsertUserID"] = DBNull.Value;
                row["vInsertIPAddress"] = DBNull.Value;
                row["tInsertTransactionDate"] = DateTime.Now;
                row["iUpdateUserID"] = DBNull.Value;
                row["vUpdateIPAddress"] = DBNull.Value;
                row["tUpdateTransactionDate"] = DateTime.Now;
                row["vAccessGroupRule"] = DBNull.Value;
                if (Delete_permission.IsChecked == true)
                {
                    row["Delete_File"] = 1;
                    Global.Delete_permission = true;
                }
                else
                {
                    row["Delete_File"] = 0;
                    Global.Delete_permission = false;

                }
                if (Download_permission.IsChecked == true)
                {
                    row["Download_File"] = 1;
                    Global.Download_permission = true;
                }
                else
                {
                    row["Download_File"] = 0;
                    Global.Download_permission = false;

                }
                if (Upload_permission.IsChecked == true)
                {
                    row["Upload_File"] = 1;
                    Global.Upload_permission = true;
                }
                else
                {
                    row["Upload_File"] = 0;
                    Global.Upload_permission = false;

                }
                if (Search_permission.IsChecked == true)
                {
                    row["Search_File"] = 1;
                    Global.Search_permission = true;
                }
                else
                {
                    row["Search_File"] = 0;
                    Global.Search_permission = false;
                }
                if (Move_permission.IsChecked == true)
                {
                    row["Move_File"] = 1;
                    Global.Move_permission = true;
                }
                else
                {
                    row["Move_File"] = 0;
                    Global.Move_permission = false;
                }
                dtuserMas.Rows.Add(row);
                Hashtable _ht1 = new User_Mstr_Dal().updUserMstr(dtuserMas);
                Show_Datagrid();
            }
        }
    }
        public class user_master
        {
            public int iUserID { get; set; }

            public int iCompanyID { get; set; }
            public string vUserName { get; set; }

            public string vUserPass { get; set; }
            public bool bIsActive { get; set; }
            public bool Upload_File { get; set; }
            public bool Search_File { get; set; }
            public bool Move_File { get; set; }
            public bool Delete_File { get; set; }
            public bool Download_File { get; set; }

        }    
}
