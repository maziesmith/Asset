﻿using Arthas.Controls.Metro;
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
                InitialData();
            }
        }

        /// <summary>
        /// 数据初始化
        /// </summary>
        private void InitialData()
        {
            //1.验证是否登录

            //2.初始化数据
            DataView dvlist = FixedAsset.Query_V_FixedAssets();
            //绑定资产数据
            dtgShow.ItemsSource = dvlist;
            //绑定下拉框数据
            BindDrop();//资产管理页事业部和部门
            BindDrop2();//新增页下拉框

            //打印列表
            string userAccount = ini.IniReadValue("登录详细", "UserAccount"); 
            DataView dvlist2 = PrintList.Query_V_PrintList(userAccount);
            dtgPrintList.ItemsSource = dvlist2;

            //3.界面元素初始化
            //导出excel按钮置灰
            btExport.IsEnabled = false;
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
        private void BtChange_Click(object sender, RoutedEventArgs e)
        {
            FixedAssetsID = Convert.ToInt32(((System.Data.DataRowView)((System.Windows.FrameworkElement)sender).DataContext)["FixedAssetsID"]);
            InitChangeData();
            tabpChangeAsset.IsSelected = true;
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
            mtxtType.Visibility = Visibility.Hidden;
            mtxtName.Visibility = Visibility.Hidden;
            mtxtDepartment.Visibility = Visibility.Hidden;
            mtxtLimitedYear.Visibility = Visibility.Hidden;
            mtxtNum.Visibility = Visibility.Hidden;
            mtxtOriginalValue.Visibility = Visibility.Hidden;
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
                    MessageBox.Show("您输入的固定资产编码已经存在！","失败",MessageBoxButton.OK, MessageBoxImage.Warning);
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
        /// 当前修改资产的ID
        /// </summary>
        int FixedAssetsID { get; set; }

        /// <summary>
        /// 初始化修改资产页面数据
        /// </summary>
        private void InitChangeData()
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