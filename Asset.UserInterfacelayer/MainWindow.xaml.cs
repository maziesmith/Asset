using Arthas.Controls.Metro;
using Arthas.Utility.Media;
using Asset.BusinessLogicLayer;
using Asset.DataAccessHelper;
using Asset.CommonComponent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace Asset
{
    public partial class MainWindow : MetroWindow
    {
        //实例化一个ini配置文件
        IniFiles ini = new IniFiles(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\config.ini");

        public MainWindow()
        {
            InitializeComponent();

            //读取并绑定初始主题颜色
            RgbaColor bColor = new RgbaColor(ConfigurationManager.AppSettings["ThemeColor"]);
            BorderBrush = bColor.OpaqueSolidColorBrush;
            //改变主题颜色
            color1.ColorChange += delegate
            {
                // 不要通过XAML来绑定颜色，无法获取到通知
                BorderBrush = color1.CurrentColor.OpaqueSolidColorBrush;
            };

            exit.Click += delegate { Close(); };

            ContentRendered += delegate
            {
                // 手动加载指定HTML
                web1.Document = ResObj.GetString(Assembly.GetExecutingAssembly(), "Resources.about.html");

                // 导航到指定网页
                web2.Source = new Uri("http://ie.icoa.cn/");
            };

            //左边工具栏按钮单击展开关闭事件绑定
            foreach (FrameworkElement fe in lists.Children)
            {
                if (fe is MetroExpander)
                {
                    (fe as MetroExpander).Click += delegate (object sender, EventArgs e)
                    {
                        if ((fe as MetroExpander).CanHide)
                        {
                            foreach (FrameworkElement fe1 in lists.Children)
                            {
                                if (fe1 is MetroExpander && fe1 != sender)
                                {
                                    (fe1 as MetroExpander).IsExpanded = false;
                                }
                            }
                        }
                    };
                }
            }

        }

        //关闭主窗口
        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //保存主题颜色
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            cfa.AppSettings.Settings["ThemeColor"].Value = BorderBrush.ToString();
            cfa.Save();
        }

        //点击菜单栏登录按钮
        private void MenuLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow win = new LoginWindow();
            win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //设置登录窗口主题
            if(BorderBrush!=null)
            {
                win.BorderBrush = BorderBrush;
            }
            if (win.ShowDialog().Value == true)
            {
                //登录成功

                DataView dvlist = FixedAsset.Query_V_FixedAssets();
                //绑定资产数据
                dtgShow.ItemsSource = dvlist;
                //绑定下拉框数据－事业部和部门
                BindDrop();
                //导出excel按钮置灰
                btExport.IsEnabled = false;
            }
        }

        #region 资产管理页面
        /// <summary>
        /// 绑定事业部下拉列表
        /// </summary>
        private void BindDrop()
        {
            //将数据捆绑到下拉列表中
            DataView dv = Division.QueryDivision();
            cbxDivisionID.SelectedValuePath = dv.Table.Columns[0].Caption;
            cbxDivisionID.DisplayMemberPath = dv.Table.Columns[2].Caption;
            cbxDivisionID.ItemsSource = dv;
            cbxDivisionID.Text="请选择事业部";
            cbxDepartmentID.Text = "请选择部门";
        }

        //选择事业部下拉列表项时，绑定部门下拉列表
        private void CbxDivisionID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int showDivisionID = -1;
            //获取事业部ID
            if (cbxDivisionID.SelectedItem!=null&&cbxDivisionID.SelectedItem.ToString() != "")
                showDivisionID = Convert.ToInt32(cbxDivisionID.SelectedValue.ToString());
            //绑定部门数据到下拉列表
            DataView dv1 = Department.QueryDepartment(showDivisionID);
            cbxDepartmentID.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxDepartmentID.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxDepartmentID.ItemsSource = dv1;
            cbxDepartmentID.Text = "请选择部门";
        }

        //开始查询单击
        private void BtQuery_Click(object sender, RoutedEventArgs e)
        {
            string strSql = "";
            int divisionID, departmentID, useSituationID;
            //关键字
            string keywords = Convert.ToString(txtKeyWords.Text);

            //获取使用情况
            string showAllUseSituationID = string.Empty;
            foreach (UIElement element in panelUseSituation.Children)
            {
                if (element is CheckBox)
                {
                    if ((element as CheckBox).IsChecked.Value)
                        showAllUseSituationID += (element as CheckBox).Tag + ",";
                }
            }
            showAllUseSituationID = showAllUseSituationID.TrimEnd(',');
            //绑定数据
            DataView dvlist = new DataView();
            if (showAllUseSituationID == "")
            {
                if (cbxDivisionID.SelectedValue == null & cbxDepartmentID.SelectedValue == null)
                {
                    strSql = "select * from Web_V_FixedAssets where AssetsCoding like \'%" + keywords + "%\' or AssetName like \'%" + keywords + "%\' order by FixedAssetsID desc";
                }
                else if (cbxDivisionID.SelectedValue != null & cbxDepartmentID.SelectedValue == null)
                {
                    divisionID = Convert.ToInt32(cbxDivisionID.SelectedValue.ToString());
                    strSql = "select * from Web_V_FixedAssets where DivisionID=" + divisionID + " and (AssetsCoding like \'%" + keywords + "%\' or AssetName like \'%" + keywords + "%\') order by FixedAssetsID desc";
                }
                else if (cbxDivisionID.SelectedValue != null & cbxDepartmentID.SelectedValue != null)
                {
                    divisionID = Convert.ToInt32(cbxDivisionID.SelectedValue.ToString());
                    departmentID = Convert.ToInt32(cbxDepartmentID.SelectedValue.ToString());
                    strSql = "select * from Web_V_FixedAssets where DivisionID=" + divisionID + " and DepartmentID=" + departmentID + " and (AssetsCoding like \'%" + keywords + "%\' or AssetName like \'%" + keywords + "%\') order by FixedAssetsID desc";
                }
            }
            else
            {
                if (cbxDivisionID.SelectedValue == null & cbxDepartmentID.SelectedValue == null)
                {
                    strSql = "select * from Web_V_FixedAssets where UseSituationID in(" + showAllUseSituationID + ") and (AssetsCoding like \'%" + keywords + "%\' or AssetName like \'%" + keywords + "%\') order by FixedAssetsID desc";
                }
                else if (cbxDivisionID.SelectedValue != null & cbxDepartmentID.SelectedValue == null)
                {
                    divisionID = Convert.ToInt32(cbxDivisionID.SelectedValue.ToString());
                    strSql = "select * from Web_V_FixedAssets where UseSituationID in(" + showAllUseSituationID + ") and DivisionID=" + divisionID + " and (AssetsCoding like \'%" + keywords + "%\' or AssetName like \'%" + keywords + "%\') order by FixedAssetsID desc";
                }
                else if (cbxDivisionID.SelectedValue != null & cbxDepartmentID.SelectedValue != null)
                {
                    divisionID = Convert.ToInt32(cbxDivisionID.SelectedValue.ToString());
                    departmentID = Convert.ToInt32(cbxDepartmentID.SelectedValue.ToString());
                    strSql = "select * from Web_V_FixedAssets where UseSituationID in(" + showAllUseSituationID + ") and DivisionID=" + divisionID + " and DepartmentID=" + departmentID + " and (AssetsCoding like \'%" + keywords + "%\' or AssetName like \'%" + keywords + "%\') order by FixedAssetsID desc";
                }
            }
            dvlist = FixedAsset.QueryFixedAssets1(strSql);
            dtgShow.ItemsSource = dvlist;
            //导出excel按钮启用
            btExport.IsEnabled = true;
        }

        //导出为Excel单击
        private void BtExport_Click(object sender, RoutedEventArgs e)
        {
            string strSql = "";
            int divisionID, departmentID, useSituationID;
            //关键字
            string keywords = Convert.ToString(txtKeyWords.Text);

            //获取使用情况
            string showAllUseSituationID = string.Empty;
            foreach (UIElement element in panelUseSituation.Children)
            {
                if (element is CheckBox)
                {
                    if ((element as CheckBox).IsChecked.Value)
                        showAllUseSituationID += (element as CheckBox).Tag + ",";
                }
            }
            showAllUseSituationID = showAllUseSituationID.TrimEnd(',');

            //绑定数据
            DataView dvlist = new DataView();
            if (showAllUseSituationID == "")
            {
                if (cbxDivisionID.SelectedValue == null & cbxDepartmentID.SelectedValue == null)
                {
                    strSql = "select * from Web_V_FixedAssets where AssetsCoding like \'%" + keywords + "%\' or AssetName like \'%" + keywords + "%\' order by FixedAssetsID desc";
                }
                else if (cbxDivisionID.SelectedValue != null & cbxDepartmentID.SelectedValue == null)
                {
                    divisionID = Convert.ToInt32(cbxDivisionID.SelectedValue.ToString());
                    strSql = "select * from Web_V_FixedAssets where DivisionID=" + divisionID + " and (AssetsCoding like \'%" + keywords + "%\' or AssetName like \'%" + keywords + "%\') order by FixedAssetsID desc";
                }
                else if (cbxDivisionID.SelectedValue != null & cbxDepartmentID.SelectedValue != null)
                {
                    divisionID = Convert.ToInt32(cbxDivisionID.SelectedValue.ToString());
                    departmentID = Convert.ToInt32(cbxDepartmentID.SelectedValue.ToString());
                    strSql = "select * from Web_V_FixedAssets where DivisionID=" + divisionID + " and DepartmentID=" + departmentID + " and (AssetsCoding like \'%" + keywords + "%\' or AssetName like \'%" + keywords + "%\') order by FixedAssetsID desc";
                }
            }
            else
            {
                if (cbxDivisionID.SelectedValue == null & cbxDepartmentID.SelectedValue == null)
                {
                    strSql = "select * from Web_V_FixedAssets where UseSituationID in(" + showAllUseSituationID + ") and (AssetsCoding like \'%" + keywords + "%\' or AssetName like \'%" + keywords + "%\') order by FixedAssetsID desc";
                }
                else if (cbxDivisionID.SelectedValue != null & cbxDepartmentID.SelectedValue == null)
                {
                    divisionID = Convert.ToInt32(cbxDivisionID.SelectedValue.ToString());
                    strSql = "select * from Web_V_FixedAssets where UseSituationID in(" + showAllUseSituationID + ") and DivisionID=" + divisionID + " and (AssetsCoding like \'%" + keywords + "%\' or AssetName like \'%" + keywords + "%\') order by FixedAssetsID desc";
                }
                else if (cbxDivisionID.SelectedValue != null & cbxDepartmentID.SelectedValue != null)
                {
                    divisionID = Convert.ToInt32(cbxDivisionID.SelectedValue.ToString());
                    departmentID = Convert.ToInt32(cbxDepartmentID.SelectedValue.ToString());
                    strSql = "select * from Web_V_FixedAssets where UseSituationID in(" + showAllUseSituationID + ") and DivisionID=" + divisionID + " and DepartmentID=" + departmentID + " and (AssetsCoding like \'%" + keywords + "%\' or AssetName like \'%" + keywords + "%\') order by FixedAssetsID desc";
                }
            }

            dvlist = FixedAsset.QueryFixedAssets1(strSql);

            string showFileName = Guid.NewGuid().ToString() + ".xls";
            string showNewFileName = "\\xls\\" + showFileName;
            string sNewFullFile = Environment.CurrentDirectory + showNewFileName;
            try
            {
                File.Copy(Environment.CurrentDirectory + "\\xls\\template.xls", sNewFullFile);
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message);
                return;
            }



            string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=True;Data Source=" + sNewFullFile + ";Extended Properties=Excel 8.0;";
            System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(strConn);
            OleDbCommand cmd = null;

            bool bRet = false;
            try
            {
                conn.Open();

                //cmd = new OleDbCommand("create table [sheet4]([姓名] Text,[年龄] Text,[电话] int)", conn);
                //cmd.ExecuteNonQuery();

                string strSQL = "INSERT INTO [Sheet1$] ([资产编码],[资产名称],[资产大类ID],[资产大类],[资产小类ID],[资产小类],[购置日期],[建立日期],[型号规格],[生产厂家],[品牌],[保管员工号],[保管员姓名],[存放地点],[事业部ID],[事业部名称],[部门ID],[部门名称],[使用情况ID],[使用情况],[申请状态ID],[增加方式ID],[增加方式],[计量单位ID],[计量单位],[原值],[有限年份],[残值率],[剩余月份],[残值],[月折旧],[累计折旧],[净值],[备注],[低值易耗品]) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

                cmd = new OleDbCommand(strSQL, conn);

                for (int i = 0; i < 35; i++)
                {
                    cmd.Parameters.Add(i.ToString(), OleDbType.VarChar);
                }

                DataView dv = dvlist;
                foreach (DataRowView row in dv)
                {
                    cmd.Parameters[0].Value = row["AssetsCoding"].ToString();
                    cmd.Parameters[1].Value = row["AssetName"].ToString();
                    cmd.Parameters[2].Value = (int)row["MajorID"];
                    cmd.Parameters[3].Value = row["MajorName"].ToString();
                    cmd.Parameters[4].Value = (int)row["SubID"];
                    cmd.Parameters[5].Value = row["SubName"].ToString();

                    cmd.Parameters[6].Value = row["PurchaseDate"].ToString();
                    cmd.Parameters[7].Value = row["RecordedDate"].ToString();
                    cmd.Parameters[8].Value = row["SpecificationsModel"].ToString();
                    cmd.Parameters[9].Value = row["Manufacturer"].ToString();
                    cmd.Parameters[10].Value = row["Brand"].ToString();
                    cmd.Parameters[11].Value = row["UserAccount"].ToString();

                    cmd.Parameters[12].Value = row["Contactor"].ToString();
                    cmd.Parameters[13].Value = row["StorageSites"].ToString();
                    cmd.Parameters[14].Value = (int)row["DivisionID"];
                    cmd.Parameters[15].Value = row["DivisionName"].ToString();
                    cmd.Parameters[16].Value = (int)row["DepartmentID"];
                    cmd.Parameters[17].Value = row["DepartmentName"].ToString();

                    cmd.Parameters[18].Value = (int)row["UseSituationID"];
                    cmd.Parameters[19].Value = row["ShowUseSituationID"].ToString();
                    cmd.Parameters[20].Value = (int)row["ApplyStatus"];
                    cmd.Parameters[21].Value = (int)row["AddWaysID"];
                    cmd.Parameters[22].Value = row["AddWaysName"].ToString();
                    cmd.Parameters[23].Value = (int)row["UnitsID"];

                    cmd.Parameters[24].Value = row["UnitsName"].ToString();
                    cmd.Parameters[25].Value = row["OriginalValue"].ToString();
                    cmd.Parameters[26].Value = row["LimitedYear"].ToString();
                    cmd.Parameters[27].Value = row["ResidualValueRate"].ToString();
                    cmd.Parameters[28].Value = row["ShowRemainderMonth"].ToString();
                    cmd.Parameters[29].Value = row["ShowResiduals"].ToString();

                    cmd.Parameters[30].Value = row["ShowMonthDepreciation"].ToString();
                    cmd.Parameters[31].Value = row["ShowAccumulatedDepreciation"].ToString();
                    cmd.Parameters[32].Value = row["ShowNetValue"].ToString();
                    cmd.Parameters[33].Value = row["AssetsBackup"].ToString();
                    cmd.Parameters[34].Value = (int)row["LowConsumables"];

                    cmd.ExecuteNonQuery();
                }
                bRet = true;

            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message);
            }
            finally
            {
                if (cmd != null)
                {
                    cmd.Dispose();
                }
                conn.Dispose();

            }
            if (bRet)
                //Response.Redirect(FileName);
                System.Diagnostics.Process.Start(sNewFullFile);
        }

        //全选
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox headercb = (CheckBox)sender;
            for (int i = 0; i < dtgShow.Items.Count; i++)
            {
                //获取行
                DataGridRow neddrow = (DataGridRow)dtgShow.ItemContainerGenerator.ContainerFromIndex(i);
                //获取该行的某列
                CheckBox cb = (CheckBox)dtgShow.Columns[0].GetCellContent(neddrow);
                cb.IsChecked = headercb.IsChecked;
            }
            GetSelected();
        }

        /// <summary>
        /// 得到用户的选择
        /// </summary>
        /// <returns>用户选择固定资产的编号集合</returns>
        private ArrayList GetSelected()
        {
            ArrayList selectedItems = new ArrayList();
            for (int i = 0; i < dtgShow.Items.Count; i++)
            {
                //获取行
                DataGridRow neddrow = (DataGridRow)dtgShow.ItemContainerGenerator.ContainerFromIndex(i);
                //获取该行的某列
                CheckBox cb = (CheckBox)dtgShow.Columns[0].GetCellContent(neddrow);
                if (cb.IsChecked!=null||(bool)cb.IsChecked)
                {
                    if(string.IsNullOrEmpty((dtgShow.Columns[1].GetCellContent(neddrow) as TextBlock).Text))
                    {
                        selectedItems.Add(Convert.ToInt32((dtgShow.Columns[1].GetCellContent(neddrow) as TextBlock).Text));
                    }
                }
            }
            return selectedItems;
        }

        //加入打印列表
        private void BtAddPrint_Click(object sender, RoutedEventArgs e)
        {
            string userAccount = string.Empty;
            if (ini.ExistINIFile())
            {
                //如果存在配置文件就进行读取
                userAccount = ini.IniReadValue("登录详细", "UserAccount");
            }
            int showAddCount = 0;

            PrintList printList = new PrintList();
            Hashtable ht = new Hashtable();
            ArrayList selectedFixedAssets = this.GetSelected();

            //如果用户没有选择,就单击该按钮,则给出警告
            if (selectedFixedAssets.Count == 0)
            {
                MessageBox.Show("请选择固定资产!");
                return;
            }
            else
            {
                //循环将固定资产信息加入打印列表中
                foreach (int fixedAssetsID in selectedFixedAssets)
                {
                    PrintList printlist1 = new PrintList();
                    printlist1.LoadData(fixedAssetsID, userAccount);
                    if (printlist1.Exist)
                    {
                        //break;
                    }
                    else
                    {
                        ht.Clear();
                        ht.Add("PFixedAssetsID", fixedAssetsID);
                        ht.Add("PUserAccount", SqlStringConstructor.GetQuotedString(userAccount));
                        printList.Add(ht);

                        showAddCount = showAddCount + 1;
                    }
                }
                MessageBox.Show("加入打印列表成功");
            }
        }

        //查看打印列表
        private void BtViewPrint_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion


        private void BtnEnter_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    /// <summary>
    /// 使用情况转换器,在xaml中通过Converter使用
    /// </summary>
    class GetUseSituation : IValueConverter
    {
        //Convert方法用来将数据转换成我们想要的显示的格式  
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int iValue = (int)value;
            switch (iValue.ToString())
            {
                case "1":
                    return "使用中";
                case "2":
                    return "报废";
                case "3":
                    return "闲置";
                case "4":
                    return "维修中";
                default:
                    return "未知";
            }
        }

        //ConvertBack方法将显示值转换成原来的格式,因为我不需要反向转换,所以直接抛出个异常  
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 申请状态转换器,在xaml中通过Converter使用
    /// </summary>
    class GetApplyStatus : IValueConverter
    {
        //Convert方法用来将数据转换成我们想要的显示的格式  
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int iValue = (int)value;
            switch (iValue.ToString())
            {
                case "0":
                    return "--";
                case "1":
                    return "异动申请";
                case "2":
                    return "维修中";
                case "3":
                    return "报废申请";
                case "4":
                    return "新增";
                default:
                    return "未知";
            }
        }

            //ConvertBack方法将显示值转换成原来的格式,因为我不需要反向转换,所以直接抛出个异常  
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}