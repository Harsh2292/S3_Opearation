using Dal;
using System;
using System.Collections;
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
    /// Interaction logic for Rule_Master.xaml
    /// </summary>
    public partial class Rule_Master : Page
    {
        public Rule_Master()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save_data();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            clearData();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(null);
        }

        public void Save_data()
        {
            DataSet dsMstr = new DataSet();
            dsMstr = new Rule_Mstr_DAL().getRuleMstr("", 0, 1);
            if (dsMstr.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in dsMstr.Tables[0].Rows)
                {
                    switch (dr["vRuleTyp"].ToString())
                    {
                        case "PRIMARY_BUCKET":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.bucketName = dr["sStr1"].ToString();
                                    dr["sStr1"] = P_Name.Text;
                                }
                                break;
                            }

                        case "BKUP_BUCKET":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.BackupBucket = dr["sStr1"].ToString();
                                    dr["sStr1"] = name.Text;
                                }
                                break;
                            }

                        case "PRIMARY_IMAGE_PTH":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.P_Imagepath = dr["sStr1"].ToString();
                                    dr["sStr1"] = p_Img.Text;
                                }
                                break;
                            }

                        case "PRIMARY_VIDEO_PTH":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.P_Video = dr["sStr1"].ToString();
                                    dr["sStr1"] = p_Video.Text;
                                }
                                break;
                            }

                        case "BKUP_IMAGE_PTH":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.B_Imagepath = dr["sStr1"].ToString();
                                    dr["sStr1"] = b_Img.Text;
                                }
                                break;
                            }

                        case "BKUP_VIDEO_PTH":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.B_Video = dr["sStr1"].ToString();
                                    dr["sStr1"] = b_Video.Text;
                                }
                                break;
                            }

                        case "PRIMARY_CERTI":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.P_Certi = dr["sStr1"].ToString();
                                    dr["sStr1"] = p_Certi.Text;
                                }
                                break;
                            }
                        case "PRIMARY_MASK":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.P_MaskLab = dr["sStr1"].ToString();
                                    dr["sStr1"] = p_Masklab.Text;
                                }
                                break;
                            }
                        case "PRIMARY_ActualProp":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.P_ActualProp = dr["sStr1"].ToString();
                                    dr["sStr1"] = p_Actual_Prop.Text;
                                }
                                break;
                            }

                        case "BKUP_ActualProp":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.B_ActualProp = dr["sStr1"].ToString();
                                    dr["sStr1"] = b_Actual_Prop.Text;
                                }
                                break;
                            }

                        case "BKUP_Certi":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.B_Certi = dr["sStr1"].ToString();
                                    dr["sStr1"] = b_Certi.Text;
                                }
                                break;
                            }

                        case "BKUP_Mask":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.B_MaskLab = dr["sStr1"].ToString();
                                    dr["sStr1"] = b_Masklab.Text;
                                }
                                break;
                            }

                        case "Access key":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.Accesskey = dr["sStr1"].ToString();
                                    dr["sStr1"] = Accesskry.Text;
                                }
                                break;
                            }

                        case "Security key":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.SecurityKey = dr["sStr1"].ToString();
                                    dr["sStr1"] = Securitykey.Text;
                                }
                                break;
                            }

                        case "Bucket Region":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.Primaryregion = dr["sStr1"].ToString();
                                    dr["sStr1"] = p_PrimaryRegion.Text;
                                }
                                break;
                            }
                        case "Destination_Region":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.Backupregion = dr["sStr1"].ToString();
                                    dr["sStr1"] = b_BackupRegion.Text;
                                }
                                break;
                            }
                        case "PRIMARY_html_PTH":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.P_Html = dr["sStr1"].ToString();
                                    dr["sStr1"] = p_html.Text;
                                }
                                break;
                            }
                        case "PRIMARY_Mp4_PTH":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.P_Mp4 = dr["sStr1"].ToString();
                                    dr["sStr1"] = p_Mp4.Text;
                                }
                                break;
                            }
                        case "bckup_html_PTH":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.B_Html = dr["sStr1"].ToString();
                                    dr["sStr1"] = bhtml.Text;
                                }
                                break;
                            }
                        case "bckup_mp4_PTH":
                            {
                                if (Convert.ToBoolean(dr["bIsActive"]))
                                {
                                    Global.B_Mp4 = dr["sStr1"].ToString();
                                    dr["sStr1"] = bMp4.Text;
                                }
                                break;
                            }
                    }
                }

                dsMstr.Tables[0].AcceptChanges();
                DataTable dtRuleMas = new DataTable();
                dtRuleMas.Columns.Add("iRuleMasterId", typeof(int));
                dtRuleMas.Columns.Add("iRuleMasterCode", typeof(int));
                dtRuleMas.Columns.Add("vRuleTyp", typeof(string));
                dtRuleMas.Columns.Add("vRuleDsc", typeof(string));
                dtRuleMas.Columns.Add("tFromDate", typeof(DateTime));
                dtRuleMas.Columns.Add("tToDate", typeof(DateTime));
                dtRuleMas.Columns.Add("bIsActive", typeof(string));
                dtRuleMas.Columns.Add("iInt1", typeof(int));
                dtRuleMas.Columns.Add("iInt2", typeof(int));
                dtRuleMas.Columns.Add("iInt3", typeof(int));
                dtRuleMas.Columns.Add("sStr1", typeof(string));
                dtRuleMas.Columns.Add("sStr2", typeof(string));
                dtRuleMas.Columns.Add("sStr3", typeof(string));
                dtRuleMas.Columns.Add("tDate1", typeof(DateTime));
                dtRuleMas.Columns.Add("tDate2", typeof(DateTime));
                dtRuleMas.Columns.Add("tDate3", typeof(DateTime));
                // Populate the DataTable with data
                foreach (DataRow _dr in dsMstr.Tables[0].Rows)
                {
                    DataRow row = dtRuleMas.NewRow();
                    row["iRuleMasterId"] = _dr["iRuleMasterId"];
                    row["iRuleMasterCode"] = _dr["iRuleMasterCode"];
                    row["vRuleTyp"] = _dr["vRuleTyp"];
                    row["vRuleDsc"] = _dr["vRuleDsc"];
                    row["tFromDate"] = _dr["tFromDate"];
                    row["tToDate"] = _dr["tToDate"];
                    row["bIsActive"] = _dr["bIsActive"];
                    row["iInt1"] = _dr["iInt1"];
                    row["iInt2"] = _dr["iInt2"];
                    row["iInt3"] = _dr["iInt3"];
                    row["sStr1"] = _dr["sStr1"];
                    row["sStr2"] = _dr["sStr2"];
                    row["sStr3"] = _dr["sStr3"];
                    row["tDate1"] = _dr["tDate1"];
                    row["tDate2"] = _dr["tDate2"];
                    row["tDate3"] = _dr["tDate3"];
                    dtRuleMas.Rows.Add(row);
                }
                Hashtable _ht = new Rule_Mstr_DAL().updRuleMstr(dtRuleMas);
                if (_ht != null)
                {
                    MessageBox.Show("Records updated successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        public void clearData()
        {
            //primary_bucket 
            name.Text = string.Empty;
            p_Img.Text = string.Empty;
            p_Certi.Text = string.Empty;
            p_Video.Text = string.Empty;
            p_Masklab.Text = string.Empty;
            p_PrimaryRegion.Text = string.Empty;
            Accesskry.Text = string.Empty;
            p_Actual_Prop.Text = string.Empty;

            //backup_bucket
            P_Name.Text = string.Empty;
            b_Img.Text = string.Empty;
            b_Video.Text = string.Empty;
            b_Masklab.Text = string.Empty;
            b_Certi.Text = string.Empty;
            b_Actual_Prop.Text = string.Empty;
            Securitykey.Text = string.Empty;
            b_BackupRegion.Text= string.Empty;
        }

        public void Page_lOAD()
        {
            new ComUtils().loadGlobalData();
            P_Name.Text = Global.bucketName;
            name.Text = Global.BackupBucket;
            p_Img.Text = Global.P_Imagepath;
            b_Img.Text = Global.B_Imagepath;
            p_Video.Text = Global.P_Video;
            b_Video.Text = Global.B_Video;
            p_Certi.Text = Global.P_Certi;
            p_Masklab.Text = Global.P_MaskLab;
            p_Actual_Prop.Text = Global.P_ActualProp;
            b_Actual_Prop.Text = Global.B_ActualProp;
            b_Certi.Text = Global.B_Certi;
            b_Masklab.Text = Global.B_MaskLab;
            Accesskry.Text = Global.Accesskey;
            Securitykey.Text = Global.SecurityKey;
            p_PrimaryRegion.Text = Global.Primaryregion;
            b_BackupRegion.Text = Global.Backupregion;
            p_html.Text = Global.P_Html;
            p_Mp4.Text = Global.P_Mp4;
            bhtml.Text = Global.B_Html;
            bMp4.Text = Global.B_Mp4;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Page_lOAD();
        }
    }
}
