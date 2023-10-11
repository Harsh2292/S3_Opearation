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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Amazon.S3.Model;
using Amazon.S3;
using Dal;
using static System.Net.Mime.MediaTypeNames;
using Amazon.S3.Transfer;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using System.Data;
using System.Collections.Concurrent;
using Amazon.Elasticsearch;
using Amazon.Elasticsearch.Model;
using Nest;
using System.Drawing;
using System.Net;
using System.Data.SqlClient;
using System.Collections.ObjectModel;
using MaterialDesignThemes.Wpf;
using Org.BouncyCastle.Utilities.Collections;

namespace S3_Opearation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Controls.UserControl
    {
        List<SearchResult> results = new List<SearchResult>();
        List<SearchResult> result1 = new List<SearchResult>();
        List<SearchResult> pageResults = new List<SearchResult>();
        string path;
        int itemsPerPage;
        int currentPage;
        private int totalItems1;
        int totalItems;
        int totalPages;
        private int totalPages1;
        string[] folderName;
        public static string? connectionString;
        public string flg;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void Move_Click(object sender, RoutedEventArgs e)
        {
            MoveFile();
        }
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            results.Clear();
            result1.Clear();
            searched_Files.ItemsSource = null;
            searched_Files.Items.Clear();
            searched_Folders.ItemsSource = null;
            searched_Folders.Items.Clear();
        }
        private void Download_Click(object sender, RoutedEventArgs e)
        {
            DownloadSelectedFile();
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            delete_file();
        }
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            if (All.IsChecked == true)
            {
                SearchS3File();
            }
            else
            {
                filtered_Search();
            }
        }

        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public async Task SearchS3File()
        {
            string connectionString = Datautil.connectionString;
            new ComUtils().loadGlobalData();

            if (!string.IsNullOrEmpty(search_File.Text))
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                var s3Client = new AmazonS3Client(Global.Accesskey, Global.SecurityKey, Global.primaryRegion);

                string query = search_File.Text;
                HashSet<string> fileNames = new HashSet<string>(query.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

                List<SearchResult> searchResults = new List<SearchResult>();
                List<SearchResult> searchFolders = new List<SearchResult>();
                HashSet<string> processedFolders = new HashSet<string>();

                bool hasMatchingKeysInDatabase = false; // Flag to check if there are matching keys in the database
                foreach (string name in fileNames)
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        // Check if any keys in the database contain the search term
                        using (var command = new SqlCommand("SELECT COUNT(*) FROM TblS3ObjectKeys WHERE S3_keys LIKE '%' + @SearchTerm + '%'", connection))
                        {
                            command.Parameters.AddWithValue("@SearchTerm", name);
                            int count = (int)command.ExecuteScalar();
                            hasMatchingKeysInDatabase = count > 0;
                        }
                    }
                }
                foreach (string Fname in fileNames)
                {
                    if (hasMatchingKeysInDatabase)
                    {
                        // Show matching keys from the database in the data grid
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            using (var command = new SqlCommand("SELECT * FROM TblS3ObjectKeys WHERE  S3_keys LIKE '%' + @SearchTerm + '%' ", connection))
                            {
                                command.Parameters.AddWithValue("@SearchTerm", Fname);
                                using (var reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string key = reader["S3_keys"].ToString();
                                        string[] parts = key.Split('/');
                                        string name = parts[^1];
                                        searchResults.Add(new SearchResult
                                        {
                                            IsSelected = true,
                                            Key = key,
                                            Name = name,
                                        });
                                        string folder = key.Substring(0, key.LastIndexOf('/'));
                                        string Folder_name = folder.Substring(folder.LastIndexOf('/') + 1);
                                        lock (processedFolders)
                                        {
                                            if (processedFolders.Add(folder))
                                            {
                                                searchFolders.Add(new SearchResult
                                                {
                                                    IsChecked = true,
                                                    folderName = Folder_name,
                                                    Key = folder,
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        string continuationToken = null;
                        const int pageSize = 1000; // Adjust the batch size as per your requirements

                        var allObjectKeys = new ConcurrentBag<string>();
                        do
                        {
                            var fetchedKeys = new List<string>(); // List to store the fetched keys in the current batch

                            var listRequest = new ListObjectsV2Request
                            {
                                BucketName = Global.bucketName,
                                MaxKeys = pageSize,
                                ContinuationToken = continuationToken
                            };

                            var listResponse = await s3Client.ListObjectsV2Async(listRequest);

                            // Store the fetched keys in the current batch
                            foreach (var s3Object in listResponse.S3Objects)
                            {
                                fetchedKeys.Add(s3Object.Key);
                            }

                            // Insert the current batch of keys into the database
                            using (var connection = new SqlConnection(connectionString))
                            {
                                connection.Open();

                                foreach (var key in fetchedKeys)
                                {
                                    string[] parts = key.Split('/');
                                    string flg = parts[0];
                                    using (var command = new SqlCommand("INSERT INTO TblS3ObjectKeys (S3_keys, flg) SELECT @S3_keys, @flg WHERE NOT EXISTS (SELECT 1  FROM TblS3ObjectKeys  WHERE S3_keys = @S3_keys)", connection))
                                    {
                                        command.Parameters.AddWithValue("@S3_keys", key);
                                        command.Parameters.AddWithValue("@flg", flg);
                                        command.ExecuteNonQuery();
                                    }
                                }
                            }
                            continuationToken = listResponse.NextContinuationToken;
                        } while (continuationToken != null);
                        using (var connection = new SqlConnection(connectionString))
                        {
                            //connection.Open();
                            using (var command = new SqlCommand("SELECT * FROM TblS3ObjectKeys WHERE S3_keys LIKE '%' + @SearchTerm + '%'", connection))
                            {
                                command.Parameters.AddWithValue("@SearchTerm", Fname);
                                using (var reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string key = reader["S3_keys"].ToString();
                                        string[] parts = key.Split('/');
                                        string name = parts[^1];
                                        searchResults.Add(new SearchResult
                                        {
                                            IsSelected = true,
                                            Key = key,
                                            Name = name,
                                        });
                                    }
                                }
                            }
                            connection.Close();
                        }
                    }
                }
                Mouse.OverrideCursor = null;

                itemsPerPage = 15;
                currentPage = 1;
                totalItems = searchResults.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

                List<SearchResult> pageResults = searchResults
                    .Skip((currentPage - 1) * itemsPerPage)
                    .Take(itemsPerPage)
                    .ToList();

                result1 = searchResults;
                results = searchFolders;
                searched_Folders.ItemsSource = results;
                searched_Files.ItemsSource = pageResults;
            }
            else
            {
                System.Windows.MessageBox.Show("Search for a proper file");
            }
        }

        public async void filtered_Search()
        {
            searched_Files.ItemsSource = null;
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            new ComUtils().loadGlobalData();

            if (Image_file.IsChecked == true)
            {
                path = Global.P_Imagepath;
                await Search_pathvise();
                Mouse.OverrideCursor = null;
                itemsPerPage = 20;
                currentPage = 1;
                totalItems = result1.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
                List<SearchResult> pageResults = result1.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();
                searched_Files.ItemsSource = pageResults;
            }

            else if (videos_file.IsChecked == true)
            {
                path = Global.P_Video;
                await Search_pathvise();
                Mouse.OverrideCursor = null;
                itemsPerPage = 20;
                currentPage = 1;
                totalItems = result1.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
                List<SearchResult> pageResults = result1.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();
                searched_Files.ItemsSource = pageResults;
            }
            else if (certi_file.IsChecked == true)
            {
                path = Global.P_Certi;
                await Search_pathvise();
                Mouse.OverrideCursor = null;
                itemsPerPage = 20;
                currentPage = 1;
                totalItems = result1.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
                List<SearchResult> pageResults = result1.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();
                searched_Files.ItemsSource = pageResults;
            }
            else if (MaskLabreportno_file.IsChecked == true)
            {
                path = Global.P_MaskLab;
                await Search_pathvise();
                Mouse.OverrideCursor = null;
                itemsPerPage = 20;
                currentPage = 1;
                totalItems = result1.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
                List<SearchResult> pageResults = result1.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();
                searched_Files.ItemsSource = pageResults;
            }
            else if (ActualProportions_file.IsChecked == true)
            {
                path = Global.P_ActualProp;
                await Search_pathvise();
                Mouse.OverrideCursor = null;
                itemsPerPage = 20;
                currentPage = 1;
                totalItems = result1.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
                List<SearchResult> pageResults = result1.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();
                searched_Files.ItemsSource = pageResults;
            }
            else if (Html_file.IsChecked == true)
            {
                path = Global.P_Html;
                await Search_pathvise();
                Mouse.OverrideCursor = null;
                itemsPerPage = 20;
                currentPage = 1;
                totalItems = result1.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
                List<SearchResult> pageResults = result1.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();
                searched_Files.ItemsSource = pageResults;
            }
            else if (Mp4_file.IsChecked == true)
            {
                path = Global.P_Mp4;
                await Search_pathvise();
                Mouse.OverrideCursor = null;
                itemsPerPage = 20;
                currentPage = 1;
                totalItems = result1.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);
                List<SearchResult> pageResults = result1.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();
                searched_Files.ItemsSource = pageResults;
            }
            else
            {
                System.Windows.MessageBox.Show("Please enter the data");
                Mouse.OverrideCursor = null;
            }
        }

        public async Task Search_pathvise()
        {
            string connectionString = Datautil.connectionString;
            new ComUtils().loadGlobalData();

            if (!string.IsNullOrEmpty(search_File.Text))
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                var s3Client = new AmazonS3Client(Global.Accesskey, Global.SecurityKey, Global.primaryRegion);

                string query = search_File.Text;
                HashSet<string> fileNames = new HashSet<string>(query.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

                List<SearchResult> searchResults = new List<SearchResult>();
                List<SearchResult> searchFolders = new List<SearchResult>();
                HashSet<string> processedFolders = new HashSet<string>();

                bool hasMatchingKeysInDatabase = false; // Flag to check if there are matching keys in the database
                foreach (string name in fileNames)
                {
                    using (var connection = new SqlConnection(connectionString))
                    {
                        //string m_flg = "Moved";

                        connection.Open();
                        // Check if any keys in the database contain the search term
                        using (var command = new SqlCommand("SELECT COUNT(*) FROM TblS3ObjectKeys WHERE S3_keys LIKE '%' + @SearchTerm + '%'", connection))
                        {
                            command.Parameters.AddWithValue("@SearchTerm", name);
                            //command.Parameters.AddWithValue("@m_flg", m_flg);
                            int count = (int)command.ExecuteScalar();
                            hasMatchingKeysInDatabase = count > 0;
                        }
                    }
                }

                foreach (string Fname in fileNames)
                {
                    if (hasMatchingKeysInDatabase)
                    {
                        string m_flg = "Moved";
                        string flg = path.Substring(0, path.LastIndexOf('/'));
                        // Show matching keys from the database in the data grid
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            using (var command = new SqlCommand("SELECT * FROM TblS3ObjectKeys WHERE S3_keys LIKE '%' + @SearchTerm + '%'", connection))
                            {
                                command.Parameters.AddWithValue("@SearchTerm", Fname);
                                using (var reader = command.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string key = reader["S3_keys"].ToString();
                                        string[] parts = key.Split('/');
                                        string name = parts[^1];
                                        searchResults.Add(new SearchResult
                                        {
                                            IsSelected = true,
                                            Key = key,
                                            Name = name,
                                        });
                                        string folder = key.Substring(0, key.LastIndexOf('/'));
                                        string Folder_name = folder.Substring(folder.LastIndexOf('/') + 1);
                                        lock (processedFolders)
                                        {
                                            if (processedFolders.Add(folder))
                                            {
                                                searchFolders.Add(new SearchResult
                                                {
                                                    IsChecked = true,
                                                    folderName = Folder_name,
                                                    Key = folder,
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                            connection.Close();
                        }
                    }
                    else
                    {
                        var confirmationResult = System.Windows.Forms.MessageBox.Show("Are you sure you want to get objects from S3?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (confirmationResult == DialogResult.Yes)
                        {
                            string continuationToken = null;
                            const int pageSize = 1000; // Adjust the batch size as per your requirements

                            var allObjectKeys = new ConcurrentBag<string>();
                            do
                            {
                                var fetchedKeys = new List<string>(); // List to store the fetched keys in the current batch

                                var listRequest = new ListObjectsV2Request
                                {
                                    BucketName = Global.bucketName,
                                    MaxKeys = pageSize,
                                    ContinuationToken = continuationToken
                                };

                                var listResponse = await s3Client.ListObjectsV2Async(listRequest);

                                // Store the fetched keys in the current batch
                                foreach (var s3Object in listResponse.S3Objects)
                                {
                                    fetchedKeys.Add(s3Object.Key);
                                }

                                // Insert the current batch of keys into the database
                                using (var connection = new SqlConnection(connectionString))
                                {
                                    connection.Open();

                                    foreach (var key in fetchedKeys)
                                    {
                                        string[] parts = key.Split('/');
                                        string flg = parts[0];
                                        using (var command = new SqlCommand("INSERT INTO TblS3ObjectKeys (S3_keys, flg) SELECT @S3_keys, @flg WHERE NOT EXISTS (SELECT 1  FROM TblS3ObjectKeys  WHERE S3_keys = @S3_keys)", connection))
                                        {
                                            command.Parameters.AddWithValue("@S3_keys", key);
                                            command.Parameters.AddWithValue("@flg", flg);
                                            command.ExecuteNonQuery();
                                        }
                                    }
                                }
                                continuationToken = listResponse.NextContinuationToken;
                            } while (continuationToken != null);
                            using (var connection = new SqlConnection(connectionString))
                            {
                                string m_flg = "Moved";
                                connection.Open();
                                using (var command = new SqlCommand("SELECT * FROM TblS3ObjectKeys WHERE  S3_keys LIKE '%' + @SearchTerm + '%'", connection))
                                {
                                    command.Parameters.AddWithValue("@SearchTerm", Fname);
                                    using (var reader = command.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            string key = reader["S3_keys"].ToString();
                                            string[] parts = key.Split('/');
                                            string name = parts[^1];
                                            searchResults.Add(new SearchResult
                                            {
                                                IsSelected = true,
                                                Key = key,
                                                Name = name,
                                            });
                                            string folder = key.Substring(0, key.LastIndexOf('/'));
                                            string Folder_name = folder.Substring(folder.LastIndexOf('/') + 1);
                                            lock (processedFolders)
                                            {
                                                if (processedFolders.Add(folder))
                                                {
                                                    searchFolders.Add(new SearchResult
                                                    {
                                                        IsChecked = true,
                                                        folderName = Folder_name,
                                                        Key = folder,                                                        
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                                connection.Close();
                            }

                        }
                    }
                }
                Mouse.OverrideCursor = null;

                itemsPerPage = 15;
                currentPage = 1;
                totalItems = searchResults.Count;
                totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

                List<SearchResult> pageResults = searchResults
                    .Skip((currentPage - 1) * itemsPerPage)
                    .Take(itemsPerPage)
                    .ToList();

                result1 = searchResults;
                results = searchFolders;
                searched_Folders.ItemsSource = results;
                searched_Files.ItemsSource = pageResults;
            }
            else
            {
                System.Windows.MessageBox.Show("Search for a proper file");
            }
        }


        //public async Task SearchFiles()
        //{
        //    new ComUtils().loadGlobalData();

        //    string query = search_File.Text;

        //    if (string.IsNullOrEmpty(query))
        //    {
        //        System.Windows.MessageBox.Show("Search for a proper file");
        //        return;
        //    }

        //    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

        //    var s3Client = new AmazonS3Client(Global.Accesskey, Global.SecurityKey, Global.primaryRegion);
        //    var searchResults = new List<SearchResult>();

        //    try
        //    {
        //        var fileNames = new HashSet<string>(query.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);

        //        var allObjectKeys = new ConcurrentBag<string>();

        //        string continuationToken = null;
        //        do
        //        {
        //            var listRequest = new ListObjectsV2Request
        //            {
        //                BucketName = Global.bucketName,
        //                Prefix = path,
        //                MaxKeys = 1000,
        //                ContinuationToken = continuationToken
        //            };

        //            var listResponse = await s3Client.ListObjectsV2Async(listRequest);

        //            // Use parallel processing to add object keys to the ConcurrentBag
        //            Parallel.ForEach(listResponse.S3Objects, s3Object =>
        //            {                       
        //                    allObjectKeys.Add(s3Object.Key);                        
        //            });

        //            continuationToken = listResponse.NextContinuationToken;
        //        } while (continuationToken != null);

        //        // Search and compare the file names using the stored object keys (HashSet.Contains is faster)
        //        var matchingFiles = allObjectKeys
        //            .Where(key => fileNames.Contains(System.IO.Path.GetFileName(key)))
        //            .ToList();

        //        // Process the matching files
        //        searchResults.AddRange(matchingFiles
        //            .Select(key =>
        //            {
        //                string[] parts = key.Split('/');
        //                string fileName = parts[^1];
        //                string folder = string.Join("/", parts[..^1]);

        //                return new SearchResult
        //                {
        //                    IsSelected = true,
        //                    Name = fileName,
        //                    Key = key,
        //                    Folder = folder,
        //                    Type = "File"
        //                };
        //            }));

        //        Mouse.OverrideCursor = null;

        //        itemsPerPage = 15;
        //        currentPage = 1;
        //        totalItems = searchResults.Count;
        //        totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

        //        result1 = searchResults;
        //        searched_Files.ItemsSource = searchResults
        //            .Skip((currentPage - 1) * itemsPerPage)
        //            .Take(itemsPerPage)
        //            .ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Windows.MessageBox.Show($"An error occurred while searching: {ex.Message}");
        //    }
        //    finally
        //    {
        //        Mouse.OverrideCursor = null;
        //    }
        //}


        ////public async Task SearchFiles_Select()
        ////    {
        ////        new ComUtils().loadGlobalData();

        ////        string query = search_File.Text;

        ////        if (string.IsNullOrEmpty(query))
        ////        {
        ////            System.Windows.MessageBox.Show("Search for a proper file");
        ////            return;
        ////        }

        ////        Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

        ////        var s3Client = new AmazonS3Client(Global.Accesskey, Global.SecurityKey, Global.primaryRegion);
        ////        var searchResults = new List<SearchResult>();

        ////        try
        ////        {
        ////            var allFilesAndFolders = new List<S3Object>();

        ////            var request = new SelectObjectContentRequest
        ////            {
        ////                Bucket = Global.bucketName,      
        ////                Key=path,
        ////                Expression = "SELECT * FROM S3Object ", // Fetches the first column (object key) from each object
        ////                 ExpressionType = ExpressionType.SQL,
        ////                InputSerialization = new InputSerialization
        ////                {
        ////                    // The InputSerialization depends on the format of your data.
        ////                    JSON = new JSONInput
        ////                    {
        ////                        JsonType = "LINES"
        ////                    }
        ////                },
        ////                OutputSerialization = new OutputSerialization
        ////                {
        ////                    JSON = new JSONOutput()
        ////                },
        ////              // Use the continuation token as the start point for subsequent requests
        ////            };
        ////            var response = await s3Client.SelectObjectContentAsync(request);

        ////            if (response != null && response.Payload != null)
        ////            {
        ////                using (var responseStream = response.Payload)
        ////                {
        ////                    using (var reader = new StreamReader((Stream)responseStream))
        ////                    {
        ////                        while (!reader.EndOfStream)
        ////                        {
        ////                            var line = await reader.ReadLineAsync();
        ////                            if (!string.IsNullOrWhiteSpace(line))
        ////                            {
        ////                                // Each line contains the object key, so add it to the list
        ////                                allFilesAndFolders.Add(new S3Object { Key = line });
        ////                            }
        ////                        }
        ////                    }
        ////                }
        ////            }
        ////            else
        ////            {
        ////                // Handle the case when the response or payload is null
        ////                // You may show a message or take appropriate actions
        ////            }

        ////            // Get all the file objects
        ////            var fileObjects = allFilesAndFolders
        ////                .Where(obj => !obj.Key.EndsWith("/"))
        ////                .ToList();

        ////            // Process the matching files
        ////            searchResults.AddRange(fileObjects
        ////                .Select(obj =>
        ////                {
        ////                    string[] parts = obj.Key.Split('/');
        ////                    string fileName = parts[^1];
        ////                    string folder = string.Join("/", parts[..^1]);

        ////                    return new SearchResult
        ////                    {
        ////                        IsSelected = true,
        ////                        Name = fileName,
        ////                        Key = obj.Key,
        ////                        Folder = folder,
        ////                        Type = "File"
        ////                    };
        ////                }));

        ////            Mouse.OverrideCursor = null;

        ////            itemsPerPage = 15;
        ////            currentPage = 1;
        ////            totalItems = searchResults.Count;
        ////            totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

        ////            result1 = searchResults;
        ////            searched_Files.ItemsSource = searchResults
        ////                .Skip((currentPage - 1) * itemsPerPage)
        ////                .Take(itemsPerPage)
        ////                .ToList();
        ////        }
        ////        catch (Exception ex)
        ////        {
        ////            System.Windows.MessageBox.Show($"An error occurred while searching: {ex.Message}");
        ////        }
        ////        finally
        ////        {
        ////            Mouse.OverrideCursor = null;
        ////        }
        ////    }


        //public async Task datafillAsync()
        //{
        //    new ComUtils().loadGlobalData();

        //    if (!string.IsNullOrEmpty(search_File.Text))
        //    {
        //        searched_Files.ItemsSource = null;
        //        Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        //        var s3Client = new AmazonS3Client(Global.Accesskey, Global.SecurityKey, Global.primaryRegion);

        //        string query = search_File.Text;
        //        string[] fileNames = query.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        //        HashSet<string> searchFileNames = new HashSet<string>(fileNames);

        //        ConcurrentBag<SearchResult> searchResults = new ConcurrentBag<SearchResult>();
        //        HashSet<string> processedFolders = new HashSet<string>();

        //        string continuationToken = null;
        //        const int pageSize = 1000000; // Adjust the batch size as per your requirements
        //        try
        //        {
        //            ListObjectsV2Request listRequest = new ListObjectsV2Request
        //            {
        //                BucketName = Global.bucketName,
        //                Prefix = path,
        //                MaxKeys = pageSize,
        //                ContinuationToken = continuationToken
        //            };

        //            ListObjectsV2Response listResponse;

        //            do
        //            {
        //                listResponse = await s3Client.ListObjectsV2Async(listRequest);

        //                await Task.Run(() =>
        //                {
        //                    Parallel.ForEach(listResponse.S3Objects, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, obj =>
        //                    {
        //                        foreach (string fileName in searchFileNames)
        //                        {
        //                            if (obj.Key.Contains(fileName.Trim()))
        //                            {
        //                                string folderName = obj.Key.Substring(0, obj.Key.LastIndexOf('/'));
        //                                string name = folderName.Substring(folderName.LastIndexOf('/') + 1);
        //                                string folderType = folderName.EndsWith("/") ? "File" : "Folder";

        //                                lock (processedFolders)
        //                                {
        //                                    if (processedFolders.Add(folderName))
        //                                    {
        //                                        searchResults.Add(new SearchResult
        //                                        {
        //                                            IsChecked = true,
        //                                            folderName = name,
        //                                            Key = folderName,
        //                                            Type = folderType
        //                                        });
        //                                    }
        //                                }
        //                                break; // No need to check further file names for this object
        //                            }
        //                        }
        //                    });
        //                });

        //                listRequest.ContinuationToken = listResponse.NextContinuationToken;
        //            } while (listResponse.IsTruncated);

        //            results = searchResults.ToList();
        //            searched_Folders.ItemsSource = results;
        //        }
        //        catch (Exception ex)
        //        {
        //            System.Windows.MessageBox.Show($"An error occurred while searching: {ex.Message}");
        //        }
        //        finally
        //        {
        //            Mouse.OverrideCursor = null;
        //        }
        //    }
        //    else
        //    {
        //        System.Windows.MessageBox.Show("Search for a proper Folder");
        //    }
        //}

        private void DownloadSelectedFile()
        {
            string connectionString = Datautil.connectionString;

            var confirmationResult = System.Windows.Forms.MessageBox.Show("Are you sure you want to perform the Download operation?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmationResult == DialogResult.Yes)
            {
                new ComUtils().loadGlobalData();

                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                bool isAnyFileSelected = false;

                foreach (SearchResult result in result1)
                {
                    if (result.IsSelected)
                    {
                        isAnyFileSelected = true;
                        var s3Client = new AmazonS3Client(Global.Accesskey, Global.SecurityKey, Global.primaryRegion);
                        string BucketName = Global.bucketName;
                        string selectedKey = result.Key;
                        string Download = "Yes";
                        string User = Global.UserName.ToUpper();
                        string LocalFilePath = @"C:\Downloads\" + result.Name;
                        var transferUtility = new TransferUtility(s3Client);
                        transferUtility.Download(LocalFilePath, BucketName, selectedKey);
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            // Update rows in the database that contain the search term
                            using (var command = new SqlCommand("UPDATE TblS3ObjectKeys SET Dwnld=@Download, Usr=@User WHERE S3_keys LIKE '%' + @SearchTerm + '%'", connection))
                            {
                                command.Parameters.AddWithValue("@SearchTerm", selectedKey);
                                command.Parameters.AddWithValue("@Download", Download);
                                command.Parameters.AddWithValue("@User", User);

                                int rowsAffected = command.ExecuteNonQuery();

                                Console.WriteLine($"{rowsAffected} rows updated.");
                            }
                            connection.Close();
                        }
                    }
                }

                Mouse.OverrideCursor = null;

                if (isAnyFileSelected)
                {
                    System.Windows.MessageBox.Show("File was downloaded successfully");
                }
                else
                {
                    System.Windows.MessageBox.Show("Please select the File/Files first!!!");
                }
            }

        }
        public async void delete_file()
        {
            string connectionString = Datautil.connectionString;

            var confirmationResult = System.Windows.Forms.MessageBox.Show("Are you sure you want to perform the Delete operation?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmationResult == DialogResult.Yes)
            {
                new ComUtils().loadGlobalData();

                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                bool isAnyFileSelected = false;

                foreach (SearchResult result in result1)
                {
                    if (result.IsSelected)
                    {
                        isAnyFileSelected = true;
                        var s3Client = new AmazonS3Client(Global.Accesskey, Global.SecurityKey, Global.primaryRegion);
                        var filePath = result.Key;
                        var deleteRequest = new DeleteObjectRequest { BucketName = Global.bucketName, Key = filePath };
                        await s3Client.DeleteObjectAsync(deleteRequest);
                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            // Delete rows from the database that contain the search term
                            using (var command = new SqlCommand("DELETE FROM TblS3ObjectKeys WHERE S3_keys LIKE '%' + @SearchTerm + '%'", connection))
                            {
                                command.Parameters.AddWithValue("@SearchTerm", filePath);
                            }
                            connection.Close();
                        }
                    }
                }
                results.Clear();
                searched_Folders.ItemsSource = null;
                searched_Folders.Items.Clear();
                result1.Clear();
                searched_Files.ItemsSource = null;
                searched_Files.Items.Clear();
                Mouse.OverrideCursor = null;

                if (isAnyFileSelected)
                {
                    System.Windows.MessageBox.Show("File was deleted successfully");
                }
                else
                {
                    System.Windows.MessageBox.Show("Please select the File/Files first!!!");
                }
            }

        }
        public async void MoveFile()
        {
            string connectionString = Datautil.connectionString;
            string User = Global.UserName;
            var confirmationResult = System.Windows.Forms.MessageBox.Show("Are you sure you want to perform the Move operation?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmationResult == DialogResult.Yes)
            {
                string mflg = "Moved";
                new ComUtils().loadGlobalData();

                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                var s3Client = new AmazonS3Client(Global.Accesskey, Global.SecurityKey, Global.backupRegion);
                bool isAnyFileSelected = false;
                // Loop through the search results list to find the selected files
                try
                {
                    foreach (SearchResult result in result1)
                    {
                        if (result.IsSelected)
                        {
                            var copyRequest = new CopyObjectRequest
                            {
                                SourceBucket = Global.bucketName,
                                SourceKey = result.Key,
                                DestinationBucket = Global.BackupBucket,
                                DestinationKey = result.Key
                            };

                            // Call the Amazon S3 CopyObjectAsync method to perform the copy operation
                            var response = await s3Client.CopyObjectAsync(copyRequest);
                            var request = new DeleteObjectRequest
                            {
                                BucketName = Global.bucketName,
                                Key = result.Key
                            };
                            var response1 = await s3Client.DeleteObjectAsync(request);
                            using (var connection = new SqlConnection(connectionString))
                            {
                                connection.Open();
                                // Delete rows from the database that contain the search term
                                using (var command = new SqlCommand("Update TblS3ObjectKeys SET m_flg=@m_flg , Usr=@User where S3_keys=@SearchTerm", connection))
                                {
                                    command.Parameters.AddWithValue("@SearchTerm", result.Key);
                                    command.Parameters.AddWithValue("@m_flg", mflg);
                                    command.Parameters.AddWithValue("@User", User);
                                }
                                connection.Close();
                            }
                            isAnyFileSelected = true;
                        }
                    }
                }
                catch
                {
                    System.Windows.MessageBox.Show("This file/folder has been deleted");
                }
                Mouse.OverrideCursor = null;
                searched_Files.ItemsSource = null;
                searched_Files.Items.Clear();
                results.Clear();
                searched_Folders.ItemsSource = null;
                searched_Folders.Items.Clear();
                result1.Clear();

                if (isAnyFileSelected)
                {
                    System.Windows.MessageBox.Show("File was moved successfully");
                }
                else
                {
                    System.Windows.MessageBox.Show("Please select the File/Files first!!!");
                }
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
            List<SearchResult> pageResults = result1.Skip((currentPage - 1) * itemsPerPage).Take(itemsPerPage).ToList();
            searched_Files.ItemsSource = pageResults;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Global.Delete_permission == true)
            {
                Delete.IsEnabled = true;
            }
            else
            {
                Delete.IsEnabled = false;
            }

            if (Global.Download_permission == true)
            {
                Download.IsEnabled = true;

            }
            else
            {
                Download.IsEnabled = false;
            }

            if (Global.Search_permission == true)
            {
                Search.IsEnabled = true;
            }
            else
            {
                Search.IsEnabled = false;
            }

            if (Global.Move_permission == true)
            {
                Move.IsEnabled = true;
            }
            else
            {
                Move.IsEnabled = false;
            }
        }

        private void search_File_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (search_File.Text == "Search files")
            {
                search_File.Text = string.Empty;
            }
        }
        private void selectAllCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Iterate over the items in result1 list and set IsSelected property to true
            foreach (var item in result1)
            {
                item.IsSelected = true;
            }
            UpdatePageResults();
        }

        private void selectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Iterate over the items in result1 list and set IsSelected property to false
            foreach (var item in result1)
            {
                item.IsSelected = false;
            }
            UpdatePageResults();
        }
        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (true)
            {
                DependencyObject parentObject = VisualTreeHelper.GetParent(child);

                if (parentObject == null)
                {
                    return null;
                }

                if (parentObject is T parent)
                {
                    return parent;
                }

                child = parentObject;
            }
        }

        // Helper method to find the visual child of a specific type
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child != null && child is T childType)
                {
                    return childType;
                }

                T childOfChild = FindVisualChild<T>(child);

                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }


        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            SearchResult srchResult = new SearchResult();
            if (srchResult.IsChecked == true)
            {
                if (srchResult.Key.Contains(srchResult.folderName))
                {
                    srchResult.IsSelected = true;
                }
            }
        }

        private void searched_Folders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            SearchResult srchResult = row.DataContext as SearchResult;
            foreach (SearchResult result in result1)
            {
                result.IsSelected = false;
            }
            searched_Files.ItemsSource = result1;
            if (srchResult != null)
            {

                // Access the selected row's data and perform desired action
                string folderName = srchResult.Key + "/";

                // Check if 'result1' list contains a SearchResult with the same 'folderName'
                foreach (SearchResult result in result1)
                {
                    if (result.Key.Contains(folderName))
                    {
                        result.IsSelected = true;
                    }
                }
                searched_Files.ItemsSource = null;
                searched_Files.ItemsSource = result1;

            }
        }
        private T FindVisualChild2<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var foundChild = FindVisualChild<T>(child);
                if (foundChild != null)
                {
                    return foundChild;
                }
            }

            return null;
        }

    }
}
