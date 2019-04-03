using Arthas.Controls.Metro;
using Asset.BusinessLogicLayer;
using Asset.CommonComponent;
using Asset.DataAccessHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
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

namespace Asset
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : MetroWindow
    {
        //实例化一个配置文件
        IniFiles ini = new IniFiles(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"\config.ini");
        public LoginWindow()
        {
            InitializeComponent();
            //设置密码框文本装饰
            txtPwd.TextDecorations = new TextDecorationCollection(new TextDecoration[] {
                new TextDecoration() {
                     Location= TextDecorationLocation.Strikethrough,
                      Pen= new Pen(Brushes.Black, 10f) {
                          DashCap =  PenLineCap.Round,
                           StartLineCap= PenLineCap.Round,
                            EndLineCap= PenLineCap.Round,
                             DashStyle= new DashStyle(new double[] {0.0,1.2 }, 0.6f)
                      }
                }
            });
        }

        //登录跳转
        private void LinkLogin_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            tabiLogin.IsSelected = true;
        }

        //注册跳转
        private void LinkRegistered_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            tabiRegistered.IsSelected = true;
        }

        //登录
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            txbError.Visibility = Visibility.Hidden;
            //获取用户在页面上的输入
            string userAccount = txtAcc.Text;          //用户登陆名
            string userPassword = txtPwd.Text;        //用户登陆密码

            UserList userlist = new UserList();
            userlist.LoadData(userAccount);
            if (userlist.Exist)
            {
                if (userlist.UserPassword == userPassword)          //如果密码正确，转入管理员页面
                {
                    //保存用户信息到配置文件
                    ini.IniWriteValue("登录详细", "UserListID", userlist.UserListID.ToString());
                    ini.IniWriteValue("登录详细", "UserAccount", userlist.UserAccount.ToString());
                    ini.IniWriteValue("登录详细", "DivisionID", userlist.DivisionID.ToString());
                    ini.IniWriteValue("登录详细", "DepartmentID", userlist.DepartmentID.ToString());
                    ini.IniWriteValue("登录详细", "DepartmentName", userlist.DepartmentName.ToString());
                    ini.IniWriteValue("登录详细", "Contactor", userlist.Contactor.ToString());
                    ini.IniWriteValue("登录详细", "Tel", userlist.Tel.ToString());
                    ini.IniWriteValue("登录详细", "Address", userlist.Address.ToString());
                    ini.IniWriteValue("登录详细", "UserLevel", userlist.UserLevel.ToString());
                    //判断当前用权值
                    switch (userlist.UserLevel.ToString())
                    {
                        case "0":
                            //"管理员";
                            break;
                        case "1":
                            //"技术员";
                            break;
                        case "2":
                            //"用户";
                            break;
                        default:
                            break;
                    }
                    //登录成功
                    this.DialogResult = true;
                    this.Close();
                }
                else                                                //如果密码错误，给出提示，光标停留在密码框中
                {
                    //登录失败密码错误
                    txbError.Text = "密码错误，请重新输入密码！";
                    txbError.Visibility = Visibility.Visible;
                    txtPwd.Focus();
                }
            }
            else
            {
                //登录失败输入的用户名不存在
                txbError.Text = "您输入的用户名不存在！";
                txbError.Visibility = Visibility.Visible;
                txtAcc.Focus();
            }
        }

        //绑定下拉列表数据
        private void BindDrop()
        {
            //将数据捆绑到下拉列表中
            DataView dv = Division.QueryDivision();
            cbxDivisionID.SelectedValuePath = dv.Table.Columns[0].Caption;
            cbxDivisionID.DisplayMemberPath = dv.Table.Columns[2].Caption;
            cbxDivisionID.ItemsSource = dv;

            cbxDivisionID.Text="请选择事业部";        //第一项中加入内容,重点是绑定后添加
            cbxDepartmentID.Text="请选择部门名称";          //第一项中加入内容,重点是绑定后添加
        }

        //选择事业部
        private void CbxDivisionID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int divisionID = -1;
            if (cbxDivisionID.SelectedItem != null && cbxDivisionID.SelectedValue != null)
                divisionID = Convert.ToInt32(cbxDivisionID.SelectedValue.ToString());
            DataView dv1 = Department.QueryDepartment(divisionID);
            cbxDepartmentID.SelectedValuePath = dv1.Table.Columns[0].Caption;
            cbxDepartmentID.DisplayMemberPath = dv1.Table.Columns[3].Caption;
            cbxDepartmentID.ItemsSource = dv1;
        }

        //注册
        private void BtnRegistered_Click(object sender, RoutedEventArgs e)
        {
            txbErr.Visibility = Visibility.Collapsed;
            if (cbxDivisionID.SelectedItem ==null || cbxDivisionID.SelectedValue == null|| cbxDepartmentID.SelectedItem == null || cbxDepartmentID.SelectedValue == null)
            {
                txbErr.Text = "请选择部门";
                txbErr.Visibility = Visibility.Visible;
                return;
            }
            if(string.IsNullOrEmpty(mtbUserAccount.Text))
            {
                txbErr.Text = "请输入用户名";
                txbErr.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(mtbUserPassword.Text))
            {
                txbErr.Text = "请输入密码";
                txbErr.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(mtbJobPosition.Text))
            {
                txbErr.Text = "请输入职称";
                txbErr.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(mtbContactor.Text))
            {
                txbErr.Text = "请输入真实姓名";
                txbErr.Visibility = Visibility.Visible;
                return;
            }
            if (string.IsNullOrEmpty(mtbTel.Text))
            {
                txbErr.Text = "请输入联系电话";
                txbErr.Visibility = Visibility.Visible;
                return;
            }

            UserList userlist = new UserList();
            userlist.LoadData(mtbUserAccount.Text);

            if (userlist.Exist)
            {
                txbErr.Text = "您登记的用户名已经存在!";
                txbErr.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                Hashtable ht = new Hashtable();
                ht.Add("UserAccount", SqlStringConstructor.GetQuotedString(mtbUserAccount.Text));
                ht.Add("UserPassword", SqlStringConstructor.GetQuotedString(mtbUserPassword.Text));
                ht.Add("DivisionID", SqlStringConstructor.GetQuotedString(cbxDivisionID.SelectedValue.ToString()));
                ht.Add("DepartmentID", SqlStringConstructor.GetQuotedString(cbxDepartmentID.SelectedValue.ToString()));
                ht.Add("Contactor", SqlStringConstructor.GetQuotedString(mtbContactor.Text));
                ht.Add("JobPosition", SqlStringConstructor.GetQuotedString(mtbJobPosition.Text));
                ht.Add("Tel", SqlStringConstructor.GetQuotedString(mtbTel.Text));

                UserList adduserlist = new UserList();
                adduserlist.Add(ht);
                MessageBox.Show("用户登记成功！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                tabiLogin.IsSelected = true;
            }
        }
    }
}
