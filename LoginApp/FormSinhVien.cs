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
    public partial class FormSinhVien : Form
    {
        databaseDataContext db = new databaseDataContext();
        // === 1. KHAI BÁO CÁC BIẾN TOÀN CỤC ===
        private int currentPage = 1;
        private int pageSize = 5;
        private int totalRecords = 0;
        private int totalPages = 1;
        private DataTable dtToanBoSinhVien = new DataTable();

        public FormSinhVien()
        {
            InitializeComponent();

            // -------------------------------------------------------------
            // THỦ THUẬT: ÉP BUỘC KẾT NỐI SỰ KIỆN (Trị dứt điểm lỗi bấm không chạy)
            // Lệnh này đảm bảo 100% khi bấm nút Sửa hoặc bấm vào Bảng thì code sẽ chạy
            // -------------------------------------------------------------
            this.btnSua.Click -= new System.EventHandler(this.btnSua_Click);
            this.btnSua.Click += new System.EventHandler(this.btnSua_Click);

            // Dùng CellClick thay vì CellContentClick để bấm vào đâu trong dòng cũng ăn
            this.dgvSinhVien.CellClick -= new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dgvSinhVien.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // -------------------------------------------------------------

            LoadComboBoxGioiTinh();
            LoadComboBoxLop();
            HienThiDuLieuLenBang("");
        }

        // === 2. CÁC HÀM XỬ LÝ DỮ LIỆU ===
        private void LoadComboBoxGioiTinh()
        {
            cboGioiTinh.Items.Clear();
            cboGioiTinh.Items.Add("Nam");
            cboGioiTinh.Items.Add("Nữ");
            cboGioiTinh.SelectedIndex = -1;
        }

        // ==============================
        // LOAD DANH SÁCH LỚP VÀO COMBOBOX
        // ==============================
        private void LoadComboBoxLop()
        {
            var dsLop = db.tbl_lophocs.ToList();

            cboLop.DataSource = dsLop;
            cboLop.DisplayMember = "MaLop";
            cboLop.ValueMember = "MaLop";
            cboLop.SelectedIndex = -1;
        }

        // ==============================
        // HIỂN THỊ DANH SÁCH SINH VIÊN
        // ==============================
        private void HienThiDuLieuLenBang(string tuKhoa)
        {
            dgvSinhVien.Rows.Clear();

            var query = db.tbl_sinhviens.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(sv =>
                    sv.MaSV.Contains(tuKhoa) ||
                    sv.HoTen.Contains(tuKhoa) ||
                    sv.GioiTinh.Contains(tuKhoa) ||
                    sv.MaLop.Contains(tuKhoa)
                );
            }

            totalRecords = query.Count();
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            if (totalPages == 0)
            {
                totalPages = 1;
            }

            if (currentPage > totalPages)
            {
                currentPage = totalPages;
            }

            var dsSinhVien = query
                .OrderBy(sv => sv.MaSV)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var sv in dsSinhVien)
            {
                dgvSinhVien.Rows.Add(
                    sv.MaSV,
                    sv.HoTen,
                    sv.GioiTinh,
                    sv.NgaySinh.HasValue ? sv.NgaySinh.Value.ToString("dd/MM/yyyy") : "",
                    sv.MaLop
                );
            }
        }

        // ==============================
        // CLICK VÀO BẢNG
        // ==============================
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int i = e.RowIndex;

            if (i >= 0 && i < dgvSinhVien.Rows.Count)
            {
                DataGridViewRow row = dgvSinhVien.Rows[i];

                if (row.Cells[0].Value != null)
                {
                    txtMaSV.Text = row.Cells[0].Value.ToString();
                    txtHoTen.Text = row.Cells[1].Value?.ToString();
                    cboGioiTinh.Text = row.Cells[2].Value?.ToString();

                    DateTime ngaySinh;
                    if (DateTime.TryParse(row.Cells[3].Value?.ToString(), out ngaySinh))
                    {
                        dtpNgaySinh.Value = ngaySinh;
                    }

                    cboLop.Text = row.Cells[4].Value?.ToString();

                    txtMaSV.Enabled = false;
                }
            }
        }

        // ==============================
        // THÊM SINH VIÊN
        // ==============================
        private void btnThem_Click(object sender, EventArgs e)
        {
            if (txtMaSV.Text == "" || txtHoTen.Text == "" || cboLop.Text == "")
            {
                MessageBox.Show("Vui lòng nhập đủ Mã SV, Họ tên và Lớp!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var kt = db.tbl_sinhviens.FirstOrDefault(sv => sv.MaSV == txtMaSV.Text.Trim());

            if (kt != null)
            {
                MessageBox.Show("Mã sinh viên này đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            tbl_sinhvien svMoi = new tbl_sinhvien();

            svMoi.MaSV = txtMaSV.Text.Trim();
            svMoi.HoTen = txtHoTen.Text.Trim();
            svMoi.GioiTinh = cboGioiTinh.Text;
            svMoi.NgaySinh = dtpNgaySinh.Value;
            svMoi.MaLop = cboLop.SelectedValue.ToString();

            db.tbl_sinhviens.InsertOnSubmit(svMoi);
            db.SubmitChanges();

            MessageBox.Show("Thêm sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            currentPage = totalPages;
            HienThiDuLieuLenBang(txtTimKiem.Text);
            btnLamMoi_Click(sender, e);
        }

        // ==============================
        // SỬA SINH VIÊN
        // ==============================
        private void btnSua_Click(object sender, EventArgs e)
        {
            string maSV = txtMaSV.Text.Trim();

            if (string.IsNullOrEmpty(maSV))
            {
                MessageBox.Show("Vui lòng bấm vào 1 sinh viên trên bảng dữ liệu trước!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var sv = db.tbl_sinhviens.FirstOrDefault(x => x.MaSV == maSV);

            if (sv == null)
            {
                MessageBox.Show("Không tìm thấy sinh viên trong cơ sở dữ liệu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            sv.HoTen = txtHoTen.Text.Trim();
            sv.GioiTinh = cboGioiTinh.Text;
            sv.NgaySinh = dtpNgaySinh.Value;
            sv.MaLop = cboLop.SelectedValue.ToString();

            db.SubmitChanges();

            MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            HienThiDuLieuLenBang(txtTimKiem.Text);
            btnLamMoi_Click(sender, e);
        }

        // ==============================
        // XÓA SINH VIÊN
        // ==============================
        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtMaSV.Text == "")
            {
                MessageBox.Show("Vui lòng chọn sinh viên cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn xóa sinh viên này?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No)
            {
                return;
            }

            var sv = db.tbl_sinhviens.FirstOrDefault(x => x.MaSV == txtMaSV.Text.Trim());

            if (sv == null)
            {
                MessageBox.Show("Không tìm thấy sinh viên!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            db.tbl_sinhviens.DeleteOnSubmit(sv);
            db.SubmitChanges();

            MessageBox.Show("Xóa sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            HienThiDuLieuLenBang(txtTimKiem.Text);
            btnLamMoi_Click(sender, e);
        }

        // ==============================
        // LÀM MỚI
        // ==============================
        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtMaSV.Clear();
            txtHoTen.Clear();

            cboGioiTinh.SelectedIndex = -1;
            cboLop.SelectedIndex = -1;

            dtpNgaySinh.Value = DateTime.Now;

            txtMaSV.Enabled = true;
            txtMaSV.Focus();
        }

        // ==============================
        // TÌM KIẾM
        // ==============================
        private void btnTim_Click(object sender, EventArgs e)
        {
            currentPage = 1;
            HienThiDuLieuLenBang(txtTimKiem.Text.Trim());
        }

        // ==============================
        // PHÂN TRANG
        // ==============================
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages)
            {
                currentPage++;
                HienThiDuLieuLenBang(txtTimKiem.Text);
            }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                HienThiDuLieuLenBang(txtTimKiem.Text);
            }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            currentPage = 1;
            HienThiDuLieuLenBang(txtTimKiem.Text);
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            currentPage = totalPages;
            HienThiDuLieuLenBang(txtTimKiem.Text);
        }

        // === 7. NHỮNG HÀM RÁC ===
        private void button1_Click(object sender, EventArgs e) { }
        private void button1_Click_1(object sender, EventArgs e) { }
        private void button1_Click_2(object sender, EventArgs e) { }
        private void button1_Click_3(object sender, EventArgs e) { }
        private void txtMaSV_TextChanged(object sender, EventArgs e) { }
        private void txtHoTen_TextChanged(object sender, EventArgs e) { }
        private void cboGioiTinh_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cboLop_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtTimKiem_TextChanged(object sender, EventArgs e) { }
        private void GroupBox_Enter(object sender, EventArgs e) { }
    }
}