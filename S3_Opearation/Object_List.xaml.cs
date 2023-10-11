using Amazon.S3.Model;
using Amazon.S3;
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
using System.Windows.Shapes;
using System.Data;

namespace S3_Opearation
{
    /// <summary>
    /// Interaction logic for Object_List.xaml
    /// </summary>
    public partial class Object_List : Page
    {
        DataSet dsMstr;
        public static string? rootFolder;
        Rule_Mstr_DAL _obj_Rule_Mstr_DAL = new Rule_Mstr_DAL();

        public Object_List()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRootObjects();
            DataSet ds = new DataSet();
            if (!string.IsNullOrEmpty(Global.UserName.ToString()))
            {
                ds = _obj_Rule_Mstr_DAL.Get_User_Ctrl_Restric(Global.UserName.ToString(), this.ToString());
            }

            //_dt = _obj_Rule_Mstr_DAL.Get_User_Ctrl_Restric("piyush", this.ToString());
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    // Find the control by name
                    //System.Windows.Controls.Button foundControl = (System.Windows.Controls.Button)S3_Bucket_Grid.FindName(controlName);
                    FrameworkElement foundControl = this.FindName(row[0].ToString()) as FrameworkElement;

                    if (foundControl != null)
                    {
                        //Type controlType = foundControl.GetType();
                        foundControl.Visibility = Visibility.Visible;
                    }
                }
            }
            dsMstr = new Rule_Mstr_DAL().getRuleMstr("", 0, 1);
            if (dsMstr.Tables[0].Rows.Count > 0)
            {
                //Bucket_name.Text = Global.bucketName;
                //Backup_Bucket_Activity.Text = Global.BackupBucket;
            }
            else
            {
                System.Windows.MessageBox.Show("No data found.", "Information", MessageBoxButton.OK);
            }
        }

        private void LoadRootObjects()
        {
            //Global.Primaryregion
            new ComUtils().loadGlobalData();
            var s3Client = new AmazonS3Client(Global.Accesskey, Global.SecurityKey, Global.primaryRegion);


            // Define a list of S3 objects
            ListObjectsV2Request request = new ListObjectsV2Request
            {
                BucketName = Global.bucketName,
                Delimiter = "/"
            };

            ListObjectsV2Response response;
            do
            {
                response = s3Client.ListObjectsV2Async(request).GetAwaiter().GetResult();

                foreach (var prefix in response.CommonPrefixes)
                {
                    rootFolder = prefix.Substring(0, prefix.Length - 1); // Remove trailing slash
                    if (!Folder_Info.Items.Contains(rootFolder))
                    {
                        Folder_Info.Items.Add(rootFolder);
                    }
                }

                request.ContinuationToken = response.NextContinuationToken;
            } while (response.IsTruncated);
        }

        private void Folder_Info_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            new ComUtils().loadGlobalData();
            string selectedRootFolder = (string)Folder_Info.SelectedItem;
            if (selectedRootFolder != null)
            {
                path_txt.Text = Global.bucketName+"/"+selectedRootFolder;

                // Replace with your own S3 bucket name and AWS region
                //string bucketName = "hkprimerybkt";
                var s3Client = new AmazonS3Client(Global.Accesskey, Global.SecurityKey, Global.primaryRegion);

                ListObjectsV2Request request = new ListObjectsV2Request
                {
                    BucketName = Global.bucketName,
                    Prefix = selectedRootFolder + "/",
                    Delimiter = "/"
                };
                Folder_Info.ItemsSource = null;
                Folder_Info.Items.Clear();

                //ListObjectsV2Response response;
                var response = s3Client.ListObjectsV2Async(request).GetAwaiter().GetResult();

                List<string> filesAndFolders = response.S3Objects.Select(obj => obj.Key).ToList();
                List<string> subFolders = response.CommonPrefixes.Select(prefix => prefix.TrimEnd('/')).ToList();

                Folder_Info.ItemsSource = filesAndFolders.Concat(subFolders);

            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            new ComUtils().loadGlobalData();
            Folder_Info.ItemsSource = null;
            path_txt.Text = "";
            Folder_Info.Items.Clear();
            LoadRootObjects();
        }
    }
}
