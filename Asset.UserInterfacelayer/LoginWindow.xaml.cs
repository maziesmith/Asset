using Arthas.Controls.Metro;
using Asset.BusinessLogicLayer;
using Asset.CommonComponent;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            //使用的框架只能在这里设置大小，xaml中设置大小无效
            this.Width = 350;
            this.Height = 500;
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

    }
}
