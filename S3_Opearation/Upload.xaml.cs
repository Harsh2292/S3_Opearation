using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using Amazon.S3.Transfer;
using Amazon;
using static System.Net.Mime.MediaTypeNames;
using Amazon.S3.Internal;
using Amazon.S3.Encryption;
using System.Security.Cryptography;
using SharpCompress.Common;
using Amazon.Runtime.Internal;
using MaterialDesignColors;
using Dal;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows.Controls.Primitives;
using Binding = System.Windows.Data.Binding;
using System.Collections;
using System.Windows.Forms;
using Ookii.Dialogs.Wpf;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Xml.Linq;
using System.Net.NetworkInformation;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Windows.Forms.VisualStyles;
using Nest;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Net;

namespace S3_Opearation
{
    /// <summary>
    /// Interaction logic for Upload.xaml
    /// </summary>
    public partial class Upload : System.Windows.Controls.Page
    {
        public Upload()
        {
            InitializeComponent();

            // Set the data context of the window to the progress object
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        Popup popup = new Popup();
        public string[] vFilesInfo;
        public string[] vfilename;
        public string[] vFolderInfo;
        public string[] vFoldername;
        string connectionString = Datautil.connectionString;
        int itemsPerPage;
        int currentPage;
        int totalItems;
        int totalPages;
        double count1;
        string vKeyName;
        string Path_S3folders;
        List<Dal.SearchResult> searchResults = new List<Dal.SearchResult>();
        string path;
        string selectedValue;
        string Type;
        String SelectedValue;




        private void BrowseFoldersButton_Click(object sender, RoutedEventArgs e)
        {
            var folderBrowserDialog = new VistaFolderBrowserDialog();
            folderBrowserDialog.Description = "Select Folders";
            folderBrowserDialog.UseDescriptionForTitle = true;
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
            folderBrowserDialog.Multiselect = true;
            bool? dialogResult = folderBrowserDialog.ShowDialog();
            if (dialogResult == true)
            {
                Folder_Info.ItemsSource = null;
                Folder_Info.Items.Clear();
                vFolderInfo = folderBrowserDialog.SelectedPaths;
                itemsPerPage = 30;
                currentPage = 1;
                totalItems = vFolderInfo.Count();
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
                List<string> pageResults = vFolderInfo.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();
                Folder_Info.ItemsSource = pageResults;
                string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string csvFilePath = System.IO.Path.Combine(downloadsFolder, "RemainingFiles.csv");
                var records = vFolderInfo.Select(filePath => new { FilePath = filePath});

                using (var writer = new StreamWriter(csvFilePath))
                using (var csv = new CsvWriter(writer, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
                {
                    csv.WriteRecords(records);
                }
                // Process selected folders
            }
            upload.Content = "Upload Folder";            
        }
        private void BrowseFilesButton_Click(object sender, RoutedEventArgs e)
        {
            popup.IsOpen = false;
            Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog();
            openFile.Multiselect = true;            
            openFile.FilterIndex = 1;
            openFile.FilterIndex = 1;
            openFile.Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|" +
                              "Video Files (*.mp4;*.avi;*.wmv)|*.mp4;*.avi;*.wmv|" +
                              "PDF Files (*.pdf)|*.pdf|" +
                              "HTML Files (*.html;*.htm)|*.html;*.htm|" +
                              "All files (*.*)|*.*";
            bool? response = openFile.ShowDialog();
           
            if (response == true)
            {
                vFilesInfo = openFile.FileNames;
                vfilename = openFile.SafeFileNames;
                itemsPerPage = 30;
                currentPage = 1;
                totalItems = vfilename.Count();
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
                List<string> pageResults = vfilename.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();
                Folder_Info.ItemsSource = pageResults;
                string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string csvFilePath = System.IO.Path.Combine(downloadsFolder, "RemainingFiles.csv");
                var records = vFilesInfo.Select(filePath => new { FilePath = filePath });

                using (var writer = new StreamWriter(csvFilePath))
                using (var csv = new CsvWriter(writer, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
                {
                    csv.WriteRecords(records);
                }
            }
            upload.Content = "Upload Files";
        }
        private async void upload_Click(object sender, RoutedEventArgs e)
        {
            bool hasMatchingKeysInDatabase = false; // Flag to check if there are matching keys in the database
            int count;

            if (upload.Content == "Upload Files")
            {
                await Upload_fileAsync();
            }
            else if (upload.Content == "Upload Folder")
            {
                await Upload_Folder();
            }
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                // Check if any keys in the database contain the search term
                Parallel.ForEach(searchResults, item =>
                {
                    using (var command = new SqlCommand("SELECT COUNT(*) FROM TblS3ObjectKeys WHERE S3_keys LIKE '%' + @SearchTerm + '%'", connection))
                     {
                             command.Parameters.AddWithValue("@SearchTerm", item.Key);
                             count = (int)command.ExecuteScalar();
                     }
                    if(count==0) 
                    {
                        string[] parts = item.Key.Split('/');
                        string flg = parts[0];
                        using (var command = new SqlCommand("INSERT INTO TblS3ObjectKeys (S3_keys, flg) VALUES (@S3_keys,@flg)", connection))
                        {
                            command.Parameters.AddWithValue("@S3_keys", item.Key);
                            command.Parameters.AddWithValue("@flg", flg);
                            command.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        string[] parts = item.Key.Split('/');
                        string flg = parts[0];
                        using (var command = new SqlCommand("Update  TblS3ObjectKeys SET S3_keys=@S3_keys, flg=@flg where S3_keys=@S3_keys", connection))
                        {
                            command.Parameters.AddWithValue("@S3_keys", item.Key);
                            command.Parameters.AddWithValue("@flg", flg);
                            command.ExecuteNonQuery();
                        }
                    }
                });
                connection.Close();
            }          
            Folder_Info.ItemsSource = null;
            Folder_Info.Items.Clear();
        }     

        public async Task Upload_fileAsync()
        {            
            int file_upload;
            new ComUtils().loadGlobalData();
            var s3Client = new AmazonS3Client(Global.Accesskey, Global.SecurityKey, Global.primaryRegion);
            TextBlock selectedTextBlock = file_Types.SelectedItem as TextBlock;
            string selectedValue = selectedTextBlock.Text;

            if (selectedValue == "Image")
            {
                Path_S3folders = Global.P_Imagepath;
                Global.key = Path_S3folders;
                await upload_Files();
            }

            else if (selectedValue == "Videos")
            {
                Path_S3folders = Global.P_Video;
                Global.key = Path_S3folders;
                await upload_Files();
            }
            else if (selectedValue == "Certificate")
            {
                Path_S3folders = Global.P_Certi;
                Global.key = Path_S3folders;
                await upload_Files();
            }
            else if (selectedValue == "MaskLabreportno")
            {
                Path_S3folders = Global.P_MaskLab;
                Global.key = Path_S3folders;
                await upload_Files();
            }
            else if (selectedValue == "ActualProportions")
            {

                Path_S3folders = Global.P_ActualProp;
                Global.key = Path_S3folders;
                await upload_Files();
            }
            else if (selectedValue == "Html")
            {
                Path_S3folders = Global.P_Html;
                Global.key = Path_S3folders;
                await upload_Files();
            }
            else if (selectedValue == "Mp4")
            {
                Path_S3folders = Global.P_Mp4 ;
                Global.key = Path_S3folders;
                await upload_Files();
            }
            Folder_Info.ItemsSource = null;
            Folder_Info.Items.Clear();
        }


        public async Task upload_Files()
        {
            // Check internet connectivity before starting the file upload
          

            int file_upload;
            new ComUtils().loadGlobalData();
            var s3Client = new AmazonS3Client(Global.Accesskey, Global.SecurityKey, Global.primaryRegion);

            try
            {
                // Periodically check internet connectivity while the file upload is in progress               
                int overallProgress = Convert.ToInt32(0.0);
                int count1 = vFilesInfo.Count();
                int count2 = vfilename.Count();
                file_upload = count2;
                int increment = Convert.ToInt32(100.0 / (count2));

                if (count1 != count2)
                {
                    System.Windows.MessageBox.Show("The number of files selected does not match the number of files in the CSV.");
                    return;
                }

                // Make a copy of the list of file paths to use while uploading
                List<string> remainingFiles = new List<string>(vFilesInfo);

                foreach (string i in vFilesInfo)
                {                   
                    Upload_Dal.vFilePath = i;

                    foreach (string name in vfilename)
                    {                       
                        if (Upload_Dal.vFilePath.Contains(name))
                        {
                            Upload_Dal.vKeyName = Global.key + name;

                            try
                            {
                                await Upload_Dal.UploadFileAsync(s3Client, Global.bucketName, Upload_Dal.vKeyName, Upload_Dal.vFilePath);

                                // Remove the uploaded file from the list of remaining files
                                remainingFiles.Remove(i);

                                searchResults.Add(new Dal.SearchResult
                                {
                                    IsChecked = true,
                                    folderName = name,
                                    Key = Upload_Dal.vKeyName
                                });
                            }
                            catch
                            {
                                // Handle any specific exception related to file upload
                                // For example, you can add specific error handling if needed
                                // In this case, the file upload process will continue to the next file
                            }
                        }
                    }

                    overallProgress += increment;
                    // Update the progress bar on the UI thread
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressBar.Value = overallProgress;
                        file_upload--;
                        countLabel.Content = $"Pending Files:{file_upload}";
                    });

                    // Update the CSV file after each successful upload
                    string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string csvFilePath = System.IO.Path.Combine(downloadsFolder, "RemainingFiles.csv");

                    // Convert each file path to an instance of FilePathInfo
                    var records = remainingFiles.Select(filePath => new FilePathInfo { FilePath = filePath,Type = "File",Location= Global.key });

                    using (var writer = new StreamWriter(csvFilePath))
                    using (var csv = new CsvWriter(writer, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
                    {
                        csv.WriteRecords(records);
                    }
                }

                // Reset the progress bar and display the message
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    countLabel.Content = $"Pending Files:{0}";
                    progressBar.Value = 0.0;
                });

                System.Windows.MessageBox.Show("Files were uploaded successfully");
            }
            catch (Exception ex)
            {
                // Handle any exception that occurred during the upload process
                System.Windows.MessageBox.Show("An error occurred during file upload: " + ex.Message);
            }
        }
      
        public async Task Upload_Folder()
        {
            new ComUtils().loadGlobalData();
            var s3Client = new AmazonS3Client(Global.Accesskey, Global.SecurityKey, Global.primaryRegion);
            TextBlock selectedTextBlock = file_Types.SelectedItem as TextBlock;
            if (selectedTextBlock != null)
            {
                selectedValue = selectedTextBlock.Text;
            }
            else
            {
                selectedValue=SelectedValue;
            }
            if (selectedValue == "Image")
            {
                int folder_Uploaded = Convert.ToInt32(count1);

                foreach (string i in vFolderInfo)
                {
                    string path = i;
                    try
                    {
                        double overallProgress = 0.0;
                         count1 = vFolderInfo.Count();
                        double increment = 100.0 / (count1);
                        string Bucket_Folder = Global.P_Imagepath + path.Substring(path.LastIndexOf("\\") + 1);
                        string buckt_F = Bucket_Folder.Replace(" ", "_");                       
                        await Upload_Dal.UploadDirAsync(s3Client, Global.bucketName, path, "*.*", buckt_F);

                        overallProgress += increment;
                        folder_Uploaded--;
                        // Update the progress bar on the UI thread
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            countLabel.Content = $"Pending Folders:{folder_Uploaded}";
                            progressBar.Value = overallProgress;
                        });

                    }
                    catch (Exception ex)
                    {
                        // Handle any exception that occurred during the upload process
                        System.Windows.MessageBox.Show("An error occurred during Folder upload: " + ex.Message);
                    }
                }
            
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    countLabel.Content = $"{0}%";
                    progressBar.Value = 0.0;
                    System.Windows.MessageBox.Show("Folder was uploaded successfully");
                });
            }
            else if (selectedValue == "Videos")
            {
                List<string> remainingFiles = new List<string>(vFolderInfo);

                double count1 = vFolderInfo.Count();
                int folder_Uploaded = (int)count1;

                double overallProgress = 0.0;
                double increment = 100.0 / count1;

                // Create a list to track the progress of each folder
                List<double> folderProgress = new List<double>(Enumerable.Repeat(0.0, folder_Uploaded));

                // Use Parallel.ForEach to upload folders concurrently
                Parallel.ForEach(vFolderInfo, async (i, loopState, index) =>
                {
                    string path = i;
                    string Bucket_Folder = Global.P_Video + path.Substring(path.LastIndexOf("\\") + 1);
                    string buckt_F = Bucket_Folder.Replace(" ", "_");

                    try
                    {
                        await Upload_Dal.UploadDirAsync(s3Client, Global.bucketName, path, "*.*", buckt_F);
                        await TrackMPUAsync_folder(s3Client, Global.bucketName, buckt_F, path, progressBar, increment);
                        remainingFiles.Remove(i);                  
                        searchResults.Add(new Dal.SearchResult
                        {
                            IsChecked = true,
                            folderName = path.Substring(path.LastIndexOf("\\") + 1),
                            Key = buckt_F
                        });
                        // Update progress
                        Interlocked.Decrement(ref folder_Uploaded);
                        double folderProg = increment * (count1 - folder_Uploaded);
                        folderProgress[(int)index] = folderProg;

                        // Update the progress bar and count label on the UI thread periodically
                        if ((int)index % 10 == 0) // Adjust the frequency of UI updates as needed
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                countLabel.Content = $"Pending Folders: {folder_Uploaded}";
                                //progressBar.Value = folderProgress.Average();
                            });
                        }
                        string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        string csvFilePath = System.IO.Path.Combine(downloadsFolder, "RemainingFiles.csv");

                        // Convert each file path to an instance of FilePathInfo
                        var records = remainingFiles.Select(filePath => new FilePathInfo { FilePath = filePath, Type = "Folder" });

                        using (var writer = new StreamWriter(csvFilePath))
                        using (var csv = new CsvWriter(writer, new CsvHelper.Configuration.CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
                        {
                            csv.WriteRecords(records);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle any exception that occurred during the upload process
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            System.Windows.MessageBox.Show("An error occurred during Folder upload: " + ex.Message);
                        });
                    }
                });

                // Wait for all folder uploads to complete
                while (folder_Uploaded > 0)
                {
                    // Update the progress bar and count label on the UI thread periodically
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        countLabel.Content = $"Pending Folders: {folder_Uploaded}";
                        progressBar.Value = folderProgress.Average();
                    });

                    await Task.Delay(100); // Adjust the delay duration as needed
                }

                // Reset the progress bar and display the success message on the UI thread
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    countLabel.Content = $"Pending Folders: {0}";
                    progressBar.Value = 0;
                    System.Windows.MessageBox.Show("Folders were uploaded successfully");
                });

            }
            else if (selectedValue == "Certificate")
            {
                int folder_Uploaded = Convert.ToInt32(count1);

                foreach (string i in vFolderInfo)
                {
                    string path = i;
                    try
                    {
                        double overallProgress = 0.0;
                        double count1 = vFolderInfo.Count();
                        double increment = 100.0 / (count1);
                        string Bucket_Folder = Global.P_Certi + path.Substring(path.LastIndexOf("\\") + 1);
                        string buckt_F = Bucket_Folder.Replace(" ", "_");
                        await Upload_Dal.UploadDirAsync(s3Client, Global.bucketName, path, "*.*", buckt_F);
                        overallProgress += increment;
                        folder_Uploaded--;
                        // Update the progress bar on the UI thread
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            countLabel.Content = $"Pending Folders:{folder_Uploaded}";
                            progressBar.Value = overallProgress;
                        });

                    }
                    catch (Exception ex)
                    {
                        // Handle any exception that occurred during the upload process
                        System.Windows.MessageBox.Show("An error occurred during Folder upload: " + ex.Message);
                    }
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    countLabel.Content = $"{0}%";
                    progressBar.Value = 0.0;
                    System.Windows.MessageBox.Show("Folder was uploaded successfully");
                });
            }
            else if (selectedValue == "MaskLabreportno")
            {
                int folder_Uploaded = Convert.ToInt32(count1);

                foreach (string i in vFolderInfo)
                {
                    string path = i;
                    try
                    {
                        double overallProgress = 0.0;
                        double count1 = vFolderInfo.Count();
                        double increment = 100.0 / (count1);
                        string Bucket_Folder = Global.P_MaskLab + path.Substring(path.LastIndexOf("\\") + 1);
                        string buckt_F = Bucket_Folder.Replace(" ", "_");
                        await Upload_Dal.UploadDirAsync(s3Client, Global.bucketName, path, "*.*", buckt_F);
                        overallProgress += increment;
                        folder_Uploaded--;
                        // Update the progress bar on the UI thread
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            countLabel.Content = $"Pending Folders:{folder_Uploaded}";
                            progressBar.Value = overallProgress;
                        });

                    }
                    catch (Exception ex)
                    {
                        // Handle any exception that occurred during the upload process
                        System.Windows.MessageBox.Show("An error occurred during Folder upload: " + ex.Message);
                    }
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    countLabel.Content = $"{0}%";
                    progressBar.Value = 0.0;
                    System.Windows.MessageBox.Show("Folder was uploaded successfully");
                });
            }
            else if (selectedValue == "ActualProportions")
            {
                int folder_Uploaded = Convert.ToInt32(count1);

                foreach (string i in vFolderInfo)
                {
                    string path = i;
                    try
                    {
                        double overallProgress = 0.0;
                        double count1 = vFolderInfo.Count();
                        double increment = 100.0 / (count1);
                        string Bucket_Folder = Global.P_ActualProp + path.Substring(path.LastIndexOf("\\") + 1);
                        string buckt_F = Bucket_Folder.Replace(" ", "_");
                        await Upload_Dal.UploadDirAsync(s3Client, Global.bucketName, path, "*.*", buckt_F);
                        overallProgress += increment;
                        folder_Uploaded--;
                        // Update the progress bar on the UI thread
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            countLabel.Content = $"Pending Folders:{folder_Uploaded}";
                            progressBar.Value = overallProgress;
                        });

                    }
                    catch (Exception ex)
                    {
                        // Handle any exception that occurred during the upload process
                        System.Windows.MessageBox.Show("An error occurred during Folder upload: " + ex.Message);
                    }
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    countLabel.Content = $"{0}%";
                    progressBar.Value = 0.0;
                    System.Windows.MessageBox.Show("Folder was uploaded successfully");
                });
            }
            else if (selectedValue == "Html")
            {
                int folder_Uploaded = Convert.ToInt32(count1);

                foreach (string i in vFolderInfo)
                {
                    string path = i;
                    try
                    {
                        double overallProgress = 0.0;
                        double count1 = vFolderInfo.Count();
                        double increment = 100.0 / (count1);
                        string Bucket_Folder = Global.P_Html + path.Substring(path.LastIndexOf("\\") + 1);
                        string buckt_F = Bucket_Folder.Replace(" ", "_");
                        await Upload_Dal.UploadDirAsync(s3Client, Global.bucketName, path, "*.*", buckt_F);
                        overallProgress += increment;
                        folder_Uploaded--;
                        // Update the progress bar on the UI thread
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            countLabel.Content = $"Pending Folders:{folder_Uploaded}";
                            progressBar.Value = overallProgress;
                        });

                    }
                    catch (Exception ex)
                    {
                        // Handle any exception that occurred during the upload process
                        System.Windows.MessageBox.Show("An error occurred during Folder upload: " + ex.Message);
                    }
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    countLabel.Content = $"{0}%";
                    progressBar.Value = 0.0;
                    System.Windows.MessageBox.Show("Folder was uploaded successfully");
                });
            }
            else if (selectedValue == "Mp4")
            {
                int folder_Uploaded = Convert.ToInt32(count1);

                foreach (string i in vFolderInfo)
                {
                    string path = i;
                    try
                    {
                        double overallProgress = 0.0;
                        double count1 = vFolderInfo.Count();
                        double increment = 100.0 / (count1);
                        string Bucket_Folder = Global.P_Mp4 + path.Substring(path.LastIndexOf("\\") + 1);
                        string buckt_F = Bucket_Folder.Replace(" ", "_");
                        await Upload_Dal.UploadDirAsync(s3Client, Global.bucketName, path, "*.*", buckt_F);
                        overallProgress += increment;
                        folder_Uploaded--;
                        // Update the progress bar on the UI thread
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            countLabel.Content = $"Pending Folders:{folder_Uploaded}";
                            progressBar.Value = overallProgress;
                        });

                    }
                    catch (Exception ex)
                    {
                        // Handle any exception that occurred during the upload process
                        System.Windows.MessageBox.Show("An error occurred during Folder upload: " + ex.Message);
                    }
                }
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    countLabel.Content = $"{0}%";
                    progressBar.Value = 0.0;
                    System.Windows.MessageBox.Show("Folder was uploaded successfully");
                });
            }
        }    

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                UpdatePageResults();
            }
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                UpdatePageResults();
            }
        }

        private void UpdatePageResults()
        {
            List<string> pageResults = vfilename.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();
            Folder_Info.ItemsSource = pageResults;
        }

        //private static async Task TrackMPUAsync(IAmazonS3 pClient, string pBucketName, string pKeyName, string pFilePath, System.Windows.Controls.ProgressBar progressBar, int increment)
        //{
        //    try
        //    {
        //        var fileTransferUtility = new TransferUtility(pClient);

        //        var uploadRequest = new TransferUtilityUploadRequest
        //        {
        //            BucketName = pBucketName,
        //            FilePath = pFilePath,
        //            Key = pKeyName
        //        };

        //        uploadRequest.UploadProgressEvent += (sender, e) =>
        //        {
        //            // Calculate the progress for the current file
        //            double fileProgress = (e.TransferredBytes * increment) / e.TotalBytes;

        //            // Update the progress bar on the UI thread
        //            System.Windows.Application.Current.Dispatcher.Invoke(() =>
        //            {
        //                progressBar.Value += fileProgress;
        //            });
        //        };

        //        await fileTransferUtility.UploadAsync(uploadRequest);
        //        Console.WriteLine("Upload completed");
        //    }
        //    catch (AmazonS3Exception e)
        //    {
        //        Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
        //    }
        //}

        private async Task TrackMPUAsync_folder(IAmazonS3 pClient, string pBucketName, string pKeyName, string pFilePath, System.Windows.Controls.ProgressBar progressBar, double increment)
        {
            try
            {
                var fileTransferUtility = new TransferUtility(pClient);

                var uploadDirectoryRequest = new TransferUtilityUploadDirectoryRequest
                {
                    BucketName = pBucketName,
                    Directory = pFilePath,
                    SearchPattern = "*.*",
                    SearchOption = SearchOption.AllDirectories,
                    KeyPrefix = pKeyName
                };

                uploadDirectoryRequest.UploadDirectoryProgressEvent += (sender, e) =>
                {
                    // Calculate the progress for the current file
                    double fileProgress = (e.TransferredBytes * increment) / e.TotalBytes;

                    // Update the progress bar on the UI thread
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressBar.Value += fileProgress;
                    });
                };

                await fileTransferUtility.UploadDirectoryAsync(uploadDirectoryRequest);
                Console.WriteLine("Upload completed");
            }
            catch (AmazonS3Exception e)
            {
            }
            catch (Exception e)
            {
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (file_Types.SelectedItem != null)
            {
                TextBlock selectedTextBlock = file_Types.SelectedItem as TextBlock;
                string selectedValue = selectedTextBlock.Text;

                if (selectedValue == "Videos")
                {
                    browse_Files.IsEnabled = false;
                    browse.IsEnabled = true;
                }
                else
                {
                    browse_Files.IsEnabled = true;
                    browse.IsEnabled = false;
                }
            }
        }

        private async void resume_Click(object sender, RoutedEventArgs e)
        {
            string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string csvFilePath = System.IO.Path.Combine(downloadsFolder, "RemainingFiles.csv");

            List<FilePathInfo> filepaths = new List<FilePathInfo>();

            if (File.Exists(csvFilePath))
            {
                using (var reader = new StreamReader(csvFilePath))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    filepaths.AddRange(csv.GetRecords<FilePathInfo>());
                }
            }

            foreach (var type in filepaths)
            {
                if (type.Type == "File")
                {
                    Type = "File";
                    List<string> vfilenameList = new List<string>();
                    List<string> vfilepathList = new List<string>();

                    foreach (var pathInfo in filepaths)
                    {
                        string filename = pathInfo.FilePath.Substring(pathInfo.FilePath.LastIndexOf('\\') + 1);
                        string filepath = pathInfo.FilePath.ToString();
                        Global.key = pathInfo.Location.ToString();

                        // Add the filename to the List<string>
                        vfilenameList.Add(filename);
                        vfilepathList.Add(filepath);
                    }

                    // If you really need it as an array, you can convert the List to a string array.
                    vfilename = vfilenameList.ToArray();
                    vFilesInfo = vfilepathList.ToArray();
                }
                else if (type.Type == "Folder")
                {
                    Type = "Folder";
                    List<string> vfolderList = new List<string>();
                    foreach (var pathInfo in filepaths)
                    {
                        string filepath = pathInfo.FilePath.ToString();

                        // Add the filename to the List<string>
                        vfolderList.Add(filepath);
                    }
                    SelectedValue = "Videos";
                    vFolderInfo = vfolderList.ToArray();
                }                
            }
            if (Type == "File")
            {
               await upload_Files();
            }
            else if (Type == "Folder")
            {
              await  Upload_Folder();
            }
        }
    }
    public class FilePathInfo
    {
        public string FilePath { get; set; }

        public string Location { get; set; }

        public string Type { get; set; }    
    }
}



   