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
                //web2.Source = new Uri("http://ie.icoa.cn/");
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

        /// <summary>
        /// 数据初始化
        /// </summary>
        private void InitialData()
        {
            //1.验证是否登录

            //2.数据初始化
            //绑定资产数据
            DataView dvlist = FixedAsset.Query_V_FixedAssets();
            dtgShow.ItemsSource = dvlist;

            //打印列表
            string userAccount = ini.IniReadValue("登录详细", "UserAccount");
            DataView dvlist2 = PrintList.Query_V_PrintList(userAccount);
            dtgPrintList.ItemsSource = dvlist2;

            //审核新增
            DataView dvlist3 = FixedAsset.QueryFixedAssets(4);
            dtgCheckAddAssets.ItemsSource = dvlist3;

            //审核异动
            DataView dvlist4 = FixedAsset.QueryFixedAssets(1);
            dtgCheckChangeAssets.ItemsSource = dvlist4;

            //审核维修
            DataView dvlist5 = FixedAsset.QueryFixedAssets(2);
            dtgCheckRepairAssets.ItemsSource = dvlist5;

            //绑定下拉框数据
            BindDrop();//资产管理页事业部和部门
            BindDrop2();//新增页下拉框

            //3.界面元素初始化
            //导出excel按钮置灰
            btExport.IsEnabled = false;
        }

        #region 菜单栏
        //点击用户菜单登录按钮
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
                InitialData();
                //构建用户菜单
                if (ini.ExistINIFile())
                {
                    //如果存在配置文件就进行读取
                    string userAccount = ini.IniReadValue("登录详细", "UserAccount");
                    UserList userList = new UserList();
                    userList.LoadData(userAccount);
                    mainWindow.Title = "资产管理系统--" + "管理员：" + userAccount;
                    //1.先隐藏用户菜单
                    foreach (FrameworkElement fe in lists.Children)
                    {
                        
                        if (fe is MetroExpander)
                        {
                            foreach (var fe1 in (fe as MetroExpander).Children)
                            {
                                if (fe1 is MetroExpander)
                                {
                                    (fe1 as MetroExpander).Visibility = Visibility.Collapsed;
                                }
                            }
                        }
                    }
                    //2.根据权限显示
                    foreach (string duty in userList.Duties)
                    {
                        if(this.FindName("treeMenu" + duty)!=null)
                        {
                            (this.FindName("treeMenu" + duty) as MetroExpander).Visibility = Visibility.Visible;
                        }
                    }
                }
            }
        }

        //退出登录
        private void MenuExitLogin_Click(object sender, RoutedEventArgs e)
        {
            dtgShow.ItemsSource = null;
        }

        #endregion

        #region 树菜单

        //资产查询
        private void TreeMenuManageFixedAssets_Click(object sender, EventArgs e)
        {
            tabpManageFixedAssets.IsSelected = true;
        }

        //资产新增
        private void TreeMenuAddFixedAssets_Click(object sender, EventArgs e)
        {
            tabpAddAsset.IsSelected = true;
        }
        #endregion

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
                MessageBox.Show(er.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show(er.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
            //GetSelected();
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
                MessageBox.Show("请选择固定资产！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("加入打印列表成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            //打印列表数据
            DataView dvlist2 = PrintList.Query_V_PrintList(userAccount);
            dtgPrintList.ItemsSource = dvlist2;
        }

        //查看打印列表
        private void BtViewPrint_Click(object sender, RoutedEventArgs e)
        {
            tabiPrint.IsSelected = true;
        }

        //修改
        private void BtModify_Click(object sender, RoutedEventArgs e)
        {
            FixedAssetsID = Convert.ToInt32(((System.Data.DataRowView)((System.Windows.FrameworkElement)sender).DataContext)["FixedAssetsID"]);
            InitModifyData();
            tabpModifyAsset.IsSelected = true;
        }

        //异动
        private void BtChange_Click(object sender, RoutedEventArgs e)
        {
            FixedAssetsID = Convert.ToInt32(((System.Data.DataRowView)((System.Windows.FrameworkElement)sender).DataContext)["FixedAssetsID"]);
            InitChangeData();
            tabpChangeAsset.IsSelected = true;
        }

        //维修
        private void BtRepair_Click(object sender, RoutedEventArgs e)
        {
            FixedAssetsID = Convert.ToInt32(((System.Data.DataRowView)((System.Windows.FrameworkElement)sender).DataContext)["FixedAssetsID"]);
            InitRepairData();
            tabpRepairAsset.IsSelected = true;
        }

        //报废
        private void BtScrapped_Click(object sender, RoutedEventArgs e)
        {
            FixedAssetsID = Convert.ToInt32(((System.Data.DataRowView)((System.Windows.FrameworkElement)sender).DataContext)["FixedAssetsID"]);
            InitScrappedData();
            tabpScrappedAsset.IsSelected = true;
        }
        #endregion

        #region 新增资产页面
        //绑定新增资产页面下拉列表数据
        private void BindDrop2()
        {
            //将数据捆绑到下拉列表中
            DataView dv = MajorClass.QueryMajorClass();
            cbxMajorID.SelectedValuePath = dv.Table.Columns[0].Caption;
            cbxMajorID.DisplayMemberPath = dv.Table.Columns[2].Caption;
            cbxMajorID.ItemsSource = dv;
            cbxMajorID.Text = "资产一级类别";        //第一项中加入内容,重点是绑定后添加
            cbxSubID.Text = "资产二级类别";

            //绑定单位
            DataView dv1 = UnitList.QueryUnits();
            cbxUnitsID.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxUnitsID.DisplayMemberPath = dv1.Table.Columns[2].Caption;
            cbxUnitsID.ItemsSource = dv1;

            //绑定事业部
            DataView dv2 = Division.QueryDivision();
            cbxDivisionID0.SelectedValuePath = dv2.Table.Columns[0].Caption;
            cbxDivisionID0.DisplayMemberPath = dv2.Table.Columns[2].Caption;
            cbxDivisionID0.ItemsSource = dv2;
            cbxDivisionID0.Text = "请选择事业部";
            cbxDepartmentID0.Text = "请选择部门";

            //绑定增加方式
            DataView dv3 = AddWays.QueryAddWays();
            cbxAddWaysID.SelectedValuePath = dv3.Table.Columns[0].Caption;
            cbxAddWaysID.DisplayMemberPath = dv3.Table.Columns[2].Caption;
            cbxAddWaysID.ItemsSource = dv3;

            //折旧方法
            DataView dv4 = DepreciationMethod.QueryDepreciationMethod();
            cbxMethodID.SelectedValuePath = dv4.Table.Columns[0].Caption;
            cbxMethodID.DisplayMemberPath = dv4.Table.Columns[2].Caption;
            cbxMethodID.ItemsSource = dv4;

            //保管人员
            cbxUserAccount.Text = "无";

            //使用人员
            cbxUseUserAccount.Text = "无";
        }     

        //选择资产一级类别
        private void CbxMajorID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int majorID = -1;
            //获取资产一级ID
            if (cbxMajorID.SelectedItem != null && cbxMajorID.SelectedValue != null)
                majorID = Convert.ToInt32(cbxMajorID.SelectedValue.ToString());
            //绑定资产二级类别数据到下拉列表
            DataView dv1 = SubClass.QuerySubClass(majorID);
            cbxSubID.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxSubID.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxSubID.ItemsSource = dv1;
            cbxSubID.Text = "资产二级类别";
        }

        //选择资产二级类别
        private void CbxSubID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int subID = -1;
            //获取资产二级ID
            if (cbxSubID.SelectedItem != null && cbxSubID.SelectedValue != null)
                subID = Convert.ToInt32(cbxSubID.SelectedValue.ToString());
            SubClass subClass = new SubClass();
            //二级资产初始化
            subClass.LoadData(subID);
            if (subClass.Exist) //是否存在标志
            {
                cbxUnitsID.SelectedValue = subClass.UnitsID.ToString();  //计量单位
                txtLimitedYear.Text = subClass.UsefulLife.ToString();  //有限年份
                txtResidualValueRate.Text = subClass.DepreciationRate.ToString();  //残值率
            }

            //计算资产编码
            if (cbxMajorID.SelectedValue == null || cbxSubID.SelectedValue == null)
            {
                return;
            }
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadData1(subID);
            if (fixedAsset.Exist)
            {
                int ShowFixedAssetsID = fixedAsset.FixedAssetsID + 1;
                
                txtAssetsCoding.Text = cbxMajorID.SelectedValue.ToString() + cbxSubID.SelectedValue.ToString() + ShowFixedAssetsID.ToString();
            }
            else
            {
                txtAssetsCoding.Text = cbxMajorID.SelectedValue.ToString() + cbxSubID.SelectedValue.ToString() + "10000";
            }
        }

        //选择事业部
        private void cbxDivisionID0_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int divisionID = -1;
            //获取事业部ID
            if (cbxDivisionID0.SelectedItem != null && cbxDivisionID0.SelectedValue != null)
                divisionID = Convert.ToInt32(cbxDivisionID0.SelectedValue.ToString());
            //绑定部门数据到下拉列表
            DataView dv1 = Department.QueryDepartment(divisionID);
            cbxDepartmentID0.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxDepartmentID0.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxDepartmentID0.ItemsSource = dv1;
            cbxDepartmentID0.Text = "请选择部门";
        }

        //选择部门
        private void cbxDepartmentID0_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int departmentID = -1;
            if (cbxDepartmentID0.SelectedItem != null && cbxDepartmentID0.SelectedValue != null)
                departmentID = Convert.ToInt32(cbxDepartmentID0.SelectedValue.ToString());
            //加载保管人员
            DataView dv1 = UserList.QueryUserLists(departmentID);
            cbxUserAccount.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxUserAccount.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxUserAccount.ItemsSource = dv1;
            cbxUserAccount.Text="无";
            //加载使用人员
            cbxUseUserAccount.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxUseUserAccount.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxUseUserAccount.ItemsSource = dv1;
            cbxUseUserAccount.Text = "无";
        }

        //确认新增
        private void BtnEnter_Click(object sender, RoutedEventArgs e)
        {
            //1.验证用户输入
            mtxtType.Visibility = Visibility.Collapsed;
            mtxtName.Visibility = Visibility.Collapsed;
            mtxtDepartment.Visibility = Visibility.Collapsed;
            mtxtLimitedYear.Visibility = Visibility.Collapsed;
            mtxtNum.Visibility = Visibility.Collapsed;
            mtxtOriginalValue.Visibility = Visibility.Collapsed;
            mtxtSpecificationsModel.Visibility = Visibility.Collapsed;
            //验证必要项
            if (cbxMajorID.SelectedItem==null || cbxSubID.SelectedItem==null)    //类别
            {
                mtxtType.Visibility = Visibility.Visible;
                return;
            }
            if(string.IsNullOrEmpty(txtAssetName.Text)) //名称
            {
                mtxtName.Visibility = Visibility.Visible;
                return;
            }
            if (cbxDivisionID0.SelectedItem==null || cbxDepartmentID0.SelectedItem==null)   //部门
            {
                mtxtDepartment.Visibility = Visibility.Visible;
                return;
            }
            if (!int.TryParse(txtLimitedYear.Text, out int year))  //有限年份
            {
                mtxtLimitedYear.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(txtAssetsCoding.Text) || !int.TryParse(txtAssetNum.Text, out int num))  //编码和数量
            {
                mtxtNum.Visibility = Visibility.Visible;
                return;
            }
            if (!double.TryParse(txtOriginalValue.Text, out double ori))  //原值
            {
                mtxtOriginalValue.Visibility = Visibility.Visible;
                return;
            }
            if(string.IsNullOrEmpty(txtSpecificationsModel.Text))
            {
                mtxtSpecificationsModel.Visibility = Visibility.Visible;
                return;
            }

            //2.获取用户在页面上的输入
            string assetsCoding = txtAssetsCoding.Text;                        //资产编码
            int showAssetsCoding = Convert.ToInt32(txtAssetsCoding.Text);      //资产编码
            int showAssetNum = Convert.ToInt32(txtAssetNum.Text);              //资产数目

            for (int i = 0; i < showAssetNum; i++)
            {
                showAssetsCoding = showAssetsCoding + 1;
                FixedAsset fixedAsset = new FixedAsset();
                fixedAsset.LoadData(assetsCoding);

                if (fixedAsset.Exist)
                {
                    MessageBox.Show("您输入的固定资产编码已经存在！","失败",MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }
                else
                {
                    Hashtable ht = new Hashtable();

                    ht.Add("AssetsCoding", showAssetsCoding);
                    ht.Add("AssetName", SqlStringConstructor.GetQuotedString(txtAssetName.Text));
                    ht.Add("MajorID", SqlStringConstructor.GetQuotedString(cbxMajorID.SelectedValue.ToString()));
                    ht.Add("MajorName", SqlStringConstructor.GetQuotedString(cbxMajorID.Text));
                    ht.Add("SubID", SqlStringConstructor.GetQuotedString(cbxSubID.SelectedValue.ToString()));
                    ht.Add("SubName", SqlStringConstructor.GetQuotedString(cbxSubID.Text));
                    ht.Add("SpecificationsModel", SqlStringConstructor.GetQuotedString(txtSpecificationsModel.Text));
                    ht.Add("Brand", SqlStringConstructor.GetQuotedString(txtBrand.Text));
                    ht.Add("Manufacturer", SqlStringConstructor.GetQuotedString(txtManufacturer.Text));
                    ht.Add("UnitsID", SqlStringConstructor.GetQuotedString(cbxUnitsID.SelectedValue.ToString()));
                    ht.Add("UnitsName", SqlStringConstructor.GetQuotedString(cbxUnitsID.Text));
                    ht.Add("UseSituationID", SqlStringConstructor.GetQuotedString((cbxUseSituationID.SelectedIndex+1).ToString()));
                    ht.Add("DivisionID", SqlStringConstructor.GetQuotedString(cbxDivisionID0.SelectedValue.ToString()));
                    ht.Add("DivisionName", SqlStringConstructor.GetQuotedString(cbxDivisionID0.Text));
                    ht.Add("DepartmentID", SqlStringConstructor.GetQuotedString(cbxDepartmentID0.SelectedValue.ToString()));
                    ht.Add("DepartmentName", SqlStringConstructor.GetQuotedString(cbxDepartmentID0.Text));
                    ht.Add("UserAccount", SqlStringConstructor.GetQuotedString(cbxUserAccount.SelectedValue.ToString()));
                    ht.Add("Contactor", SqlStringConstructor.GetQuotedString(cbxUserAccount.Text));
                    ht.Add("AddWaysID", SqlStringConstructor.GetQuotedString(cbxAddWaysID.SelectedValue.ToString()));
                    ht.Add("AddWaysName", SqlStringConstructor.GetQuotedString(cbxAddWaysID.Text));
                    ht.Add("OriginalValue", SqlStringConstructor.GetQuotedString(txtOriginalValue.Text));
                    if (dateExFactoryDate.ToString() == "")
                    {
                        ht.Add("ExFactoryDate", "Null");
                    }
                    else
                    {
                        ht.Add("ExFactoryDate", SqlStringConstructor.GetQuotedString(dateExFactoryDate.ToString()));
                    }
                    if (datePurchaseDate.ToString() == "")
                    {
                        ht.Add("PurchaseDate", "Null");
                    }
                    else
                    {
                        ht.Add("PurchaseDate", SqlStringConstructor.GetQuotedString(datePurchaseDate.ToString()));
                    }
                    if (dateRecordedDate.ToString() == "")
                    {
                        ht.Add("RecordedDate", "Null");
                    }
                    else
                    {
                        ht.Add("RecordedDate", SqlStringConstructor.GetQuotedString(dateRecordedDate.ToString()));
                    }

                    ht.Add("MethodID", SqlStringConstructor.GetQuotedString(cbxMethodID.SelectedValue.ToString()));
                    ht.Add("MethodName", SqlStringConstructor.GetQuotedString(cbxMethodID.Text));
                    ht.Add("ResidualValueRate", SqlStringConstructor.GetQuotedString(txtResidualValueRate.Text));
                    ht.Add("LimitedYear", SqlStringConstructor.GetQuotedString(txtLimitedYear.Text));
                    ht.Add("StorageSites", SqlStringConstructor.GetQuotedString(txtStorageSites.Text));
                    ht.Add("AssetsBackup", SqlStringConstructor.GetQuotedString(txtAssetsBackup.Text));
                    ht.Add("UseUserAccount", SqlStringConstructor.GetQuotedString(cbxUseUserAccount.SelectedValue.ToString()));
                    ht.Add("UseContactor", SqlStringConstructor.GetQuotedString(cbxUseUserAccount.Text));
                    ht.Add("LowConsumables", chkLowConsumables.IsChecked.Value == true ? 1 : 0);

                    FixedAsset fixedAsset1 = new FixedAsset();
                    fixedAsset1.Add(ht);
                }
            }
            MessageBox.Show("添加固定资产成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            InitialData();
        }
        #endregion

        #region 打印列表页面
        
        //删除打印列表
        private void BtDelPrint_Click(object sender, RoutedEventArgs e)
        {
            //删除
            string userAccount = ini.IniReadValue("登录详细", "UserAccount");
            PrintList printlist = new PrintList();
            printlist.Delete(userAccount);
            //重载
            DataView dvlist2 = PrintList.Query_V_PrintList(userAccount);
            dtgPrintList.ItemsSource = dvlist2;
        }

        //打印标识卡
        private void BtPrintMark_Click(object sender, RoutedEventArgs e)
        {

        }

        //删除单个打印项
        private void BtDelPrintItem_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("您确定要从打印列表中删除吗？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                int printListID = Convert.ToInt32(((System.Data.DataRowView)((System.Windows.FrameworkElement)sender).DataContext)["PrintListID"]);
                PrintList printList = new PrintList();
                printList.LoadData1(printListID);
                printList.Delete();
                //重载
                string userAccount = ini.IniReadValue("登录详细", "UserAccount");
                DataView dvlist2 = PrintList.Query_V_PrintList(userAccount);
                dtgPrintList.ItemsSource = dvlist2;
            }
        }
        #endregion

        #region 修改资产页面
        
        /// <summary>
        /// 当前操作资产的ID
        /// </summary>
        int FixedAssetsID { get; set; }

        /// <summary>
        /// 初始化修改资产页面数据
        /// </summary>
        private void InitModifyData()
        {
            BindDrop3();
            int fixedAssetsID = FixedAssetsID;

            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadData(fixedAssetsID);

            txtAssetsCoding1.Text = fixedAsset.AssetsCoding;
            txtAssetName1.Text = fixedAsset.AssetName;
            cbxMajorID1.SelectedValue = fixedAsset.MajorID;

            int majorID = -1;
            if (fixedAsset.MajorID.ToString() != "")
                majorID = Convert.ToInt32(fixedAsset.MajorID.ToString());
            //资产小类数据绑定
            DataView dv1 = SubClass.QuerySubClass(majorID);
            cbxSubID1.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxSubID1.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxSubID1.ItemsSource = dv1;
            cbxSubID1.Text = fixedAsset.SubID.ToString();

            txtSpecificationsModel1.Text = fixedAsset.SpecificationsModel;
            txtBrand1.Text = fixedAsset.Brand;
            txtManufacturer1.Text = fixedAsset.Manufacturer;

            cbxUnitsID1.SelectedValue = fixedAsset.UnitsID;
            cbxUseSituationID1.SelectedValue = fixedAsset.UseSituationID;

            cbxDivisionID1.SelectedValue = fixedAsset.DivisionID;

            int divisionID = -1;
            if (cbxDivisionID1.SelectedItem != null && cbxDivisionID1.SelectedValue != null)
                divisionID = Convert.ToInt32(cbxDivisionID1.SelectedValue);
            //绑定部门数据
            DataView dv2 = Department.QueryDepartment(divisionID);
            cbxDepartmentID1.SelectedValuePath = dv2.Table.Columns[0].Caption;
            cbxDepartmentID1.DisplayMemberPath = dv2.Table.Columns[3].Caption;
            cbxDepartmentID1.ItemsSource = dv2;

            cbxDepartmentID1.SelectedValue = fixedAsset.DepartmentID;


            int departmentID = -1;
            if (cbxDepartmentID1.SelectedItem != null&&cbxDepartmentID1.SelectedValue != null)
                departmentID = Convert.ToInt32(cbxDepartmentID1.SelectedValue);
            //绑定用户数据
            DataView dv3 = UserList.QueryUserLists(departmentID);
            cbxUserAccount1.SelectedValuePath = dv3.Table.Columns[1].Caption;
            cbxUserAccount1.DisplayMemberPath = dv3.Table.Columns[5].Caption;
            cbxUserAccount1.ItemsSource = dv3;

            if (!string.IsNullOrEmpty(fixedAsset.UserAccount) && fixedAsset.UserAccount != "无")
            {
                cbxUserAccount1.SelectedValue = fixedAsset.UserAccount;
            }
            else
            {
                cbxUserAccount1.Text = "无";
            }
            //使用者数据
            cbxUseUserAccount1.SelectedValuePath = dv3.Table.Columns[1].Caption;
            cbxUseUserAccount1.DisplayMemberPath = dv3.Table.Columns[5].Caption;
            cbxUseUserAccount1.ItemsSource = dv3;
            if (!string.IsNullOrEmpty(fixedAsset.UseUserAccount) && fixedAsset.UseUserAccount != "无")
            {
                cbxUseUserAccount1.SelectedValue = fixedAsset.UseUserAccount;
            }
            else
            {
                cbxUseUserAccount1.Text = "无";
            }
            
            cbxAddWaysID1.SelectedValue = fixedAsset.AddWaysID;
            txtOriginalValue1.Text = fixedAsset.OriginalValue.ToString();
            if (fixedAsset.ExFactoryDate.ToString() != "0001-1-1 0:00:00")
            {
                dateExFactoryDate1.Text = fixedAsset.ExFactoryDate.ToShortDateString();
            }
            if (fixedAsset.PurchaseDate.ToString() != "0001-1-1 0:00:00")
            {
                datePurchaseDate1.Text = fixedAsset.PurchaseDate.ToShortDateString();
            }
            if (fixedAsset.RecordedDate.ToString() != "0001-1-1 0:00:00")
            {
                dateRecordedDate1.Text = fixedAsset.RecordedDate.ToShortDateString();
            }
            cbxMethodID1.SelectedValue = fixedAsset.MethodID;
            txtLimitedYear1.Text = fixedAsset.LimitedYear.ToString();

            txtResidualValueRate1.Text = fixedAsset.ResidualValueRate.ToString();                //残值率
            double ShowResiduals = Math.Round(fixedAsset.OriginalValue * fixedAsset.ResidualValueRate, 2);
            mtxtResiduals1.Text = ShowResiduals.ToString();                                       //残值
            double ShowMonthDepreciation = Math.Round((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12), 2);
            mtxtMonthDepreciation1.Text = ShowMonthDepreciation.ToString();                       //本月折旧

            if (fixedAsset.RecordedDate.ToString() != "0001-1-1 0:00:00")
            {
                DateTime start1 = fixedAsset.RecordedDate;
                DateTime end1 = DateTime.Now;
                double ShowRemainderMonth = DateDiff("month", start1, end1);
                if (ShowRemainderMonth >= fixedAsset.LimitedYear * 12)
                {
                    mtxtRemainderMonth1.Text = "0";
                    mtxtNetValue1.Text = "0";

                    double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate), 2);
                    mtxtAccumulatedDepreciation1.Text = ShowAccumulatedDepreciation.ToString();              //累计折旧
                }
                else
                {
                    double ShowRemainderMonth1 = Math.Round((fixedAsset.LimitedYear * 12) - ShowRemainderMonth, 0);
                    mtxtRemainderMonth1.Text = ShowRemainderMonth1.ToString();       //剩余月份

                    double ShowNetValue = Math.Round(((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12)) * ShowRemainderMonth1, 2);
                    mtxtNetValue1.Text = ShowNetValue.ToString();                    //净值

                    double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate) - (((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12)) * ShowRemainderMonth1), 2);
                    mtxtAccumulatedDepreciation1.Text = ShowAccumulatedDepreciation.ToString();                  //累计折旧
                }
            }
            else
            {
                mtxtRemainderMonth1.Text = "0";
                mtxtNetValue1.Text = "0";

                double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate), 2);
                mtxtAccumulatedDepreciation1.Text = ShowAccumulatedDepreciation.ToString();                      //累计折旧
            }


            txtStorageSites1.Text = fixedAsset.StorageSites;
            txtAssetsBackup1.AddLine(fixedAsset.AssetsBackup);

            if (fixedAsset.LowConsumables == 1)
            {
                chkLowConsumables1.IsChecked = true;
            }

            cbxApplyStatus1.Text = fixedAsset.ApplyStatus.ToString();


            //异动历史记录
            DataView dv4 = AssetsChange.QueryAssetsChanges(fixedAssetsID);
            dtgChangesHistoryRecord.ItemsSource = dv4;

            //维修历史记录
            DataView dv5 = RepairList.QueryRepairHistoryRecord(fixedAssetsID);
            dtgRepairHistoryRecord.ItemsSource = dv5;

            //报废历史记录
            DataView dv6 = AssetsScrapped.QueryAssetsScrapped(fixedAssetsID);
            dtgScrappedHistoryRecord.ItemsSource = dv6;
        }

        /// <summary>
        /// 绑定修改资产页面下拉数据
        /// </summary>
        private void BindDrop3()
        {
            //将数据捆绑到下拉列表中
            DataView dv = MajorClass.QueryMajorClass();
            cbxMajorID1.SelectedValuePath = dv.Table.Columns[0].Caption;
            cbxMajorID1.DisplayMemberPath = dv.Table.Columns[2].Caption;
            cbxMajorID1.ItemsSource = dv;
            cbxMajorID1.Text = "资产一级类别";        //第一项中加入内容,重点是绑定后添加
            cbxSubID1.Text = "资产二级类别";

            //绑定单位
            DataView dv1 = UnitList.QueryUnits();
            cbxUnitsID1.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxUnitsID1.DisplayMemberPath = dv1.Table.Columns[2].Caption;
            cbxUnitsID1.ItemsSource = dv1;

            //绑定事业部
            DataView dv2 = Division.QueryDivision();
            cbxDivisionID1.SelectedValuePath = dv2.Table.Columns[0].Caption;
            cbxDivisionID1.DisplayMemberPath = dv2.Table.Columns[2].Caption;
            cbxDivisionID1.ItemsSource = dv2;
            cbxDivisionID1.Text = "请选择事业部";
            cbxDepartmentID1.Text = "请选择部门";

            //绑定增加方式
            DataView dv3 = AddWays.QueryAddWays();
            cbxAddWaysID1.SelectedValuePath = dv3.Table.Columns[0].Caption;
            cbxAddWaysID1.DisplayMemberPath = dv3.Table.Columns[2].Caption;
            cbxAddWaysID1.ItemsSource = dv3;

            //折旧方法
            DataView dv4 = DepreciationMethod.QueryDepreciationMethod();
            cbxMethodID1.SelectedValuePath = dv4.Table.Columns[0].Caption;
            cbxMethodID1.DisplayMemberPath = dv4.Table.Columns[2].Caption;
            cbxMethodID1.ItemsSource = dv4;

            //保管人员
            cbxUserAccount.Text = "无";

            //使用人员
            cbxUseUserAccount.Text = "无";
        }

        //日期差异
        private double DateDiff(string howtocompare, System.DateTime startDate, System.DateTime endDate)
        {
            double diff = 0;
            System.TimeSpan TS = new System.TimeSpan(endDate.Ticks - startDate.Ticks);

            switch (howtocompare.ToLower())
            {
                case "year":
                    diff = Convert.ToDouble(TS.TotalDays / 365);
                    break;
                case "month":
                    diff = Convert.ToDouble((TS.TotalDays / 365) * 12);
                    break;
                case "day":
                    diff = Convert.ToDouble(TS.TotalDays);
                    break;
                case "hour":
                    diff = Convert.ToDouble(TS.TotalHours);
                    break;
                case "minute":
                    diff = Convert.ToDouble(TS.TotalMinutes);
                    break;
                case "second":
                    diff = Convert.ToDouble(TS.TotalSeconds);
                    break;
            }

            return diff;
        }

        //选择资产一级类别
        private void CbxMajorID1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int majorID = -1;
            //获取资产一级ID
            if (cbxMajorID1.SelectedItem != null && cbxMajorID1.SelectedValue != null)
                majorID = Convert.ToInt32(cbxMajorID1.SelectedValue.ToString());
            //绑定资产二级类别数据到下拉列表
            DataView dv1 = SubClass.QuerySubClass(majorID);
            cbxSubID1.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxSubID1.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxSubID1.ItemsSource = dv1;
            cbxSubID1.Text = "资产二级类别";
        }

        //选择事业部
        private void CbxDivisionID1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int divisionID = -1;
            //获取事业部ID
            if (cbxDivisionID1.SelectedItem != null && cbxDivisionID1.SelectedValue != null)
                divisionID = Convert.ToInt32(cbxDivisionID1.SelectedValue.ToString());
            //绑定部门数据到下拉列表
            DataView dv1 = Department.QueryDepartment(divisionID);
            cbxDepartmentID1.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxDepartmentID1.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxDepartmentID1.ItemsSource = dv1;
            cbxDepartmentID1.Text = "请选择部门";
        }

        //选择部门
        private void CbxDepartmentID1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int departmentID = -1;
            if (cbxDepartmentID1.SelectedItem != null && cbxDepartmentID1.SelectedValue != null)
                departmentID = Convert.ToInt32(cbxDepartmentID1.SelectedValue.ToString());
            //加载保管人员
            DataView dv1 = UserList.QueryUserLists(departmentID);
            cbxUserAccount1.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxUserAccount1.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxUserAccount1.ItemsSource = dv1;
            cbxUserAccount1.Text = "无";
            //加载使用人员
            cbxUseUserAccount1.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxUseUserAccount1.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxUseUserAccount1.ItemsSource = dv1;
            cbxUseUserAccount1.Text = "无";
        }

        //确认修改资产
        private void BtnEnterChange_Click(object sender, RoutedEventArgs e)
        {
            //1.验证用户输入
            mtxtType1.Visibility = Visibility.Collapsed;
            mtxtName1.Visibility = Visibility.Collapsed;
            mtxtDepartment1.Visibility = Visibility.Collapsed;
            mtxtLimitedYear1.Visibility = Visibility.Collapsed;
            mtxtNum1.Visibility = Visibility.Collapsed;
            mtxtOriginalValue1.Visibility = Visibility.Collapsed;
            mtxtUserAccount1.Visibility = Visibility.Collapsed;
            mtxtSpecificationsModel1.Visibility = Visibility.Collapsed;
            //验证必要项
            if (cbxMajorID1.SelectedItem == null || cbxSubID1.SelectedItem == null)    //类别
            {
                mtxtType1.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(txtAssetName1.Text)) //名称
            {
                mtxtName1.Visibility = Visibility.Visible;
                return;
            }
            if (cbxDivisionID1.SelectedItem == null || cbxDepartmentID1.SelectedItem == null)   //部门
            {
                mtxtDepartment1.Visibility = Visibility.Visible;
                return;
            }
            if (!int.TryParse(txtLimitedYear1.Text, out int year))  //有限年份
            {
                mtxtLimitedYear1.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(txtAssetsCoding1.Text))  //编码
            {
                mtxtNum1.Visibility = Visibility.Visible;
                return;
            }
            if (!double.TryParse(txtOriginalValue1.Text, out double ori))  //原值
            {
                mtxtOriginalValue1.Visibility = Visibility.Visible;
                return;
            }
            if (cbxUserAccount1.SelectedItem == null)   //保管人员
            {
                mtxtUserAccount1.Visibility = Visibility.Visible;
                return;
            }
            if(string.IsNullOrEmpty(txtSpecificationsModel1.Text))  //规格型号
            {
                mtxtSpecificationsModel1.Visibility = Visibility.Visible;
            }

            //更新资产表信息FixedAssetsID
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.FixedAssetsID = FixedAssetsID;

            Hashtable ht1 = new Hashtable();

            ht1.Add("AssetsCoding", SqlStringConstructor.GetQuotedString(txtAssetsCoding1.Text));
            ht1.Add("AssetName", SqlStringConstructor.GetQuotedString(txtAssetName1.Text));
            ht1.Add("MajorID", SqlStringConstructor.GetQuotedString(cbxMajorID1.SelectedValue.ToString()));
            ht1.Add("MajorName", SqlStringConstructor.GetQuotedString(cbxMajorID1.Text));
            ht1.Add("SubID", SqlStringConstructor.GetQuotedString(cbxSubID1.SelectedValue.ToString()));
            ht1.Add("SubName", SqlStringConstructor.GetQuotedString(cbxSubID1.Text));
            ht1.Add("SpecificationsModel", SqlStringConstructor.GetQuotedString(txtSpecificationsModel1.Text));
            ht1.Add("Brand", SqlStringConstructor.GetQuotedString(txtBrand1.Text));
            ht1.Add("Manufacturer", SqlStringConstructor.GetQuotedString(txtManufacturer1.Text));
            ht1.Add("UnitsID", SqlStringConstructor.GetQuotedString(cbxUnitsID1.SelectedValue.ToString()));
            ht1.Add("UnitsName", SqlStringConstructor.GetQuotedString(cbxUnitsID1.Text));
            ht1.Add("UseSituationID", SqlStringConstructor.GetQuotedString(cbxUseSituationID1.SelectedValue.ToString()));
            ht1.Add("DivisionID", SqlStringConstructor.GetQuotedString(cbxDivisionID1.SelectedValue.ToString()));
            ht1.Add("DivisionName", SqlStringConstructor.GetQuotedString(cbxDivisionID1.Text));
            ht1.Add("DepartmentID", SqlStringConstructor.GetQuotedString(cbxDepartmentID1.SelectedValue.ToString()));
            ht1.Add("DepartmentName", SqlStringConstructor.GetQuotedString(cbxDepartmentID1.Text));
            ht1.Add("UserAccount", SqlStringConstructor.GetQuotedString(cbxUserAccount1.SelectedValue.ToString()));
            ht1.Add("Contactor", SqlStringConstructor.GetQuotedString(cbxUserAccount1.Text));
            ht1.Add("AddWaysID", SqlStringConstructor.GetQuotedString(cbxAddWaysID1.SelectedValue.ToString()));
            ht1.Add("AddWaysName", SqlStringConstructor.GetQuotedString(cbxAddWaysID1.Text));
            ht1.Add("OriginalValue", SqlStringConstructor.GetQuotedString(txtOriginalValue1.Text));
            if (dateExFactoryDate1.ToString() == "")
            {
                ht1.Add("ExFactoryDate", "Null");
            }
            else
            {
                ht1.Add("ExFactoryDate", SqlStringConstructor.GetQuotedString(dateExFactoryDate1.ToString()));
            }
            if (datePurchaseDate1.ToString() == "")
            {
                ht1.Add("PurchaseDate", "Null");
            }
            else
            {
                ht1.Add("PurchaseDate", SqlStringConstructor.GetQuotedString(datePurchaseDate1.ToString()));
            }
            if (dateRecordedDate1.ToString() == "")
            {
                ht1.Add("RecordedDate", "Null");
            }
            else
            {
                ht1.Add("RecordedDate", SqlStringConstructor.GetQuotedString(dateRecordedDate1.ToString()));
            }
            ht1.Add("MethodID", SqlStringConstructor.GetQuotedString(cbxMethodID1.SelectedValue.ToString()));
            ht1.Add("MethodName", SqlStringConstructor.GetQuotedString(cbxMethodID1.Text));
            ht1.Add("LimitedYear", SqlStringConstructor.GetQuotedString(txtLimitedYear1.Text));
            ht1.Add("StorageSites", SqlStringConstructor.GetQuotedString(txtStorageSites1.Text));
            ht1.Add("AssetsBackup", SqlStringConstructor.GetQuotedString(txtAssetsBackup1.Text));
            ht1.Add("UseUserAccount", SqlStringConstructor.GetQuotedString(cbxUseUserAccount1.SelectedValue.ToString()));
            ht1.Add("UseContactor", SqlStringConstructor.GetQuotedString(cbxUseUserAccount1.Text));
            ht1.Add("LowConsumables", chkLowConsumables1.IsChecked == true ? 1 : 0);

            fixedAsset.Update(ht1);

            MessageBox.Show("修改固定资产成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            InitialData();
        }
        #endregion

        #region 异动资产页面
        /// <summary>
        /// 初始化异动资产页面数据
        /// </summary>
        private void InitChangeData()
        {
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadData(FixedAssetsID);

            this.mtxtAssetsCoding2.Text = fixedAsset.AssetsCoding;
            this.mtxtAssetName2.Text = fixedAsset.AssetName;
            this.mtxtAssetClasses2.Text = fixedAsset.MajorName + "  " + fixedAsset.SubName;
            this.mtxtSpecificationsModel2.Text = fixedAsset.SpecificationsModel;
            this.mtxtDivisionDepartment2.Text = fixedAsset.DivisionName + "  " + fixedAsset.DepartmentName;
            this.mtxtContactor2.Text = fixedAsset.Contactor + "(" + fixedAsset.UserAccount + ")";
            this.mtxtStorageSites2.Text = fixedAsset.StorageSites;

            //将数据捆绑到下拉列表中
            DataView dv3 = Division.QueryDivision();
            cbxCDivisionID2.SelectedValuePath = dv3.Table.Columns[0].Caption;
            cbxCDivisionID2.DisplayMemberPath = dv3.Table.Columns[2].Caption;
            cbxCDivisionID2.ItemsSource = dv3;
            cbxCDivisionID2.Text="请选择事业部";
            cbxCDepartmentID2.Text="请选择部门";

            this.txtCChangesDate2.Text = DateTime.Now.ToString();
            //异动历史记录
            DataView dv4 = AssetsChange.QueryAssetsChanges(FixedAssetsID);
            dtgChangesHistoryRecord2.ItemsSource = dv4;
        }

        //选择事业部
        private void CbxCDivisionID2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int cdivisionID = -1;
            //获取事业部ID
            if (cbxCDivisionID2.SelectedItem != null && cbxCDivisionID2.SelectedValue != null)
                cdivisionID = Convert.ToInt32(cbxCDivisionID2.SelectedValue.ToString());
            //绑定部门数据到下拉列表
            DataView dv1 = Department.QueryDepartment(cdivisionID);
            cbxCDepartmentID2.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxCDepartmentID2.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxCDepartmentID2.ItemsSource = dv1;
            cbxCDepartmentID2.Text = "请选择部门";
        }

        //选择部门
        private void CbxCDepartmentID2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int cdepartmentID = -1;
            if (cbxCDepartmentID2.SelectedItem != null && cbxCDepartmentID2.SelectedValue != null)
                cdepartmentID = Convert.ToInt32(cbxCDepartmentID2.SelectedValue.ToString());
            //加载保管人员
            DataView dv1 = UserList.QueryUserLists(cdepartmentID);
            cbxCUserAccount2.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxCUserAccount2.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxCUserAccount2.ItemsSource = dv1;
            cbxCUserAccount2.Text = "无";
        }

        //申请资产异动
        private void BtnApplicationChange_Click(object sender, RoutedEventArgs e)
        {
            //1.验证用户输入
            mtxtDepartment2.Visibility = Visibility.Collapsed;
            mtxtTransferPeople2.Visibility = Visibility.Collapsed;
            mtxtUserAccount2.Visibility = Visibility.Collapsed;
            //验证必要项
            if (cbxCDivisionID2.SelectedItem == null || cbxCDepartmentID2.SelectedItem == null)   //部门
            {
                mtxtDepartment2.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(txtTransferPeople2.Text))  //转移人
            {
                mtxtTransferPeople2.Visibility = Visibility.Visible;
                return;
            }
            if (cbxCUserAccount2.SelectedItem == null)   //保管人员
            {
                mtxtUserAccount2.Visibility = Visibility.Visible;
                return;
            }

            //资产异动信息表
            Hashtable ht = new Hashtable();

            ht.Add("FixedAssetsID", SqlStringConstructor.GetQuotedString(FixedAssetsID.ToString()));
            ht.Add("CDivisionID", SqlStringConstructor.GetQuotedString(cbxCDivisionID2.SelectedValue.ToString()));
            ht.Add("CDivisionName", SqlStringConstructor.GetQuotedString(cbxCDivisionID2.Text));
            ht.Add("CDepartmentID", SqlStringConstructor.GetQuotedString(cbxCDepartmentID2.SelectedValue.ToString()));
            ht.Add("CDepartmentName", SqlStringConstructor.GetQuotedString(cbxCDepartmentID2.Text));
            ht.Add("CUserAccount", SqlStringConstructor.GetQuotedString(cbxCUserAccount2.SelectedValue.ToString()));
            ht.Add("CContactor", SqlStringConstructor.GetQuotedString(cbxCUserAccount2.Text));
            ht.Add("CStorageSites", SqlStringConstructor.GetQuotedString(txtCStorageSites2.Text));
            ht.Add("CChangesDate", SqlStringConstructor.GetQuotedString(txtCChangesDate2.Text));
            ht.Add("TransferPeople", SqlStringConstructor.GetQuotedString(txtTransferPeople2.Text));
            ht.Add("CBackup", SqlStringConstructor.GetQuotedString(mtxtCBackup2.Text));

            AssetsChange assetsChange = new AssetsChange();
            assetsChange.Add(ht);

            //更新资产表信息FixedAssetsID
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.FixedAssetsID = FixedAssetsID;

            Hashtable ht1 = new Hashtable();
            ht1.Add("ApplyStatus", 1);
            ht1.Add("ApplyContactor", SqlStringConstructor.GetQuotedString(txtTransferPeople2.Text));
            ht1.Add("ApplyDate", SqlStringConstructor.GetQuotedString(DateTime.Now.ToString()));

            fixedAsset.Update(ht1);

            MessageBox.Show("异动固定资产成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            InitialData();
        }
        #endregion

        #region 维修资产页面
        /// <summary>
        /// 初始化维修页面数据
        /// </summary>
        private void InitRepairData()
        {
            int fixedAssetsID = FixedAssetsID;

            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadData(fixedAssetsID);

            this.mtxtAssetsCoding3.Text = fixedAsset.AssetsCoding;
            this.mtxtAssetName3.Text = fixedAsset.AssetName;
            this.mtxtAssetClasses3.Text = fixedAsset.MajorName + "  " + fixedAsset.SubName;
            this.mtxtSpecificationsModel3.Text = fixedAsset.SpecificationsModel;

            //显示送修人
            if (ini.ExistINIFile())
            {
                string userAccount = ini.IniReadValue("登录详细", "UserAccount");
                //绑定数据
                this.mtxtShowUserAccount3.Text = ini.IniReadValue("登录详细", "Contactor") + "(" + userAccount + ")";
            }

            //维修历史记录
            DataView dv5 = RepairList.QueryRepairHistoryRecord(fixedAssetsID);
            dtgRepairHistoryRecord1.ItemsSource = dv5;
        }

        //申请维修单击
        private void BtnApplyRepair_Click(object sender, RoutedEventArgs e)
        {
            //1.验证用户输入
            mtxtRepaiContent3.Visibility = Visibility.Collapsed;
            //验证必要项
            if (string.IsNullOrEmpty(mtxtRepairContent3.Text))  //故障描述
            {
                mtxtRepaiContent3.Visibility = Visibility.Visible;
                return;
            }

            //资产维修信息表
            Hashtable ht = new Hashtable();
            ht.Add("FixedAssetsID", SqlStringConstructor.GetQuotedString(FixedAssetsID.ToString()));
            ht.Add("AssetsCoding", SqlStringConstructor.GetQuotedString(mtxtAssetsCoding3.Text));
            ht.Add("AssetName", SqlStringConstructor.GetQuotedString(mtxtAssetName3.Text));
            ht.Add("SpecificationsModel", SqlStringConstructor.GetQuotedString(mtxtSpecificationsModel3.Text));
            ht.Add("RepairUserAccount", SqlStringConstructor.GetQuotedString(ini.IniReadValue("登录详细", "UserAccount")));
            ht.Add("RepairContactor", SqlStringConstructor.GetQuotedString(ini.IniReadValue("登录详细", "Contactor")));
            ht.Add("RepairDivisionID", SqlStringConstructor.GetQuotedString(ini.IniReadValue("登录详细", "DivisionID")));
            ht.Add("RepairDepartmentID", SqlStringConstructor.GetQuotedString(Convert.ToString(ini.IniReadValue("登录详细", "DepartmentID"))));
            ht.Add("RepairTel", SqlStringConstructor.GetQuotedString(ini.IniReadValue("登录详细", "Tel")));
            ht.Add("RepairAddress", SqlStringConstructor.GetQuotedString(ini.IniReadValue("登录详细", "Address")));
            ht.Add("RepairContent", SqlStringConstructor.GetQuotedString(mtxtRepairContent3.Text));
            ht.Add("RepairIP", SqlStringConstructor.GetQuotedString("0"));

            RepairList repairlist = new RepairList();
            repairlist.Add(ht);

            //更新资产表信息FixedAssetsID
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.FixedAssetsID = FixedAssetsID;

            Hashtable ht1 = new Hashtable();
            ht1.Add("ApplyStatus", 2);
            ht1.Add("ApplyContactor", SqlStringConstructor.GetQuotedString(ini.IniReadValue("登录详细", "Contactor")));
            ht1.Add("ApplyDate", SqlStringConstructor.GetQuotedString(DateTime.Now.ToString()));

            fixedAsset.Update(ht1);

            MessageBox.Show("申请维修资产成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            InitialData();
        }
        #endregion

        #region 报废资产页面
        /// <summary>
        /// 初始化报废页面数据
        /// </summary>
        private void InitScrappedData()
        {
            int fixedAssetsID = FixedAssetsID;

            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadData(fixedAssetsID);

            this.mtxtAssetsCoding4.Text = fixedAsset.AssetsCoding;
            this.mtxtAssetName4.Text = fixedAsset.AssetName;
            this.mtxtAssetClasses4.Text = fixedAsset.MajorName + "  " + fixedAsset.SubName;
            this.mtxtSpecificationsModel4.Text = fixedAsset.SpecificationsModel;
        }

        //申请资产报废
        private void BtnApplyScrapped4_Click(object sender, RoutedEventArgs e)
        {
            //1.验证用户输入
            mtxtApplicant4.Visibility = Visibility.Collapsed;
            //验证必要项
            if (string.IsNullOrEmpty(txtApplicant4.Text))  //申请人
            {
                mtxtApplicant4.Visibility = Visibility.Visible;
                return;
            }

            //资产报废信息表
            Hashtable ht = new Hashtable();

            ht.Add("FixedAssetsID", SqlStringConstructor.GetQuotedString(FixedAssetsID.ToString()));
            ht.Add("Applicant", SqlStringConstructor.GetQuotedString(txtApplicant4.Text));
            ht.Add("ReduceWaysID", SqlStringConstructor.GetQuotedString((cbxReduceWaysID4.SelectedIndex + 1).ToString()));
            ht.Add("ReduceWays", SqlStringConstructor.GetQuotedString(cbxReduceWaysID4.Text));
            ht.Add("ScrappedReason", SqlStringConstructor.GetQuotedString(mtxtScrappedReason4.Text));
            ht.Add("ReduceDate", SqlStringConstructor.GetQuotedString(dateReduceDate4.ToString()));

            AssetsScrapped assetsScrapped = new AssetsScrapped();
            assetsScrapped.Add(ht);

            //更新资产表信息FixedAssetsID
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.FixedAssetsID = Convert.ToInt32(FixedAssetsID.ToString());

            Hashtable ht1 = new Hashtable();
            ht1.Add("ApplyStatus", 3);
            ht1.Add("ApplyContactor", SqlStringConstructor.GetQuotedString(txtApplicant4.Text));
            ht1.Add("ApplyDate", SqlStringConstructor.GetQuotedString(DateTime.Now.ToString()));

            fixedAsset.Update(ht1);

            MessageBox.Show("申请报废资产成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            InitialData();
        }

        #endregion

        #region 审核新增资产
        //全选
        private void ChkAll_Click(object sender, RoutedEventArgs e)
        {
            CheckBox headercb = (CheckBox)sender;
            for (int i = 0; i < dtgCheckAddAssets.Items.Count; i++)
            {
                //获取行
                DataGridRow neddrow = (DataGridRow)dtgCheckAddAssets.ItemContainerGenerator.ContainerFromIndex(i);
                //获取该行的某列
                CheckBox cb = (CheckBox)dtgCheckAddAssets.Columns[0].GetCellContent(neddrow);
                cb.IsChecked = headercb.IsChecked;
            }
        }

        /// <summary>
        /// 得到用户的选择
        /// </summary>
        /// <returns>用户选择固定资产的编号集合</returns>
        private ArrayList GetSelectedChkAdd()
        {
            ArrayList selectedItems = new ArrayList();
            for (int i = 0; i < dtgCheckAddAssets.Items.Count; i++)
            {
                //获取行
                DataGridRow neddrow = (DataGridRow)dtgCheckAddAssets.ItemContainerGenerator.ContainerFromIndex(i);
                //获取该行的某列
                CheckBox cb = (CheckBox)dtgCheckAddAssets.Columns[0].GetCellContent(neddrow);
                if (cb.IsChecked != null || (bool)cb.IsChecked)
                {
                    if (string.IsNullOrEmpty((dtgCheckAddAssets.Columns[1].GetCellContent(neddrow) as TextBlock).Text))
                    {
                        selectedItems.Add(Convert.ToInt32((dtgCheckAddAssets.Columns[1].GetCellContent(neddrow) as TextBlock).Text));
                    }
                }
            }
            return selectedItems;
        }

        //确认审核新增资产
        private void BtEnterCheckAdd_Click(object sender, RoutedEventArgs e)
        {
            ArrayList selectedFixedAssets = this.GetSelected();

            //如果用户没有选择,就单击该按钮,则给出警告
            if (selectedFixedAssets.Count == 0)
            {
                MessageBox.Show("请选择固定资产！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            //更新资产表信息FixedAssetsID
            FixedAsset fixedAsset = new FixedAsset();
            Hashtable ht1 = new Hashtable();

            //循环更新固定资产信息
            foreach (int fixedAssetsID in selectedFixedAssets)
            {
                ht1.Clear();
                fixedAsset.FixedAssetsID = Convert.ToInt32(fixedAssetsID);

                ht1.Add("ApplyStatus", 0);
                ht1.Add("AssetStatus", 0);
                ht1.Add("ApplyContactor", "null");
                ht1.Add("ApplyDate", "null");

                fixedAsset.Update(ht1);
            }
            MessageBox.Show("审核新增资产成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            InitialData();
        }

        //审核单项新增资产
        private void BtCheck_Click(object sender, RoutedEventArgs e)
        {
            FixedAssetsID = Convert.ToInt32(((System.Data.DataRowView)((System.Windows.FrameworkElement)sender).DataContext)["FixedAssetsID"]);
            tabpChkAddAssets.IsSelected = true;
            InitCheckAddData();
        }
        #endregion

        #region 审核新增资产项
        /// <summary>
        /// 初始化修改资产页面数据
        /// </summary>
        private void InitCheckAddData()
        {
            BindDrop4();
            int fixedAssetsID = FixedAssetsID;

            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadData(fixedAssetsID);

            txtAssetsCoding5.Text = fixedAsset.AssetsCoding;
            txtAssetName5.Text = fixedAsset.AssetName;
            cbxMajorID5.SelectedValue = fixedAsset.MajorID;

            int majorID = -1;
            if (fixedAsset.MajorID.ToString() != "")
                majorID = Convert.ToInt32(fixedAsset.MajorID.ToString());
            //资产小类数据绑定
            DataView dv1 = SubClass.QuerySubClass(majorID);
            cbxSubID5.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxSubID5.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxSubID5.ItemsSource = dv1;
            cbxSubID5.Text = fixedAsset.SubID.ToString();

            txtSpecificationsModel5.Text = fixedAsset.SpecificationsModel;
            txtBrand5.Text = fixedAsset.Brand;
            txtManufacturer5.Text = fixedAsset.Manufacturer;

            cbxUnitsID5.SelectedValue = fixedAsset.UnitsID;
            cbxUseSituationID5.SelectedValue = fixedAsset.UseSituationID;

            cbxDivisionID5.SelectedValue = fixedAsset.DivisionID;

            int divisionID = -1;
            if (cbxDivisionID5.SelectedItem != null && cbxDivisionID5.SelectedValue != null)
                divisionID = Convert.ToInt32(cbxDivisionID5.SelectedValue);
            //绑定部门数据
            DataView dv2 = Department.QueryDepartment(divisionID);
            cbxDepartmentID5.SelectedValuePath = dv2.Table.Columns[0].Caption;
            cbxDepartmentID5.DisplayMemberPath = dv2.Table.Columns[3].Caption;
            cbxDepartmentID5.ItemsSource = dv2;
            cbxDepartmentID5.SelectedValue = fixedAsset.DepartmentID;

            int departmentID = -1;
            if (cbxDepartmentID5.SelectedItem != null && cbxDepartmentID5.SelectedValue != null)
                departmentID = Convert.ToInt32(cbxDepartmentID5.SelectedValue);
            //绑定用户数据
            DataView dv3 = UserList.QueryUserLists(departmentID);
            cbxUserAccount5.SelectedValuePath = dv3.Table.Columns[1].Caption;
            cbxUserAccount5.DisplayMemberPath = dv3.Table.Columns[5].Caption;
            cbxUserAccount5.ItemsSource = dv3;

            if (!string.IsNullOrEmpty(fixedAsset.UserAccount) && fixedAsset.UserAccount != "无")
            {
                cbxUserAccount5.SelectedValue = fixedAsset.UserAccount;
            }
            else
            {
                cbxUserAccount5.Text = "无";
            }
            //使用者数据
            cbxUseUserAccount5.SelectedValuePath = dv3.Table.Columns[1].Caption;
            cbxUseUserAccount5.DisplayMemberPath = dv3.Table.Columns[5].Caption;
            cbxUseUserAccount5.ItemsSource = dv3;
            if (!string.IsNullOrEmpty(fixedAsset.UseUserAccount) && fixedAsset.UseUserAccount != "无")
            {
                cbxUseUserAccount5.SelectedValue = fixedAsset.UseUserAccount;
            }
            else
            {
                cbxUseUserAccount5.Text = "无";
            }

            cbxAddWaysID5.SelectedValue = fixedAsset.AddWaysID;
            txtOriginalValue5.Text = fixedAsset.OriginalValue.ToString();
            if (fixedAsset.ExFactoryDate.ToString() != "0001-1-1 0:00:00")
            {
                dateExFactoryDate5.Text = fixedAsset.ExFactoryDate.ToShortDateString();
            }
            if (fixedAsset.PurchaseDate.ToString() != "0001-1-1 0:00:00")
            {
                datePurchaseDate5.Text = fixedAsset.PurchaseDate.ToShortDateString();
            }
            if (fixedAsset.RecordedDate.ToString() != "0001-1-1 0:00:00")
            {
                dateRecordedDate5.Text = fixedAsset.RecordedDate.ToShortDateString();
            }
            cbxMethodID5.SelectedValue = fixedAsset.MethodID;
            txtLimitedYear5.Text = fixedAsset.LimitedYear.ToString();

            txtResidualValueRate5.Text = fixedAsset.ResidualValueRate.ToString();                //残值率
            double ShowResiduals = Math.Round(fixedAsset.OriginalValue * fixedAsset.ResidualValueRate, 2);
            mtxtResiduals5.Text = ShowResiduals.ToString();                                       //残值
            double ShowMonthDepreciation = Math.Round((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12), 2);
            mtxtMonthDepreciation5.Text = ShowMonthDepreciation.ToString();                       //本月折旧

            if (fixedAsset.RecordedDate.ToString() != "0001-1-1 0:00:00")
            {
                DateTime start1 = fixedAsset.RecordedDate;
                DateTime end1 = DateTime.Now;
                double ShowRemainderMonth = DateDiff("month", start1, end1);
                if (ShowRemainderMonth >= fixedAsset.LimitedYear * 12)
                {
                    mtxtRemainderMonth5.Text = "0";
                    mtxtNetValue5.Text = "0";

                    double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate), 2);
                    mtxtAccumulatedDepreciation5.Text = ShowAccumulatedDepreciation.ToString();              //累计折旧
                }
                else
                {
                    double ShowRemainderMonth1 = Math.Round((fixedAsset.LimitedYear * 12) - ShowRemainderMonth, 0);
                    mtxtRemainderMonth5.Text = ShowRemainderMonth1.ToString();       //剩余月份

                    double ShowNetValue = Math.Round(((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12)) * ShowRemainderMonth1, 2);
                    mtxtNetValue5.Text = ShowNetValue.ToString();                    //净值

                    double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate) - (((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12)) * ShowRemainderMonth1), 2);
                    mtxtAccumulatedDepreciation5.Text = ShowAccumulatedDepreciation.ToString();                  //累计折旧
                }
            }
            else
            {
                mtxtRemainderMonth5.Text = "0";
                mtxtNetValue5.Text = "0";

                double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate), 2);
                mtxtAccumulatedDepreciation5.Text = ShowAccumulatedDepreciation.ToString();                      //累计折旧
            }


            txtStorageSites5.Text = fixedAsset.StorageSites;
            txtAssetsBackup5.AddLine(fixedAsset.AssetsBackup);

            if (fixedAsset.LowConsumables == 1)
            {
                chkLowConsumables5.IsChecked = true;
            }

            cbxApplyStatus5.Text = fixedAsset.ApplyStatus.ToString();
        }

        /// <summary>
        /// 绑定修改资产页面下拉数据
        /// </summary>
        private void BindDrop4()
        {
            //将数据捆绑到下拉列表中
            DataView dv = MajorClass.QueryMajorClass();
            cbxMajorID5.SelectedValuePath = dv.Table.Columns[0].Caption;
            cbxMajorID5.DisplayMemberPath = dv.Table.Columns[2].Caption;
            cbxMajorID5.ItemsSource = dv;
            cbxMajorID5.Text = "资产一级类别";        //第一项中加入内容,重点是绑定后添加
            cbxSubID5.Text = "资产二级类别";

            //绑定单位
            DataView dv1 = UnitList.QueryUnits();
            cbxUnitsID5.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxUnitsID5.DisplayMemberPath = dv1.Table.Columns[2].Caption;
            cbxUnitsID5.ItemsSource = dv1;

            //绑定事业部
            DataView dv2 = Division.QueryDivision();
            cbxDivisionID5.SelectedValuePath = dv2.Table.Columns[0].Caption;
            cbxDivisionID5.DisplayMemberPath = dv2.Table.Columns[2].Caption;
            cbxDivisionID5.ItemsSource = dv2;
            cbxDivisionID5.Text = "请选择事业部";
            cbxDepartmentID5.Text = "请选择部门";

            //绑定增加方式
            DataView dv3 = AddWays.QueryAddWays();
            cbxAddWaysID5.SelectedValuePath = dv3.Table.Columns[0].Caption;
            cbxAddWaysID5.DisplayMemberPath = dv3.Table.Columns[2].Caption;
            cbxAddWaysID5.ItemsSource = dv3;

            //折旧方法
            DataView dv4 = DepreciationMethod.QueryDepreciationMethod();
            cbxMethodID5.SelectedValuePath = dv4.Table.Columns[0].Caption;
            cbxMethodID5.DisplayMemberPath = dv4.Table.Columns[2].Caption;
            cbxMethodID5.ItemsSource = dv4;

            //保管人员
            cbxUserAccount.Text = "无";

            //使用人员
            cbxUseUserAccount.Text = "无";
        }

        //选择一级类别
        private void CbxMajorID5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int majorID = -1;
            //获取资产一级ID
            if (cbxMajorID5.SelectedItem != null && cbxMajorID5.SelectedValue != null)
                majorID = Convert.ToInt32(cbxMajorID5.SelectedValue.ToString());
            //绑定资产二级类别数据到下拉列表
            DataView dv1 = SubClass.QuerySubClass(majorID);
            cbxSubID5.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxSubID5.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxSubID5.ItemsSource = dv1;
            cbxSubID5.Text = "资产二级类别";
        }

        //选择资产二级类别
        private void CbxSubID5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int subID = -1;
            //获取资产二级ID
            if (cbxSubID5.SelectedItem != null && cbxSubID5.SelectedValue != null)
                subID = Convert.ToInt32(cbxSubID5.SelectedValue.ToString());
            SubClass subClass = new SubClass();
            //二级资产初始化
            subClass.LoadData(subID);
            if (subClass.Exist) //是否存在标志
            {
                cbxUnitsID5.SelectedValue = subClass.UnitsID.ToString();  //计量单位
                txtLimitedYear5.Text = subClass.UsefulLife.ToString();  //有限年份
                txtResidualValueRate5.Text = subClass.DepreciationRate.ToString();  //残值率
            }

            //计算资产编码
            if (cbxMajorID5.SelectedValue == null || cbxSubID5.SelectedValue == null)
            {
                return;
            }
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadData1(subID);
            if (fixedAsset.Exist)
            {
                int ShowFixedAssetsID = fixedAsset.FixedAssetsID + 1;

                txtAssetsCoding5.Text = cbxMajorID5.SelectedValue.ToString() + cbxSubID5.SelectedValue.ToString() + ShowFixedAssetsID.ToString();
            }
            else
            {
                txtAssetsCoding5.Text = cbxMajorID5.SelectedValue.ToString() + cbxSubID5.SelectedValue.ToString() + "10000";
            }
        }

        //选择事业部
        private void CbxDivisionID5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int divisionID = -1;
            //获取事业部ID
            if (cbxDivisionID5.SelectedItem != null && cbxDivisionID5.SelectedValue != null)
                divisionID = Convert.ToInt32(cbxDivisionID5.SelectedValue.ToString());
            //绑定部门数据到下拉列表
            DataView dv1 = Department.QueryDepartment(divisionID);
            cbxDepartmentID5.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxDepartmentID5.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxDepartmentID5.ItemsSource = dv1;
            cbxDepartmentID5.Text = "请选择部门";
        }

        //选择部门
        private void CbxDepartmentID5_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int departmentID = -1;
            if (cbxDepartmentID5.SelectedItem != null && cbxDepartmentID5.SelectedValue != null)
                departmentID = Convert.ToInt32(cbxDepartmentID5.SelectedValue.ToString());
            //加载保管人员
            DataView dv1 = UserList.QueryUserLists(departmentID);
            cbxUserAccount5.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxUserAccount5.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxUserAccount5.ItemsSource = dv1;
            cbxUserAccount5.Text = "无";
            //加载使用人员
            cbxUseUserAccount5.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxUseUserAccount5.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxUseUserAccount5.ItemsSource = dv1;
            cbxUseUserAccount5.Text = "无";
        }

        //确认审核新增资产
        private void BtnEnterCheckAddAsset_Click(object sender, RoutedEventArgs e)
        {
            //1.验证用户输入
            mtxtType5.Visibility = Visibility.Collapsed;
            mtxtName5.Visibility = Visibility.Collapsed;
            mtxtDepartment5.Visibility = Visibility.Collapsed;
            mtxtLimitedYear5.Visibility = Visibility.Collapsed;
            mtxtNum5.Visibility = Visibility.Collapsed;
            mtxtOriginalValue5.Visibility = Visibility.Collapsed;
            mtxtUserAccount5.Visibility = Visibility.Collapsed;
            mtxtSpecificationsModel5.Visibility = Visibility.Collapsed;
            //验证必要项
            if (cbxMajorID5.SelectedItem == null || cbxSubID5.SelectedItem == null)    //类别
            {
                mtxtType5.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(txtAssetName5.Text)) //名称
            {
                mtxtName5.Visibility = Visibility.Visible;
                return;
            }
            if (cbxDivisionID5.SelectedItem == null || cbxDepartmentID5.SelectedItem == null)   //部门
            {
                mtxtDepartment5.Visibility = Visibility.Visible;
                return;
            }
            if (!int.TryParse(txtLimitedYear5.Text, out int year))  //有限年份
            {
                mtxtLimitedYear5.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(txtAssetsCoding5.Text))  //编码
            {
                mtxtNum5.Visibility = Visibility.Visible;
                return;
            }
            if (!double.TryParse(txtOriginalValue5.Text, out double ori))  //原值
            {
                mtxtOriginalValue5.Visibility = Visibility.Visible;
                return;
            }
            if (cbxUserAccount5.SelectedItem == null)   //保管人员
            {
                mtxtUserAccount5.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(txtSpecificationsModel5.Text))  //规格型号
            {
                mtxtSpecificationsModel5.Visibility = Visibility.Visible;
            }

            //更新资产表信息FixedAssetsID
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.FixedAssetsID = FixedAssetsID;

            Hashtable ht1 = new Hashtable();

            ht1.Add("AssetsCoding", SqlStringConstructor.GetQuotedString(txtAssetsCoding5.Text));
            ht1.Add("AssetName", SqlStringConstructor.GetQuotedString(txtAssetName5.Text));
            ht1.Add("MajorID", SqlStringConstructor.GetQuotedString(cbxMajorID5.SelectedValue.ToString()));
            ht1.Add("MajorName", SqlStringConstructor.GetQuotedString(cbxMajorID5.Text));
            ht1.Add("SubID", SqlStringConstructor.GetQuotedString(cbxSubID5.SelectedValue.ToString()));
            ht1.Add("SubName", SqlStringConstructor.GetQuotedString(cbxSubID5.Text));
            ht1.Add("SpecificationsModel", SqlStringConstructor.GetQuotedString(txtSpecificationsModel5.Text));
            ht1.Add("Brand", SqlStringConstructor.GetQuotedString(txtBrand5.Text));
            ht1.Add("Manufacturer", SqlStringConstructor.GetQuotedString(txtManufacturer5.Text));
            ht1.Add("UnitsID", SqlStringConstructor.GetQuotedString(cbxUnitsID5.SelectedValue.ToString()));
            ht1.Add("UnitsName", SqlStringConstructor.GetQuotedString(cbxUnitsID5.Text));
            ht1.Add("UseSituationID", SqlStringConstructor.GetQuotedString(cbxUseSituationID5.SelectedValue.ToString()));
            ht1.Add("DivisionID", SqlStringConstructor.GetQuotedString(cbxDivisionID5.SelectedValue.ToString()));
            ht1.Add("DivisionName", SqlStringConstructor.GetQuotedString(cbxDivisionID5.Text));
            ht1.Add("DepartmentID", SqlStringConstructor.GetQuotedString(cbxDepartmentID5.SelectedValue.ToString()));
            ht1.Add("DepartmentName", SqlStringConstructor.GetQuotedString(cbxDepartmentID5.Text));
            ht1.Add("UserAccount", SqlStringConstructor.GetQuotedString(cbxUserAccount5.SelectedValue.ToString()));
            ht1.Add("Contactor", SqlStringConstructor.GetQuotedString(cbxUserAccount5.Text));
            ht1.Add("AddWaysID", SqlStringConstructor.GetQuotedString(cbxAddWaysID5.SelectedValue.ToString()));
            ht1.Add("AddWaysName", SqlStringConstructor.GetQuotedString(cbxAddWaysID5.Text));
            ht1.Add("OriginalValue", SqlStringConstructor.GetQuotedString(txtOriginalValue5.Text));
            if (dateExFactoryDate5.ToString() == "")
            {
                ht1.Add("ExFactoryDate", "Null");
            }
            else
            {
                ht1.Add("ExFactoryDate", SqlStringConstructor.GetQuotedString(dateExFactoryDate5.ToString()));
            }
            if (datePurchaseDate5.ToString() == "")
            {
                ht1.Add("PurchaseDate", "Null");
            }
            else
            {
                ht1.Add("PurchaseDate", SqlStringConstructor.GetQuotedString(datePurchaseDate5.ToString()));
            }
            if (dateRecordedDate5.ToString() == "")
            {
                ht1.Add("RecordedDate", "Null");
            }
            else
            {
                ht1.Add("RecordedDate", SqlStringConstructor.GetQuotedString(dateRecordedDate5.ToString()));
            }
            ht1.Add("MethodID", SqlStringConstructor.GetQuotedString(cbxMethodID5.SelectedValue.ToString()));
            ht1.Add("MethodName", SqlStringConstructor.GetQuotedString(cbxMethodID5.Text));
            ht1.Add("LimitedYear", SqlStringConstructor.GetQuotedString(txtLimitedYear5.Text));
            ht1.Add("StorageSites", SqlStringConstructor.GetQuotedString(txtStorageSites5.Text));
            ht1.Add("AssetsBackup", SqlStringConstructor.GetQuotedString(txtAssetsBackup5.Text));
            ht1.Add("UseUserAccount", SqlStringConstructor.GetQuotedString(cbxUseUserAccount5.SelectedValue.ToString()));
            ht1.Add("UseContactor", SqlStringConstructor.GetQuotedString(cbxUseUserAccount5.Text));
            ht1.Add("LowConsumables", chkLowConsumables5.IsChecked == true ? 1 : 0);
            ht1.Add("ApplyStatus", 0);
            ht1.Add("AssetStatus", 0);
            ht1.Add("ApplyContactor", "null");
            ht1.Add("ApplyDate", "null");

            fixedAsset.Update(ht1);

            MessageBox.Show("审核新增资产成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            InitialData();
        }

        #endregion

        #region 审核异动资产
        //审核单项异动资产
        private void BtCheckChangeItem_Click(object sender, RoutedEventArgs e)
        {
            FixedAssetsID = Convert.ToInt32(((System.Data.DataRowView)((System.Windows.FrameworkElement)sender).DataContext)["FixedAssetsID"]);
            tabpCheckChangeAsset.IsSelected = true;
            InitCheckChangeData();
        }
        #endregion

        #region 审核异动资产项
        /// <summary>
        /// 异动资产ID
        /// </summary>
        public string AssetsChangesID { get; set; }

        /// <summary>
        /// 初始化异动资产页面数据
        /// </summary>
        private void InitCheckChangeData()
        {
            BindDrop5();
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadData(FixedAssetsID);

            txtAssetsCoding6.Text = fixedAsset.AssetsCoding;
            txtAssetName6.Text = fixedAsset.AssetName;
            cbxMajorID6.SelectedValue = fixedAsset.MajorID;
            //资产小类
            int majorID = -1;
            if (fixedAsset.MajorID.ToString() != "")
                majorID = Convert.ToInt32(fixedAsset.MajorID.ToString());
            DataView dv1 = SubClass.QuerySubClass(majorID);
            cbxSubID6.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxSubID6.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxSubID6.ItemsSource = dv1;
            cbxSubID6.SelectedValue = fixedAsset.SubID;
            //规格型号
            txtSpecificationsModel6.Text = fixedAsset.SpecificationsModel;
            //品牌
            txtBrand6.Text = fixedAsset.Brand;
            //生产厂家
            txtManufacturer6.Text = fixedAsset.Manufacturer;
            //单位
            cbxUnitsID6.SelectedValue = fixedAsset.UnitsID;
            //使用情况
            cbxUseSituationID6.SelectedIndex = fixedAsset.UseSituationID+1;
            //事业部
            cbxDivisionID6.SelectedValue = fixedAsset.DivisionID;
            //部门
            int divisionID = -1;
            if (cbxDivisionID6.SelectedItem!=null&& cbxDivisionID6.SelectedValue!=null)
                divisionID = Convert.ToInt32(cbxDivisionID6.SelectedValue);
            DataView dv2 = Department.QueryDepartment(divisionID);
            cbxDepartmentID6.SelectedValuePath = dv2.Table.Columns[0].Caption;
            cbxDepartmentID6.DisplayMemberPath = dv2.Table.Columns[3].Caption;
            cbxDepartmentID6.ItemsSource = dv2;
            cbxDepartmentID6.SelectedValue = fixedAsset.DepartmentID;
            //保管人员
            int departmentID = -1;
            if (cbxDepartmentID6.SelectedItem!=null&& cbxDepartmentID6.SelectedValue!=null)
                departmentID = Convert.ToInt32(cbxDepartmentID6.SelectedValue);
            DataView dv3 = UserList.QueryUserLists(departmentID);
            cbxUserAccount6.SelectedValuePath = dv3.Table.Columns[1].Caption;
            cbxUserAccount6.DisplayMemberPath = dv3.Table.Columns[5].Caption;
            cbxUserAccount6.ItemsSource = dv3;
            if(string.IsNullOrEmpty(fixedAsset.UserAccount))
            {
                cbxUserAccount6.Text = "无";

            }
            else
            {
                cbxUserAccount6.SelectedValue = fixedAsset.UserAccount;

            }
            //使用人员
            cbxUseUserAccount6.SelectedValuePath = dv3.Table.Columns[1].Caption;
            cbxUseUserAccount6.DisplayMemberPath = dv3.Table.Columns[5].Caption;
            cbxUseUserAccount6.ItemsSource = dv3;
            if(string.IsNullOrEmpty(fixedAsset.UseUserAccount))
            {
                cbxUseUserAccount6.Text = "无";
            }
            {
                cbxUseUserAccount6.SelectedValue = fixedAsset.UseUserAccount;
            }
            //增加方式
            cbxAddWaysID6.SelectedIndex = fixedAsset.AddWaysID-1;
            //原值
            txtOriginalValue6.Text = fixedAsset.OriginalValue.ToString();
            //出厂日期
            if (fixedAsset.ExFactoryDate.ToString() != "0001-1-1 0:00:00")
            {
                dateExFactoryDate6.Text = fixedAsset.ExFactoryDate.ToShortDateString();
            }
            //购置日期
            if (fixedAsset.PurchaseDate.ToString() != "0001-1-1 0:00:00")
            {
                datePurchaseDate6.Text = fixedAsset.PurchaseDate.ToShortDateString();
            }
            //入账日期
            if (fixedAsset.RecordedDate.ToString() != "0001-1-1 0:00:00")
            {
                dateRecordedDate6.Text = fixedAsset.RecordedDate.ToShortDateString();
            }
            //折旧方法
            cbxMethodID6.SelectedValue = fixedAsset.MethodID;
            //有限年份
            txtLimitedYear6.Text = fixedAsset.LimitedYear.ToString();
            txtResidualValueRate6.Text = fixedAsset.ResidualValueRate.ToString();                //残值率
            double ShowResiduals = Math.Round(fixedAsset.OriginalValue * fixedAsset.ResidualValueRate, 2);
            mtxtResiduals6.Text = ShowResiduals.ToString();                                       //残值
            double ShowMonthDepreciation = Math.Round((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12), 2);
            mtxtMonthDepreciation6.Text = ShowMonthDepreciation.ToString();                       //本月折旧

            if (fixedAsset.RecordedDate.ToString() != "0001-1-1 0:00:00")
            {
                DateTime start1 = fixedAsset.RecordedDate;
                DateTime end1 = DateTime.Now;
                double ShowRemainderMonth = DateDiff("month", start1, end1);
                if (ShowRemainderMonth >= fixedAsset.LimitedYear * 12)
                {
                    mtxtRemainderMonth6.Text = "0";
                    mtxtNetValue6.Text = "0";

                    double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate), 2);
                    mtxtAccumulatedDepreciation6.Text = ShowAccumulatedDepreciation.ToString();              //累计折旧
                }
                else
                {
                    double ShowRemainderMonth1 = Math.Round((fixedAsset.LimitedYear * 12) - ShowRemainderMonth, 0);
                    mtxtRemainderMonth6.Text = ShowRemainderMonth1.ToString();       //剩余月份

                    double ShowNetValue = Math.Round(((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12)) * ShowRemainderMonth1, 2);
                    mtxtNetValue6.Text = ShowNetValue.ToString();                    //净值

                    double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate) - (((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12)) * ShowRemainderMonth1), 2);
                    mtxtAccumulatedDepreciation6.Text = ShowAccumulatedDepreciation.ToString();                  //累计折旧
                }
            }
            else
            {
                mtxtRemainderMonth6.Text = "0";
                mtxtNetValue6.Text = "0";

                double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate), 2);
                mtxtAccumulatedDepreciation6.Text = ShowAccumulatedDepreciation.ToString();                      //累计折旧
            }
            //存放地点
            txtStorageSites6.Text = fixedAsset.StorageSites;
            //资产备注
            txtAssetsBackup5.AddLine(fixedAsset.AssetsBackup);
            //是否低值易耗品
            if (fixedAsset.LowConsumables == 1)
            {
                chkLowConsumables6.IsChecked = true;
            }
            //ApplyStatus.Text = fixedAsset.ApplyStatus.ToString();
            //异动固定资产信息初始化
            mtxtAssetsCoding6.Text = fixedAsset.AssetsCoding;
            mtxtAssetName6.Text = fixedAsset.AssetName;
            mtxtDivisionDepartment6.Text = fixedAsset.DivisionName + "  " + fixedAsset.DepartmentName;
            mtxtContactor6.Text = fixedAsset.Contactor;
            mtxtStorageSites6.Text = fixedAsset.StorageSites;

            int c_fixedAssetsID = fixedAsset.FixedAssetsID;
            AssetsChange assetsChange = new AssetsChange();
            assetsChange.LoadData1(c_fixedAssetsID);

            cbxCDivisionID6.SelectedValue = assetsChange.CDivisionID;

            int cdivisionID = -1;
            if (cbxCDivisionID6.SelectedItem != null&&cbxCDivisionID6.SelectedValue!=null)
                cdivisionID = Convert.ToInt32(cbxCDivisionID6.SelectedValue);

            DataView dv4 = Department.QueryDepartment(cdivisionID);
            cbxCDepartmentID6.SelectedValuePath = dv4.Table.Columns[0].Caption;
            cbxCDepartmentID6.DisplayMemberPath = dv4.Table.Columns[3].Caption;
            cbxCDepartmentID6.ItemsSource = dv4;

            cbxCDepartmentID6.SelectedValue = assetsChange.CDepartmentID;

            int cDepartmentID = -1;
            if (cbxCDepartmentID6.SelectedItem != null&& cbxCDepartmentID6.SelectedValue != null)
                cDepartmentID = Convert.ToInt32(cbxCDepartmentID6.SelectedValue);

            DataView dv5 = UserList.QueryUserLists(cDepartmentID);
            cbxCUserAccount6.SelectedValuePath = dv5.Table.Columns[1].Caption;
            cbxCUserAccount6.DisplayMemberPath = dv5.Table.Columns[5].Caption;
            cbxCUserAccount6.ItemsSource = dv5;
            if(string.IsNullOrEmpty(assetsChange.CUserAccount))
            {
                cbxCUserAccount6.Text = "无";
            }
            else
            {
                cbxCUserAccount6.SelectedValue = assetsChange.CUserAccount;
            }

            txtCStorageSites6.Text = assetsChange.CStorageSites;
            txtCChangesDate6.Text = assetsChange.CChangesDate.ToString();
            txtTransferPeople6.Text = assetsChange.TransferPeople;
            mtxtCBackup6.Add(assetsChange.CBackup);

            AssetsChangesID = assetsChange.AssetsChangesID.ToString();
            cbxUseSituationID6.SelectedIndex = fixedAsset.UseSituationID-1;

            //异动历史记录
            DataView dv6 = AssetsChange.QueryAssetsChanges(FixedAssetsID);
            dtgChangesHistoryRecord6.ItemsSource = dv6;
        }

        //绑定下拉列表
        private void BindDrop5()
        {
            //将数据捆绑到下拉列表中
            DataView dv = MajorClass.QueryMajorClass();
            cbxMajorID6.SelectedValuePath = dv.Table.Columns[0].Caption;
            cbxMajorID6.DisplayMemberPath = dv.Table.Columns[2].Caption;
            cbxMajorID6.ItemsSource = dv;
            cbxMajorID6.Text="资产一级类别";        //第一项中加入内容,重点是绑定后添加
            cbxSubID6.Text="资产二级类别";          //第一项中加入内容,重点是绑定后添加

            //绑定单位
            DataView dv1 = UnitList.QueryUnits();
            cbxUnitsID6.SelectedValuePath = dv1.Table.Columns[0].Caption.ToString();
            cbxUnitsID6.DisplayMemberPath = dv1.Table.Columns[2].Caption.ToString();
            cbxUnitsID6.ItemsSource = dv1;

            //绑定增加方式
            DataView dv2 = AddWays.QueryAddWays();
            cbxAddWaysID6.SelectedValuePath = dv2.Table.Columns[0].Caption.ToString();
            cbxAddWaysID6.DisplayMemberPath = dv2.Table.Columns[2].Caption.ToString();
            cbxAddWaysID6.ItemsSource = dv2;

            //将数据捆绑到下拉列表中
            DataView dv3 = Division.QueryDivision();
            cbxDivisionID6.SelectedValuePath = dv3.Table.Columns[0].Caption;
            cbxDivisionID6.DisplayMemberPath = dv3.Table.Columns[2].Caption;
            cbxDivisionID6.ItemsSource = dv3;

            //折旧方法
            DataView dv4 = DepreciationMethod.QueryDepreciationMethod();
            cbxMethodID6.SelectedValuePath = dv4.Table.Columns[0].Caption;
            cbxMethodID6.DisplayMemberPath = dv4.Table.Columns[2].Caption;
            cbxMethodID6.ItemsSource = dv4;

            cbxDivisionID6.Text = "请选择事业部";        //第一项中加入内容,重点是绑定后添加
            cbxDepartmentID6.Text = "请选择部门";          //第一项中加入内容,重点是绑定后添加
            cbxUserAccount6.Text = "无";
            cbxUseUserAccount6.Text = "无";

            //异动
            //将数据捆绑到下拉列表中
            DataView dv5 = Division.QueryDivision();
            cbxCDivisionID6.SelectedValuePath = dv5.Table.Columns[0].Caption;
            cbxCDivisionID6.DisplayMemberPath = dv5.Table.Columns[2].Caption;
            cbxCDivisionID6.ItemsSource = dv5;

            cbxCDivisionID6.Text = "请选择事业部";        //第一项中加入内容,重点是绑定后添加
            cbxCDepartmentID6.Text = "请选择部门";          //第一项中加入内容,重点是绑定后添加
            cbxCUserAccount6.Text = "无";
        }

        //选择事业部
        private void CbxCDivisionID6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int cdivisionID = -1;
            //获取事业部ID
            if (cbxCDivisionID6.SelectedItem != null && cbxCDivisionID6.SelectedValue != null)
                cdivisionID = Convert.ToInt32(cbxCDivisionID6.SelectedValue.ToString());
            //绑定部门数据到下拉列表
            DataView dv1 = Department.QueryDepartment(cdivisionID);
            cbxCDepartmentID6.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxCDepartmentID6.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxCDepartmentID6.ItemsSource = dv1;
            cbxCDepartmentID6.Text = "请选择部门";
        }

        //选择部门
        private void CbxCDepartmentID6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int cdepartmentID = -1;
            if (cbxCDepartmentID6.SelectedItem != null && cbxCDepartmentID6.SelectedValue != null)
                cdepartmentID = Convert.ToInt32(cbxCDepartmentID6.SelectedValue.ToString());
            //加载保管人员
            DataView dv1 = UserList.QueryUserLists(cdepartmentID);
            cbxCUserAccount6.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxCUserAccount6.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxCUserAccount6.ItemsSource = dv1;
            cbxCUserAccount6.Text = "无";
        }

        

        //选择资产一级类别
        private void CbxMajorID6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int majorID = -1;
            if (cbxMajorID6.SelectedItem != null&&cbxMajorID6.SelectedValue!=null)
                majorID = Convert.ToInt32(cbxMajorID6.SelectedValue);
            DataView dv1 = SubClass.QuerySubClass(majorID);
            cbxSubID6.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxSubID6.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxSubID6.ItemsSource = dv1;
            cbxSubID6.Text="资产二级类别";
        }

        //选择资产二级类别
        private void CbxSubID6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int subID = -1;

            if (cbxSubID6.SelectedItem != null&&cbxSubID6.SelectedValue!=null)
                subID = Convert.ToInt32(cbxSubID6.SelectedValue);
            SubClass subClass = new SubClass();
            subClass.LoadData(subID);
            if (subClass.Exist)
            {
                cbxUnitsID6.Text = subClass.UnitsID.ToString();
                txtLimitedYear6.Text = subClass.UsefulLife.ToString();
                txtResidualValueRate6.Text = subClass.DepreciationRate.ToString();
            }

            //AssetsCoding.Text = SubID.SelectedItem.Value.ToString()+ "09" + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + System.DateTime.Now.Hour.ToString()+System.DateTime.Now.Minute.ToString();
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadData1(subID);
            if (fixedAsset.Exist)
            {
                int ShowFixedAssetsID = fixedAsset.FixedAssetsID + 1;
                txtAssetsCoding6.Text = cbxMajorID6.SelectedValue.ToString() + cbxSubID6.SelectedValue.ToString() + ShowFixedAssetsID.ToString();
            }
            else
            {
                txtAssetsCoding6.Text = cbxMajorID6.SelectedValue.ToString() + cbxSubID6.SelectedValue.ToString() + "10000";
            }
        }

        //选择事业部
        private void CbxDivisionID6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int divisionID = -1;

            if (cbxDivisionID6.SelectedItem != null&&cbxDivisionID6.SelectedValue!=null)
                divisionID = Convert.ToInt32(cbxDivisionID6.SelectedValue);

            DataView dv1 = Department.QueryDepartment(divisionID);
            cbxDepartmentID6.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxDepartmentID6.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxDepartmentID6.ItemsSource = dv1;

            cbxDepartmentID6.Text="请选择部门";
        }

        //选择部门
        private void CbxDepartmentID6_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int departmentID = -1;

            if (cbxDepartmentID6.SelectedItem != null&&cbxDepartmentID6.SelectedValue!=null)
                departmentID = Convert.ToInt32(cbxDepartmentID6.SelectedValue);

            DataView dv1 = UserList.QueryUserLists(departmentID);
            cbxUserAccount6.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxUserAccount6.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxUserAccount6.ItemsSource = dv1;
            cbxUserAccount6.Text="无";

            cbxUseUserAccount6.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxUseUserAccount6.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxUseUserAccount6.ItemsSource = dv1;
            cbxUseUserAccount6.Text="无";
        }

        //选择事业部
        private void CbxCDivisionID6_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            int cDivisionID = -1;

            if (cbxCDivisionID6.SelectedItem != null&&cbxCDivisionID6.SelectedValue!=null)
                cDivisionID = Convert.ToInt32(cbxCDivisionID6.SelectedValue);

            DataView dv1 = Department.QueryDepartment(cDivisionID);
            cbxCDepartmentID6.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxCDepartmentID6.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxCDepartmentID6.ItemsSource = dv1;

            cbxCDepartmentID6.Text="请选择部门";
        }

        //选择部门
        private void CbxCDepartmentID6_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            int cDepartmentID = -1;

            if (cbxCDepartmentID6.SelectedItem != null && cbxCDivisionID6.SelectedValue != null)
                cDepartmentID = Convert.ToInt32(cbxCDepartmentID6.SelectedValue);

            DataView dv1 = UserList.QueryUserLists(cDepartmentID);
            cbxCUserAccount6.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxCUserAccount6.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxCUserAccount6.ItemsSource = dv1;

            cbxCUserAccount6.Text="无";
        }

        //确认申请资产异动
        private void BtnEnterCheckChangeAsset_Click(object sender, RoutedEventArgs e)
        {
            ////1.验证用户输入
            //mtxtDepartment2.Visibility = Visibility.Collapsed;
            //mtxtTransferPeople2.Visibility = Visibility.Collapsed;
            //mtxtUserAccount2.Visibility = Visibility.Collapsed;
            ////验证必要项
            //if (cbxCDivisionID2.SelectedItem == null || cbxCDepartmentID2.SelectedItem == null)   //部门
            //{
            //    mtxtDepartment2.Visibility = Visibility.Visible;
            //    return;
            //}
            //if (string.IsNullOrEmpty(txtTransferPeople2.Text))  //转移人
            //{
            //    mtxtTransferPeople2.Visibility = Visibility.Visible;
            //    return;
            //}
            //if (cbxCUserAccount2.SelectedItem == null)   //保管人员
            //{
            //    mtxtUserAccount2.Visibility = Visibility.Visible;
            //    return;
            //}
            //更新资产表信息FixedAssetsID
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.FixedAssetsID = FixedAssetsID;

            Hashtable ht1 = new Hashtable();
            ht1.Add("DivisionID", SqlStringConstructor.GetQuotedString(cbxCDivisionID6.SelectedValue.ToString()));
            ht1.Add("DivisionName", SqlStringConstructor.GetQuotedString(cbxCDivisionID6.Text));
            ht1.Add("DepartmentID", SqlStringConstructor.GetQuotedString(cbxCDepartmentID6.SelectedValue.ToString()));
            ht1.Add("DepartmentName", SqlStringConstructor.GetQuotedString(cbxCDepartmentID6.Text));
            ht1.Add("UserAccount", SqlStringConstructor.GetQuotedString(cbxCUserAccount6.SelectedValue.ToString()));
            ht1.Add("Contactor", SqlStringConstructor.GetQuotedString(cbxCUserAccount6.Text));
            ht1.Add("UseSituationID", SqlStringConstructor.GetQuotedString(cbxUseSituationID6.SelectedIndex+1.ToString()));
            ht1.Add("ApplyStatus", 0);
            ht1.Add("AssetStatus", 1);
            ht1.Add("ApplyContactor", "null");
            ht1.Add("ApplyDate", "null");

            fixedAsset.Update(ht1);

            //更新异动表
            AssetsChange assetsChange = new AssetsChange();
            assetsChange.AssetsChangesID = Convert.ToInt32(AssetsChangesID);

            Hashtable ht = new Hashtable();
            ht.Add("CDivisionID", SqlStringConstructor.GetQuotedString(cbxCDivisionID6.SelectedValue.ToString()));
            ht.Add("CDivisionName", SqlStringConstructor.GetQuotedString(cbxCDivisionID6.Text));
            ht.Add("CDepartmentID", SqlStringConstructor.GetQuotedString(cbxCDepartmentID6.SelectedValue.ToString()));
            ht.Add("CDepartmentName", SqlStringConstructor.GetQuotedString(cbxCDepartmentID6.Text));
            ht.Add("CUserAccount", SqlStringConstructor.GetQuotedString(cbxCUserAccount6.SelectedValue.ToString()));
            ht.Add("CContactor", SqlStringConstructor.GetQuotedString(cbxCUserAccount6.Text));
            ht.Add("CStorageSites", SqlStringConstructor.GetQuotedString(txtCStorageSites6.Text));
            ht.Add("CChangesDate", SqlStringConstructor.GetQuotedString(txtCChangesDate6.Text));
            ht.Add("TransferPeople", SqlStringConstructor.GetQuotedString(txtTransferPeople6.Text));
            ht.Add("CBackup", SqlStringConstructor.GetQuotedString(mtxtCBackup6.Text));
            ht.Add("ApprovedPerson", SqlStringConstructor.GetQuotedString(txtApprovedPerson6.Text));
            ht.Add("ChangesStatus", 0);

            assetsChange.Update(ht);

            MessageBox.Show("审核异动资产成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            InitialData();
        }




        #endregion

        #region 审核维修资产
        private void BtCheckRepairItem_Click(object sender, RoutedEventArgs e)
        {
            FixedAssetsID = Convert.ToInt32(((System.Data.DataRowView)((System.Windows.FrameworkElement)sender).DataContext)["FixedAssetsID"]);
            tabpCheckRepairAsset.IsSelected = true;
            InitCheckRepairData();
        }
        #endregion

        #region 审核维修资产项
        /// <summary>
        /// 初始化审核维修页面数据
        /// </summary>
        private void InitCheckRepairData()
        {
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadData(FixedAssetsID);

            txtAssetsCoding7.Text = fixedAsset.AssetsCoding;
            txtAssetName7.Text = fixedAsset.AssetName;
            cbxMajorID7.SelectedValue = fixedAsset.MajorID;
            //资产小类
            int majorID = -1;
            if (fixedAsset.MajorID.ToString() != "")
                majorID = Convert.ToInt32(fixedAsset.MajorID.ToString());
            DataView dv1 = SubClass.QuerySubClass(majorID);
            cbxSubID7.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxSubID7.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxSubID7.ItemsSource = dv1;
            cbxSubID7.SelectedValue = fixedAsset.SubID;
            //规格型号
            txtSpecificationsModel7.Text = fixedAsset.SpecificationsModel;
            //品牌
            txtBrand7.Text = fixedAsset.Brand;
            //生产厂家
            txtManufacturer7.Text = fixedAsset.Manufacturer;
            //单位
            cbxUnitsID7.SelectedValue = fixedAsset.UnitsID;
            //使用情况
            cbxUseSituationID7.SelectedIndex = fixedAsset.UseSituationID + 1;
            //事业部
            cbxDivisionID7.SelectedValue = fixedAsset.DivisionID;
            //部门
            int divisionID = -1;
            if (cbxDivisionID7.SelectedItem != null && cbxDivisionID7.SelectedValue != null)
                divisionID = Convert.ToInt32(cbxDivisionID7.SelectedValue);
            DataView dv2 = Department.QueryDepartment(divisionID);
            cbxDepartmentID7.SelectedValuePath = dv2.Table.Columns[0].Caption;
            cbxDepartmentID7.DisplayMemberPath = dv2.Table.Columns[3].Caption;
            cbxDepartmentID7.ItemsSource = dv2;
            cbxDepartmentID7.SelectedValue = fixedAsset.DepartmentID;
            //保管人员
            int departmentID = -1;
            if (cbxDepartmentID7.SelectedItem != null && cbxDepartmentID7.SelectedValue != null)
                departmentID = Convert.ToInt32(cbxDepartmentID7.SelectedValue);
            DataView dv3 = UserList.QueryUserLists(departmentID);
            cbxUserAccount7.SelectedValuePath = dv3.Table.Columns[1].Caption;
            cbxUserAccount7.DisplayMemberPath = dv3.Table.Columns[5].Caption;
            cbxUserAccount7.ItemsSource = dv3;
            if (string.IsNullOrEmpty(fixedAsset.UserAccount))
            {
                cbxUserAccount7.Text = "无";

            }
            else
            {
                cbxUserAccount7.SelectedValue = fixedAsset.UserAccount;
            }
            //使用人员
            cbxUseUserAccount7.SelectedValuePath = dv3.Table.Columns[1].Caption;
            cbxUseUserAccount7.DisplayMemberPath = dv3.Table.Columns[5].Caption;
            cbxUseUserAccount7.ItemsSource = dv3;
            if (string.IsNullOrEmpty(fixedAsset.UseUserAccount))
            {
                cbxUseUserAccount7.Text = "无";
            }
            {
                cbxUseUserAccount7.SelectedValue = fixedAsset.UseUserAccount;
            }
            //增加方式
            cbxAddWaysID7.SelectedIndex = fixedAsset.AddWaysID - 1;
            //原值
            txtOriginalValue7.Text = fixedAsset.OriginalValue.ToString();
            //出厂日期
            if (fixedAsset.ExFactoryDate.ToString() != "0001-1-1 0:00:00")
            {
                dateExFactoryDate7.Text = fixedAsset.ExFactoryDate.ToShortDateString();
            }
            //购置日期
            if (fixedAsset.PurchaseDate.ToString() != "0001-1-1 0:00:00")
            {
                datePurchaseDate7.Text = fixedAsset.PurchaseDate.ToShortDateString();
            }
            //入账日期
            if (fixedAsset.RecordedDate.ToString() != "0001-1-1 0:00:00")
            {
                dateRecordedDate7.Text = fixedAsset.RecordedDate.ToShortDateString();
            }
            //折旧方法
            cbxMethodID7.SelectedValue = fixedAsset.MethodID;
            //有限年份
            txtLimitedYear7.Text = fixedAsset.LimitedYear.ToString();

            txtResidualValueRate7.Text = fixedAsset.ResidualValueRate.ToString();                //残值率
            double ShowResiduals = Math.Round(fixedAsset.OriginalValue * fixedAsset.ResidualValueRate, 2);
            mtxtResiduals7.Text = ShowResiduals.ToString();                                       //残值
            double ShowMonthDepreciation = Math.Round((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12), 2);
            mtxtMonthDepreciation7.Text = ShowMonthDepreciation.ToString();                       //本月折旧

            if (fixedAsset.RecordedDate.ToString() != "0001-1-1 0:00:00")
            {
                DateTime start1 = fixedAsset.RecordedDate;
                DateTime end1 = DateTime.Now;
                double ShowRemainderMonth = DateDiff("month", start1, end1);
                if (ShowRemainderMonth >= fixedAsset.LimitedYear * 12)
                {
                    mtxtRemainderMonth7.Text = "0";
                    mtxtNetValue7.Text = "0";

                    double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate), 2);
                    mtxtAccumulatedDepreciation7.Text = ShowAccumulatedDepreciation.ToString();              //累计折旧
                }
                else
                {
                    double ShowRemainderMonth1 = Math.Round((fixedAsset.LimitedYear * 12) - ShowRemainderMonth, 0);
                    mtxtRemainderMonth7.Text = ShowRemainderMonth1.ToString();       //剩余月份

                    double ShowNetValue = Math.Round(((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12)) * ShowRemainderMonth1, 2);
                    mtxtNetValue7.Text = ShowNetValue.ToString();                    //净值

                    double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate) - (((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12)) * ShowRemainderMonth1), 2);
                    mtxtAccumulatedDepreciation7.Text = ShowAccumulatedDepreciation.ToString();                  //累计折旧
                }
            }
            else
            {
                mtxtRemainderMonth7.Text = "0";
                mtxtNetValue7.Text = "0";

                double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate), 2);
                mtxtAccumulatedDepreciation7.Text = ShowAccumulatedDepreciation.ToString();                      //累计折旧
            }
            //存放地点
            txtStorageSites7.Text = fixedAsset.StorageSites;
            //资产备注
            txtAssetsBackup7.AddLine(fixedAsset.AssetsBackup);
            //是否低值易耗品
            if (fixedAsset.LowConsumables == 1)
            {
                chkLowConsumables7.IsChecked = true;
            }
            //ApplyStatus.Text = fixedAsset.ApplyStatus.ToString();
            //异动固定资产信息初始化
            mtxtAssetsCoding7.Text = fixedAsset.AssetsCoding;
            mtxtAssetName7.Text = fixedAsset.AssetName;

            int r_fixedAssetsID = fixedAsset.FixedAssetsID;
            RepairList repairList = new RepairList();
            repairList.LoadData1(r_fixedAssetsID);

            mtxtRepairContent7.AddLine(repairList.RepairContent);
            txtApprovedPerson7.Text = repairList.RepairUserAccount + "(" + repairList.RepairContactor + ")";

            //维修历史记录
            DataView dv6 = RepairList.QueryRepairHistoryRecord(FixedAssetsID);
            dtgRepairHistoryRecord7.ItemsSource = dv6;
        }

        //绑定下拉列表
        private void BindDrop6()
        {
            //将数据捆绑到下拉列表中
            DataView dv = MajorClass.QueryMajorClass();
            cbxMajorID7.SelectedValuePath = dv.Table.Columns[0].Caption;
            cbxMajorID7.DisplayMemberPath = dv.Table.Columns[2].Caption;
            cbxMajorID7.ItemsSource = dv;
            cbxMajorID7.Text = "资产一级类别";        //第一项中加入内容,重点是绑定后添加
            cbxSubID7.Text = "资产二级类别";          //第一项中加入内容,重点是绑定后添加

            //绑定单位
            DataView dv1 = UnitList.QueryUnits();
            cbxUnitsID7.SelectedValuePath = dv1.Table.Columns[0].Caption.ToString();
            cbxUnitsID7.DisplayMemberPath = dv1.Table.Columns[2].Caption.ToString();
            cbxUnitsID7.ItemsSource = dv1;

            //绑定增加方式
            DataView dv2 = AddWays.QueryAddWays();
            cbxAddWaysID7.SelectedValuePath = dv2.Table.Columns[0].Caption.ToString();
            cbxAddWaysID7.DisplayMemberPath = dv2.Table.Columns[2].Caption.ToString();
            cbxAddWaysID7.ItemsSource = dv2;

            //将数据捆绑到下拉列表中
            DataView dv3 = Division.QueryDivision();
            cbxDivisionID7.SelectedValuePath = dv3.Table.Columns[0].Caption;
            cbxDivisionID7.DisplayMemberPath = dv3.Table.Columns[2].Caption;
            cbxDivisionID7.ItemsSource = dv3;

            //折旧方法
            DataView dv4 = DepreciationMethod.QueryDepreciationMethod();
            cbxMethodID7.SelectedValuePath = dv4.Table.Columns[0].Caption;
            cbxMethodID7.DisplayMemberPath = dv4.Table.Columns[2].Caption;
            cbxMethodID7.ItemsSource = dv4;

            cbxDivisionID7.Text = "请选择事业部";        //第一项中加入内容,重点是绑定后添加
            cbxDepartmentID7.Text = "请选择部门";          //第一项中加入内容,重点是绑定后添加
            cbxUserAccount7.Text = "无";
            cbxUseUserAccount7.Text = "无";
        }

        //选择事业部
        private void CbxDivisionID7_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int cdivisionID = -1;
            //获取事业部ID
            if (cbxDivisionID7.SelectedItem != null && cbxDivisionID7.SelectedValue != null)
                cdivisionID = Convert.ToInt32(cbxDivisionID7.SelectedValue.ToString());
            //绑定部门数据到下拉列表
            DataView dv1 = Department.QueryDepartment(cdivisionID);
            cbxDepartmentID7.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxDepartmentID7.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxDepartmentID7.ItemsSource = dv1;
            cbxDepartmentID7.Text = "请选择部门";
        }

        //选择部门
        private void CbxDepartmentID7_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int cdepartmentID = -1;
            if (cbxDepartmentID7.SelectedItem != null && cbxDepartmentID7.SelectedValue != null)
                cdepartmentID = Convert.ToInt32(cbxDepartmentID7.SelectedValue.ToString());
            //加载保管人员
            DataView dv1 = UserList.QueryUserLists(cdepartmentID);
            cbxUserAccount7.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxUserAccount7.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxUserAccount7.ItemsSource = dv1;
            cbxUserAccount7.Text = "无";
            //加载使用人员
            cbxUseUserAccount7.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxUseUserAccount7.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxUseUserAccount7.ItemsSource = dv1;
            cbxUseUserAccount7.Text = "无";
        }



        //选择资产一级类别
        private void CbxMajorID7_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int majorID = -1;
            if (cbxMajorID7.SelectedItem != null && cbxMajorID7.SelectedValue != null)
                majorID = Convert.ToInt32(cbxMajorID7.SelectedValue);
            DataView dv1 = SubClass.QuerySubClass(majorID);
            cbxSubID7.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxSubID7.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxSubID7.ItemsSource = dv1;
            cbxSubID7.Text = "资产二级类别";
        }

        //选择资产二级类别
        private void CbxSubID7_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int subID = -1;

            if (cbxSubID7.SelectedItem != null && cbxSubID7.SelectedValue != null)
                subID = Convert.ToInt32(cbxSubID7.SelectedValue);
            SubClass subClass = new SubClass();
            subClass.LoadData(subID);
            if (subClass.Exist)
            {
                cbxUnitsID7.Text = subClass.UnitsID.ToString();
                txtLimitedYear7.Text = subClass.UsefulLife.ToString();
                txtResidualValueRate7.Text = subClass.DepreciationRate.ToString();
            }

            //AssetsCoding.Text = SubID.SelectedItem.Value.ToString()+ "09" + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + System.DateTime.Now.Hour.ToString()+System.DateTime.Now.Minute.ToString();
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadData1(subID);
            if (fixedAsset.Exist)
            {
                int ShowFixedAssetsID = fixedAsset.FixedAssetsID + 1;
                txtAssetsCoding7.Text = cbxMajorID7.SelectedValue.ToString() + cbxSubID7.SelectedValue.ToString() + ShowFixedAssetsID.ToString();
            }
            else
            {
                txtAssetsCoding7.Text = cbxMajorID7.SelectedValue.ToString() + cbxSubID7.SelectedValue.ToString() + "10000";
            }
        }
        
        //确认审核维修
        private void BtnEnterCheckRepairAsset_Click(object sender, RoutedEventArgs e)
        {
            //更新资产表信息FixedAssetsID
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.FixedAssetsID = FixedAssetsID;

            Hashtable ht1 = new Hashtable();
            ht1.Add("UseSituationID", 4);
            ht1.Add("ApplyStatus", 0);
            ht1.Add("AssetStatus", 2);
            ht1.Add("ApplyContactor", "null");
            ht1.Add("ApplyDate", "null");

            fixedAsset.Update(ht1);

            //更新维修信息表

            MessageBox.Show("审核维修资产成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            InitialData();
        }

        #endregion

        #region 审核报废资产
        //审核报废资产
        private void BtCheckScrappedItem_Click(object sender, RoutedEventArgs e)
        {
            DataView dvlist = FixedAsset.QueryFixedAssets(3);           //报废固定资产
            dtgCheckScrappedAssets.ItemsSource = dvlist;
            tabpCheckScrappedAsset.IsSelected = true;
        }
        #endregion

        #region 审核报废资产项
        /// <summary>
        /// 报废资产ID
        /// </summary>
        public int AssetsScrappedID { get; set; }

        /// <summary>
        /// 初始化审核报废页面数据
        /// </summary>
        private void InitCheckScrappedData()
        {
            BindDrop8();
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadData(FixedAssetsID);

            txtAssetsCoding8.Text = fixedAsset.AssetsCoding;
            txtAssetName8.Text = fixedAsset.AssetName;
            cbxMajorID8.SelectedValue = fixedAsset.MajorID;
            //资产小类
            int majorID = -1;
            if (fixedAsset.MajorID.ToString() != "")
                majorID = Convert.ToInt32(fixedAsset.MajorID.ToString());
            DataView dv1 = SubClass.QuerySubClass(majorID);
            cbxSubID8.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxSubID8.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxSubID8.ItemsSource = dv1;
            cbxSubID8.SelectedValue = fixedAsset.SubID;
            //规格型号
            txtSpecificationsModel8.Text = fixedAsset.SpecificationsModel;
            //品牌
            txtBrand8.Text = fixedAsset.Brand;
            //生产厂家
            txtManufacturer8.Text = fixedAsset.Manufacturer;
            //单位
            cbxUnitsID8.SelectedValue = fixedAsset.UnitsID;
            //使用情况
            cbxUseSituationID8.SelectedIndex = fixedAsset.UseSituationID + 1;
            //事业部
            cbxDivisionID8.SelectedValue = fixedAsset.DivisionID;
            //部门
            int divisionID = -1;
            if (cbxDivisionID8.SelectedItem != null && cbxDivisionID8.SelectedValue != null)
                divisionID = Convert.ToInt32(cbxDivisionID8.SelectedValue);
            DataView dv2 = Department.QueryDepartment(divisionID);
            cbxDepartmentID8.SelectedValuePath = dv2.Table.Columns[0].Caption;
            cbxDepartmentID8.DisplayMemberPath = dv2.Table.Columns[3].Caption;
            cbxDepartmentID8.ItemsSource = dv2;
            cbxDepartmentID8.SelectedValue = fixedAsset.DepartmentID;
            //保管人员
            int departmentID = -1;
            if (cbxDepartmentID8.SelectedItem != null && cbxDepartmentID8.SelectedValue != null)
                departmentID = Convert.ToInt32(cbxDepartmentID8.SelectedValue);
            DataView dv3 = UserList.QueryUserLists(departmentID);
            cbxUserAccount8.SelectedValuePath = dv3.Table.Columns[1].Caption;
            cbxUserAccount8.DisplayMemberPath = dv3.Table.Columns[5].Caption;
            cbxUserAccount8.ItemsSource = dv3;
            if (string.IsNullOrEmpty(fixedAsset.UserAccount))
            {
                cbxUserAccount8.Text = "无";
            }
            else
            {
                cbxUserAccount8.SelectedValue = fixedAsset.UserAccount;
            }
            //使用人员
            cbxUseUserAccount8.SelectedValuePath = dv3.Table.Columns[1].Caption;
            cbxUseUserAccount8.DisplayMemberPath = dv3.Table.Columns[5].Caption;
            cbxUseUserAccount8.ItemsSource = dv3;
            if (string.IsNullOrEmpty(fixedAsset.UseUserAccount))
            {
                cbxUseUserAccount8.Text = "无";
            }
            {
                cbxUseUserAccount8.SelectedValue = fixedAsset.UseUserAccount;
            }
            //增加方式
            cbxAddWaysID8.SelectedIndex = fixedAsset.AddWaysID - 1;
            //原值
            txtOriginalValue8.Text = fixedAsset.OriginalValue.ToString();
            //出厂日期
            if (fixedAsset.ExFactoryDate.ToString() != "0001-1-1 0:00:00")
            {
                dateExFactoryDate8.Text = fixedAsset.ExFactoryDate.ToShortDateString();
            }
            //购置日期
            if (fixedAsset.PurchaseDate.ToString() != "0001-1-1 0:00:00")
            {
                datePurchaseDate8.Text = fixedAsset.PurchaseDate.ToShortDateString();
            }
            //入账日期
            if (fixedAsset.RecordedDate.ToString() != "0001-1-1 0:00:00")
            {
                dateRecordedDate8.Text = fixedAsset.RecordedDate.ToShortDateString();
            }
            //折旧方法
            cbxMethodID8.SelectedValue = fixedAsset.MethodID;
            //有限年份
            txtLimitedYear8.Text = fixedAsset.LimitedYear.ToString();

            txtResidualValueRate8.Text = fixedAsset.ResidualValueRate.ToString();                //残值率
            double ShowResiduals = Math.Round(fixedAsset.OriginalValue * fixedAsset.ResidualValueRate, 2);
            mtxtResiduals8.Text = ShowResiduals.ToString();                                       //残值
            double ShowMonthDepreciation = Math.Round((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12), 2);
            mtxtMonthDepreciation8.Text = ShowMonthDepreciation.ToString();                       //本月折旧

            if (fixedAsset.RecordedDate.ToString() != "0001-1-1 0:00:00")
            {
                DateTime start1 = fixedAsset.RecordedDate;
                DateTime end1 = DateTime.Now;
                double ShowRemainderMonth = DateDiff("month", start1, end1);
                if (ShowRemainderMonth >= fixedAsset.LimitedYear * 12)
                {
                    mtxtRemainderMonth8.Text = "0";
                    mtxtNetValue8.Text = "0";

                    double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate), 2);
                    mtxtAccumulatedDepreciation8.Text = ShowAccumulatedDepreciation.ToString();              //累计折旧
                }
                else
                {
                    double ShowRemainderMonth1 = Math.Round((fixedAsset.LimitedYear * 12) - ShowRemainderMonth, 0);
                    mtxtRemainderMonth8.Text = ShowRemainderMonth1.ToString();       //剩余月份

                    double ShowNetValue = Math.Round(((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12)) * ShowRemainderMonth1, 2);
                    mtxtNetValue8.Text = ShowNetValue.ToString();                    //净值

                    double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate) - (((fixedAsset.OriginalValue * (1 - fixedAsset.ResidualValueRate)) / (fixedAsset.LimitedYear * 12)) * ShowRemainderMonth1), 2);
                    mtxtAccumulatedDepreciation8.Text = ShowAccumulatedDepreciation.ToString();                  //累计折旧
                }
            }
            else
            {
                mtxtRemainderMonth8.Text = "0";
                mtxtNetValue8.Text = "0";

                double ShowAccumulatedDepreciation = Math.Round(fixedAsset.OriginalValue - (fixedAsset.OriginalValue * fixedAsset.ResidualValueRate), 2);
                mtxtAccumulatedDepreciation8.Text = ShowAccumulatedDepreciation.ToString();                      //累计折旧
            }
            //存放地点
            txtStorageSites8.Text = fixedAsset.StorageSites;
            //资产备注
            txtAssetsBackup8.AddLine(fixedAsset.AssetsBackup);
            //是否低值易耗品
            if (fixedAsset.LowConsumables == 1)
            {
                chkLowConsumables8.IsChecked = true;
            }
            //ApplyStatus.Text = fixedAsset.ApplyStatus.ToString();
            //报废固定资产信息初始化
            mtxtAssetsCoding8.Text = fixedAsset.AssetsCoding;
            mtxtAssetName8.Text = fixedAsset.AssetName;

            int s_fixedAssetsID = fixedAsset.FixedAssetsID;
            AssetsScrapped assetsScrapped = new AssetsScrapped();
            assetsScrapped.LoadData1(s_fixedAssetsID);

            txtApplicant8.Text = assetsScrapped.Applicant;
            cbxReduceWaysID8.SelectedIndex = assetsScrapped.ReduceWaysID+1;
            mtxtScrappedReason8.AddLine(assetsScrapped.ScrappedReason);
            if (assetsScrapped.ReduceDate.ToString() != "0001-1-1 0:00:00")
            {
                dateReduceDate8.Text = assetsScrapped.ReduceDate.ToShortDateString();
            }
            AssetsScrappedID = assetsScrapped.AssetsScrappedID;
        }

        //绑定下拉列表
        private void BindDrop8()
        {
            //将数据捆绑到下拉列表中
            DataView dv = MajorClass.QueryMajorClass();
            cbxMajorID8.SelectedValuePath = dv.Table.Columns[0].Caption;
            cbxMajorID8.DisplayMemberPath = dv.Table.Columns[2].Caption;
            cbxMajorID8.ItemsSource = dv;
            cbxMajorID8.Text = "资产一级类别";        //第一项中加入内容,重点是绑定后添加
            cbxSubID8.Text = "资产二级类别";          //第一项中加入内容,重点是绑定后添加

            //绑定单位
            DataView dv1 = UnitList.QueryUnits();
            cbxUnitsID8.SelectedValuePath = dv1.Table.Columns[0].Caption.ToString();
            cbxUnitsID8.DisplayMemberPath = dv1.Table.Columns[2].Caption.ToString();
            cbxUnitsID8.ItemsSource = dv1;

            //绑定增加方式
            DataView dv2 = AddWays.QueryAddWays();
            cbxAddWaysID8.SelectedValuePath = dv2.Table.Columns[0].Caption.ToString();
            cbxAddWaysID8.DisplayMemberPath = dv2.Table.Columns[2].Caption.ToString();
            cbxAddWaysID8.ItemsSource = dv2;

            //将数据捆绑到下拉列表中
            DataView dv3 = Division.QueryDivision();
            cbxDivisionID8.SelectedValuePath = dv3.Table.Columns[0].Caption;
            cbxDivisionID8.DisplayMemberPath = dv3.Table.Columns[2].Caption;
            cbxDivisionID8.ItemsSource = dv3;

            //折旧方法
            DataView dv4 = DepreciationMethod.QueryDepreciationMethod();
            cbxMethodID8.SelectedValuePath = dv4.Table.Columns[0].Caption;
            cbxMethodID8.DisplayMemberPath = dv4.Table.Columns[2].Caption;
            cbxMethodID8.ItemsSource = dv4;

            cbxDivisionID8.Text = "请选择事业部";        //第一项中加入内容,重点是绑定后添加
            cbxDepartmentID8.Text = "请选择部门";          //第一项中加入内容,重点是绑定后添加
            cbxUserAccount8.Text = "无";
            cbxUseUserAccount8.Text = "无";
        }

        //选择事业部
        private void CbxDivisionID8_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int cdivisionID = -1;
            //获取事业部ID
            if (cbxDivisionID8.SelectedItem != null && cbxDivisionID8.SelectedValue != null)
                cdivisionID = Convert.ToInt32(cbxDivisionID8.SelectedValue.ToString());
            //绑定部门数据到下拉列表
            DataView dv1 = Department.QueryDepartment(cdivisionID);
            cbxDepartmentID8.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxDepartmentID8.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxDepartmentID8.ItemsSource = dv1;
            cbxDepartmentID8.Text = "请选择部门";
        }

        //选择部门
        private void CbxDepartmentID8_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int cdepartmentID = -1;
            if (cbxDepartmentID8.SelectedItem != null && cbxDepartmentID8.SelectedValue != null)
                cdepartmentID = Convert.ToInt32(cbxDepartmentID8.SelectedValue.ToString());
            //加载保管人员
            DataView dv1 = UserList.QueryUserLists(cdepartmentID);
            cbxUserAccount8.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxUserAccount8.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxUserAccount8.ItemsSource = dv1;
            cbxUserAccount8.Text = "无";
            //加载使用人员
            cbxUseUserAccount8.SelectedValuePath = dv1.Table.Columns[1].Caption;
            cbxUseUserAccount8.DisplayMemberPath = dv1.Table.Columns[5].Caption;
            cbxUseUserAccount8.ItemsSource = dv1;
            cbxUseUserAccount8.Text = "无";
        }



        //选择资产一级类别
        private void CbxMajorID8_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int majorID = -1;
            if (cbxMajorID8.SelectedItem != null && cbxMajorID8.SelectedValue != null)
                majorID = Convert.ToInt32(cbxMajorID8.SelectedValue);
            DataView dv1 = SubClass.QuerySubClass(majorID);
            cbxSubID8.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxSubID8.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxSubID8.ItemsSource = dv1;
            cbxSubID8.Text = "资产二级类别";
        }

        //选择资产二级类别
        private void CbxSubID8_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int subID = -1;

            if (cbxSubID8.SelectedItem != null && cbxSubID8.SelectedValue != null)
                subID = Convert.ToInt32(cbxSubID8.SelectedValue);
            SubClass subClass = new SubClass();
            subClass.LoadData(subID);
            if (subClass.Exist)
            {
                cbxUnitsID8.Text = subClass.UnitsID.ToString();
                txtLimitedYear8.Text = subClass.UsefulLife.ToString();
                txtResidualValueRate8.Text = subClass.DepreciationRate.ToString();
            }

            //AssetsCoding.Text = SubID.SelectedItem.Value.ToString()+ "09" + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + System.DateTime.Now.Hour.ToString()+System.DateTime.Now.Minute.ToString();
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadData1(subID);
            if (fixedAsset.Exist)
            {
                int ShowFixedAssetsID = fixedAsset.FixedAssetsID + 1;
                txtAssetsCoding8.Text = cbxMajorID8.SelectedValue.ToString() + cbxSubID8.SelectedValue.ToString() + ShowFixedAssetsID.ToString();
            }
            else
            {
                txtAssetsCoding8.Text = cbxMajorID8.SelectedValue.ToString() + cbxSubID8.SelectedValue.ToString() + "10000";
            }
        }

        //确认审核报废
        private void BtnEnterCheckScrappedAsset_Click(object sender, RoutedEventArgs e)
        {
            //更新资产表信息FixedAssetsID
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.FixedAssetsID = FixedAssetsID;

            Hashtable ht1 = new Hashtable();
            ht1.Add("UseSituationID", 2);
            ht1.Add("ApplyStatus", 0);
            ht1.Add("AssetStatus", 3);
            ht1.Add("ApplyContactor", "null");
            ht1.Add("ApplyDate", "null");

            fixedAsset.Update(ht1);

            //更新报废表
            AssetsScrapped assetsScrapped = new AssetsScrapped();
            assetsScrapped.AssetsScrappedID = Convert.ToInt32(AssetsScrappedID);

            Hashtable ht = new Hashtable();
            ht.Add("Applicant", SqlStringConstructor.GetQuotedString(txtApplicant8.Text));
            ht.Add("ReduceWaysID", SqlStringConstructor.GetQuotedString(cbxReduceWaysID8.SelectedIndex+1.ToString()));
            ht.Add("ReduceWays", SqlStringConstructor.GetQuotedString(cbxReduceWaysID8.Text));
            ht.Add("ScrappedReason", SqlStringConstructor.GetQuotedString(mtxtScrappedReason8.Text));
            //ht.Add("ReduceDate", SqlStringConstructor.GetQuotedString(ReduceDate.Value.ToString()));
            if (dateReduceDate8.SelectedDate == null)
            {
                ht.Add("ReduceDate", "Null");
            }
            else
            {
                ht.Add("ReduceDate", SqlStringConstructor.GetQuotedString(dateReduceDate8.SelectedDate.ToString()));
            }
            ht.Add("ApprovedPerson", SqlStringConstructor.GetQuotedString(txtApprovedPerson8.Text));
            ht.Add("ScrappedStatus", 0);

            assetsScrapped.Update(ht);

            MessageBox.Show("审核维修资产成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            InitialData();
        }
        #endregion

        #region 管理事业部
        void InitDivisionData()
        {
            DataView dvlist = Division.QueryDivision();
            dtgDivision.ItemsSource = dvlist;
        }

        //确认添加
        private void BtEnterAdd_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(txtDivisionName.Text.Trim()))
            {
                MessageBox.Show("请输入事业部名称！", "提示", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            Division division = new Division();
            division.LoadData1(txtDivisionName.Text);
            if (division.Exist)
            {
                MessageBox.Show("您输入的事业部名称已经存在！", "提示", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            else
            {
                Hashtable ht = new Hashtable();
                ht.Add("DivisionName", SqlStringConstructor.GetQuotedString(txtDivisionName.Text));

                Division division1 = new Division();
                division1.Add(ht);

                MessageBox.Show("添加事业部成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 当前操作事业部ID
        /// </summary>
        public int DivisionID { get; set; }
        //编辑
        private void BtEdit_Click(object sender, RoutedEventArgs e)
        {
            DivisionID = Convert.ToInt32(((System.Data.DataRowView)((System.Windows.FrameworkElement)sender).DataContext)["DivisionID"]);
            InitEditDivisionData();
            tabpEditDivisionName.IsSelected = true;
        }

        //删除
        private void BtDelete_Click(object sender, RoutedEventArgs e)
        {
            DivisionID = Convert.ToInt32(((System.Data.DataRowView)((System.Windows.FrameworkElement)sender).DataContext)["DivisionID"]);
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadDataDivisionID(DivisionID);
            if (fixedAsset.Exist)
            {
                MessageBox.Show("该事业部下存在固定资产，不能删除！", "提示", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            else
            {
                Division division = new Division();
                division.LoadData(DivisionID);
                division.Delete();
                MessageBox.Show("删除事业部成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        #region 修改事业部
        /// <summary>
        /// 初始化页面数据
        /// </summary>
        private void InitEditDivisionData()
        {
            int divisionID = DivisionID;

            Division divison = new Division();
            divison.LoadData(divisionID);

            txtDivisionName9.Text = divison.DivisionName;
        }

        //确认修改
        private void BtnEnterEditDivisionName_Click(object sender, RoutedEventArgs e)
        {
            //验证文本
            mtxtDivisionName9.Visibility = Visibility.Collapsed;
            if(string.IsNullOrEmpty(txtDivisionName9.Text))
            {
                mtxtDivisionName9.Visibility = Visibility.Visible;
            }

            Division divison = new Division();
            divison.DivisionID = DivisionID;
            Hashtable ht = new Hashtable();
            ht.Add("DivisionName", SqlStringConstructor.GetQuotedString(txtDivisionName9.Text));
            divison.Update(ht);

            //更新固定资产表里面的信息
            Hashtable ht1 = new Hashtable();
            string where = "";

            ht1.Add("DivisionName", SqlStringConstructor.GetQuotedString(txtDivisionName9.Text));                  //固定资产查询

            where = " Where DivisionID=" + DivisionID;
            FixedAsset.Update(ht1, where);

            MessageBox.Show("修改事业部成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region 管理部门
        /// <summary>
        /// 初始化管理部门数据
        /// </summary>
        void InitDepartmentData()
        {
            DataView dvlist = Department.QueryDepartment();
            dtgDivision.ItemsSource = dvlist;
            //绑定数据事业部名称
            DataView dv = Division.QueryDivision();
            cbxDepartment.SelectedValuePath = dv.Table.Columns[0].Caption.ToString();
            cbxDepartment.DisplayMemberPath = dv.Table.Columns[2].Caption.ToString();
            cbxDepartment.ItemsSource = dv;
            cbxDepartment.Text = "请选择事业部";
        }

        //添加事业部
        private void BtEnterAddDepartment_Click(object sender, RoutedEventArgs e)
        {
            if (cbxDepartment.SelectedValue == null && cbxDepartment.SelectedItem == null)
            {
                MessageBox.Show("请选择事业部！", "提示", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            if(string.IsNullOrEmpty(txtDepartmentName.Text))
            {
                MessageBox.Show("请输入部门名称！", "提示", MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }
            //获取用户在页面上的输入
            int divisionID = Convert.ToInt32(cbxDepartment.SelectedValue);
            string departmentName = txtDepartmentName.Text;

            Department department = new Department();
            department.LoadData2(divisionID, departmentName);

            if (department.Exist)
            {
                MessageBox.Show("对不起，您输入的部门名称已经存在！", "提示", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            else
            {
                Hashtable ht = new Hashtable();
                ht.Add("DivisionID", SqlStringConstructor.GetQuotedString(cbxDepartment.SelectedValue.ToString()));
                ht.Add("DepartmentName", SqlStringConstructor.GetQuotedString(txtDepartmentName.Text));

                Department department1 = new Department();
                department1.Add(ht);

                MessageBox.Show("添加部门成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        /// <summary>
        /// 当前操作部门
        /// </summary>
        public int DepartmentID { get; set; }
        //编辑部门
        private void BtEditDepartment_Click(object sender, RoutedEventArgs e)
        {
            DepartmentID = Convert.ToInt32(((System.Data.DataRowView)((System.Windows.FrameworkElement)sender).DataContext)["DepartmentID"]);
            InitEditDepartmentData();
            tabpEditDepartment.IsSelected = true;
        }

        //删除部门
        private void BtDeleteDepartment_Click(object sender, RoutedEventArgs e)
        {
            int departmentID = DepartmentID;
            FixedAsset fixedAsset = new FixedAsset();
            fixedAsset.LoadDataDepartmentID(departmentID);
            if (fixedAsset.Exist)
            {
                MessageBox.Show("发生错误，该部门下存在固定资产，无法删除！", "提示", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            else
            {
                Department department = new Department();
                department.LoadData(departmentID);
                department.Delete();
                MessageBox.Show("删除部门成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion


        //private void InitEditDepartmentData()
        //{
        //    //绑定数据事业部名称
        //    DataView dv = Division.QueryDivision();
        //    DivisionID.SelectedValuePath = dv.Table.Columns[0].Caption.ToString();
        //    DivisionID.DisplayMemberPath = dv.Table.Columns[2].Caption.ToString();
        //    DivisionID.DataSource = dv;
        //    DivisionID.DataBind();


        //    int departmentID = Convert.ToInt32(Request.QueryString["DepartmentID"]);

        //    Department department = new Department();
        //    department.LoadData(departmentID);

        //    DivisionID.Text = department.DivisionID.ToString();
        //    DepartmentName.Text = department.DepartmentName;
        //}

    }

    #region dtg数据转换器
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
    #endregion
}