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
    public partial class FormDSSV : Form
    {
        databaseDataContext db = new databaseDataContext();

        private string maLop;
        private string tenLop;

        public FormDSSV(string maLop, string tenLop)
        {
            InitializeComponent();

            this.maLop = maLop;
            this.tenLop = tenLop;

            lblTieuDe.Text = "Danh sách sinh viên lớp " + maLop + " - " + tenLop;

            HienThiDanhSachSinhVien();
        }

        private void HienThiDanhSachSinhVien()
        {
            dgvDSSinhVien.Rows.Clear();

            var dsSinhVien = db.tbl_sinhviens
                .Where(sv => sv.MaLop == maLop)
                .OrderBy(sv => sv.MaSV)
                .ToList();

            foreach (var sv in dsSinhVien)
            {
                dgvDSSinhVien.Rows.Add(
                    sv.MaSV,
                    sv.HoTen,
                    sv.GioiTinh,
                    sv.NgaySinh.HasValue ? sv.NgaySinh.Value.ToString("dd/MM/yyyy") : ""
                );
            }

            if (dsSinhVien.Count == 0)
            {
                MessageBox.Show(
                    "Lớp này chưa có sinh viên nào!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

      
        private void btnDong_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}