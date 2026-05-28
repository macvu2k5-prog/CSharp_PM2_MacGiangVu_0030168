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
    public partial class FormMain : Form
    {
        private Form currentFormChild;
        public FormMain()
        {
            InitializeComponent();

            OpenChildForm(new FormSinhVien()); 
        }
        private void OpenChildForm(Form childForm)
        {
            // Nếu đang có form khác mở thì đóng nó lại cho đỡ nặng máy
            if (currentFormChild != null)
            {
                currentFormChild.Close();
            }

            currentFormChild = childForm;

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            pn1Content.Controls.Clear();
            pn1Content.Controls.Add(childForm);
            pn1Content.Tag = childForm;

            childForm.BringToFront();
            childForm.Show();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void menuSinhVien_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormSinhVien());
        }

        private void menuLopHoc_Click(object sender, EventArgs e)
        {
            OpenChildForm(new FormLopHoc());
        }

        private void menuDangXuat_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
