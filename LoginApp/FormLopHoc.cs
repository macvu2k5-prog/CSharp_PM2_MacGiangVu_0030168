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
    public partial class FormLopHoc : Form
    {
        databaseDataContext db = new databaseDataContext();
        // === 1. KHAI BÁO CÁC BIẾN TOÀN CỤC ===
        private int currentPage = 1;
        private int pageSize = 5;
        private int totalRecords = 0;
        private int totalPages = 1;
        private DataTable dtToanBoLopHoc = new DataTable();

        public FormLopHoc()
        {
            InitializeComponent();

            this.btnSua.Click -= new System.EventHandler(this.btnSua_Click);
            this.btnSua.Click += new System.EventHandler(this.btnSua_Click);

            this.dgvLopHoc.CellClick -= new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvLopHoc_CellContentClick);
            this.dgvLopHoc.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvLopHoc_CellContentClick);

            txtMaID.Enabled = false;

            HienThiDuLieuLenBang("");
        }

        // ==============================
        // HIỂN THỊ DANH SÁCH LỚP HỌC
        // ==============================
        private void HienThiDuLieuLenBang(string tuKhoa)
        {
            dgvLopHoc.Rows.Clear();

            var query = db.tbl_lophocs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(lh =>
                    lh.MaID.ToString().Contains(tuKhoa) ||
                    lh.MaLop.Contains(tuKhoa) ||
                    lh.TenLop.Contains(tuKhoa) ||
                    lh.GhiChu.Contains(tuKhoa)
                );
            }

            totalRecords = query.Count();
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            if (totalPages == 0)
                totalPages = 1;

            if (currentPage > totalPages)
                currentPage = totalPages;

            var dsLopHoc = query
                .OrderBy(lh => lh.MaID)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (var lh in dsLopHoc)
            {
                dgvLopHoc.Rows.Add(
                    lh.MaID,
                    lh.MaLop,
                    lh.TenLop,
                    lh.GhiChu
                );
            }
        }

        // ==============================
        // CLICK VÀO BẢNG
        // ==============================
        private void dgvLopHoc_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int i = e.RowIndex;

            if (i >= 0 && i < dgvLopHoc.Rows.Count)
            {
                DataGridViewRow row = dgvLopHoc.Rows[i];

                if (row.Cells[0].Value != null)
                {
                    txtMaID.Text = row.Cells[0].Value.ToString();
                    txtMaLop.Text = row.Cells[1].Value?.ToString();
                    txtTenLop.Text = row.Cells[2].Value?.ToString();
                    txtGhiChu.Text = row.Cells[3].Value?.ToString();

                    txtMaID.Enabled = false;
                }
            }
        }

        // ==============================
        // THÊM LỚP HỌC
        // ==============================
        private void btnThem_Click(object sender, EventArgs e)
        {
            if (txtMaLop.Text == "" || txtTenLop.Text == "")
            {
                MessageBox.Show("Vui lòng nhập Mã lớp và Tên lớp!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var kt = db.tbl_lophocs.FirstOrDefault(lh => lh.MaLop == txtMaLop.Text.Trim());

            if (kt != null)
            {
                MessageBox.Show("Mã lớp này đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            tbl_lophoc lopMoi = new tbl_lophoc();

            lopMoi.MaLop = txtMaLop.Text.Trim();
            lopMoi.TenLop = txtTenLop.Text.Trim();
            lopMoi.GhiChu = txtGhiChu.Text.Trim();

            db.tbl_lophocs.InsertOnSubmit(lopMoi);
            db.SubmitChanges();

            MessageBox.Show("Thêm lớp học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            HienThiDuLieuLenBang(txtTimKiem.Text);
            btnLamMoi_Click(sender, e);
        }

        // ==============================
        // SỬA LỚP HỌC
        // ==============================
        private void btnSua_Click(object sender, EventArgs e)
        {
            if (txtMaID.Text == "")
            {
                MessageBox.Show("Vui lòng chọn lớp học cần sửa!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maID = int.Parse(txtMaID.Text);

            var lop = db.tbl_lophocs.FirstOrDefault(lh => lh.MaID == maID);

            if (lop == null)
            {
                MessageBox.Show("Không tìm thấy lớp học trong cơ sở dữ liệu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string maLopCu = lop.MaLop;
            string maLopMoi = txtMaLop.Text.Trim();

            var ktTrung = db.tbl_lophocs.FirstOrDefault(lh => lh.MaLop == maLopMoi && lh.MaID != maID);

            if (ktTrung != null)
            {
                MessageBox.Show("Mã lớp này đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Nếu lớp đã có sinh viên thì không cho đổi Mã lớp
            bool coSinhVien = db.tbl_sinhviens.Any(sv => sv.MaLop == maLopCu);

            if (coSinhVien && maLopCu != maLopMoi)
            {
                MessageBox.Show("Không thể đổi Mã lớp vì lớp này đang có sinh viên!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lop.MaLop = maLopMoi;
            lop.TenLop = txtTenLop.Text.Trim();
            lop.GhiChu = txtGhiChu.Text.Trim();

            db.SubmitChanges();

            MessageBox.Show("Cập nhật thông tin lớp học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            HienThiDuLieuLenBang(txtTimKiem.Text);
            btnLamMoi_Click(sender, e);
        }

        // ==============================
        // XÓA LỚP HỌC
        // ==============================
        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (txtMaID.Text == "")
            {
                MessageBox.Show("Vui lòng chọn lớp học cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int maID = int.Parse(txtMaID.Text);

            var lop = db.tbl_lophocs.FirstOrDefault(lh => lh.MaID == maID);

            if (lop == null)
            {
                MessageBox.Show("Không tìm thấy lớp học!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool coSinhVien = db.tbl_sinhviens.Any(sv => sv.MaLop == lop.MaLop);

            if (coSinhVien)
            {
                MessageBox.Show("Không thể xóa lớp này vì đang có sinh viên thuộc lớp!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Bạn có chắc muốn xóa lớp học này?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No)
            {
                return;
            }

            db.tbl_lophocs.DeleteOnSubmit(lop);
            db.SubmitChanges();

            MessageBox.Show("Xóa lớp học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            HienThiDuLieuLenBang(txtTimKiem.Text);
            btnLamMoi_Click(sender, e);
        }

        // ==============================
        // LÀM MỚI
        // ==============================
        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtMaID.Clear();
            txtMaLop.Clear();
            txtTenLop.Clear();
            txtGhiChu.Clear();

            txtMaID.Enabled = false;
            txtMaLop.Focus();
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
        // XEM DANH SÁCH SINH VIÊN THEO LỚP
        // ==============================
        private void btnXemDSSV_Click(object sender, EventArgs e)
        {
            if (txtMaLop.Text == "")
            {
                MessageBox.Show(
                    "Vui lòng chọn 1 lớp để xem danh sách sinh viên!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            string maLop = txtMaLop.Text.Trim();
            string tenLop = txtTenLop.Text.Trim();

            FormDSSV frm = new FormDSSV(maLop, tenLop);
            frm.ShowDialog();
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

        // === 6. NHỮNG HÀM RÁC (Lỡ nhấp đúp - Giữ nguyên để tránh lỗi giao diện) ===
        private void txtMaID_TextChanged(object sender, EventArgs e) { }
        private void txtMaLop_TextChanged(object sender, EventArgs e) { }
        private void txtTenLop_TextChanged(object sender, EventArgs e) { }
        private void txtGhiChu_TextChanged(object sender, EventArgs e) { }
        private void txtTimKiem_TextChanged(object sender, EventArgs e) { }
    }
}