using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LoginApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string userEmail = txtUsername.Text.Trim();
            string userPass = txtPassword.Text.Trim();

           
            string correctEmail = "123";
            string correctMSSV = "12";

            
            if (userEmail == correctEmail && userPass == correctMSSV)
            {
                
                MessageBox.Show("Đăng nhập thành công", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                FormMain frmMain = new FormMain();
                this.Hide();           // Ẩn màn hình đăng nhập
                frmMain.ShowDialog();  // Mở màn hình chính lên (chờ đến khi nó đóng)

                // Khi FormMain đóng (tức là ấn Đăng xuất), nó sẽ chạy tiếp dòng lệnh dưới
                this.Show();           // Hiện lại màn hình đăng nhập
                txtPassword.Text = ""; // Xóa trắng ô pass cho an toàn
            }
            else
            {
               
                MessageBox.Show("Đăng nhập thất bại", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
