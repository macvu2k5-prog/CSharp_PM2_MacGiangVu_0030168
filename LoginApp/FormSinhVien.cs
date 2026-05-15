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

            LoadDuLieuGia(); // Khởi tạo khung dữ liệu
            HienThiDuLieuLenBang("");
        }

        // === 2. CÁC HÀM XỬ LÝ DỮ LIỆU ===
        private void LoadDuLieuGia()
        {
            dtToanBoSinhVien.Columns.Add("MaSV");
            dtToanBoSinhVien.Columns.Add("HoTen");
            dtToanBoSinhVien.Columns.Add("GioiTinh");
            dtToanBoSinhVien.Columns.Add("NgaySinh");
            dtToanBoSinhVien.Columns.Add("Lop");
        }

        private void HienThiDuLieuLenBang(string tuKhoa)
        {
            dgvSinhVien.Rows.Clear();

            var duLieuDaLoc = dtToanBoSinhVien.AsEnumerable().Where(row =>
                row["MaSV"].ToString().ToLower().Contains(tuKhoa.ToLower()) ||
                row["HoTen"].ToString().ToLower().Contains(tuKhoa.ToLower())
            ).ToList();

            totalRecords = duLieuDaLoc.Count;
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            if (totalPages == 0) totalPages = 1;

            if (currentPage > totalPages) currentPage = totalPages;

            int soDongCanBoQua = (currentPage - 1) * pageSize;

            var duLieuTrangHienTai = duLieuDaLoc.Skip(soDongCanBoQua).Take(pageSize);

            foreach (var row in duLieuTrangHienTai)
            {
                dgvSinhVien.Rows.Add(row["MaSV"], row["HoTen"], row["GioiTinh"], row["NgaySinh"], row["Lop"]);
            }
        }

        // === 3. LOGIC CHÍNH: BẤM VÀO BẢNG -> DỮ LIỆU VỌT LÊN GROUPBOX ===
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int i = e.RowIndex;

            // Kiểm tra xem có bấm đúng vào dòng có dữ liệu không
            if (i >= 0 && i < dgvSinhVien.Rows.Count)
            {
                DataGridViewRow row = dgvSinhVien.Rows[i];

                if (row.Cells[0].Value != null)
                {
                    // Dữ liệu vọt lên groupbox
                    txtMaSV.Text = row.Cells[0].Value.ToString();
                    txtHoTen.Text = row.Cells[1].Value?.ToString();
                    cboGioiTinh.Text = row.Cells[2].Value?.ToString();
                    dtpNgaySinh.Text = row.Cells[3].Value?.ToString();
                    cboLop.Text = row.Cells[4].Value?.ToString();

                    // Khóa ô Mã SV lại để lúc Sửa không lỡ tay gõ nhầm làm mất dữ liệu gốc
                    txtMaSV.Enabled = false;
                }
            }
        }

        // === 4. LOGIC CHÍNH: ĐỔI THÔNG TIN -> BẤM SỬA -> BẢNG CẬP NHẬT TỨC THÌ ===
        private void btnSua_Click(object sender, EventArgs e)
        {
            string maSV = txtMaSV.Text.Trim();

            // Nếu ô Mã SV trống nghĩa là chưa bấm vào bảng
            if (string.IsNullOrEmpty(maSV))
            {
                MessageBox.Show("Vui lòng bấm vào 1 sinh viên trên bảng dữ liệu trước!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool daCapNhat = false;

            // Quét dữ liệu gốc để cập nhật thông tin mới
            foreach (DataRow row in dtToanBoSinhVien.Rows)
            {
                if (row["MaSV"].ToString() == maSV)
                {
                    row["HoTen"] = txtHoTen.Text.Trim();
                    row["GioiTinh"] = cboGioiTinh.Text;
                    row["NgaySinh"] = dtpNgaySinh.Text;
                    row["Lop"] = cboLop.Text;

                    daCapNhat = true;
                    break;
                }
            }

            if (daCapNhat)
            {
                // Thông báo đã cập nhật thành công
                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Bảng dữ liệu cập nhật ngay lập tức
                HienThiDuLieuLenBang(txtTimKiem.Text);

                // Làm mới các ô nhập và mở khóa lại ô Mã SV
                btnLamMoi_Click(sender, e);
            }
            else
            {
                MessageBox.Show("Không tìm thấy sinh viên trong cơ sở dữ liệu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // === 5. CÁC CHỨC NĂNG CÒN LẠI (THÊM, XÓA, LÀM MỚI, TÌM KIẾM) ===
        private void btnThem_Click(object sender, EventArgs e)
        {
            if (txtMaSV.Text == "" || txtHoTen.Text == "")
            {
                MessageBox.Show("Vui lòng nhập đủ Mã SV và Họ tên!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool trungMa = dtToanBoSinhVien.AsEnumerable().Any(row => row["MaSV"].ToString() == txtMaSV.Text);
            if (trungMa)
            {
                MessageBox.Show("Mã sinh viên này đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            dtToanBoSinhVien.Rows.Add(txtMaSV.Text, txtHoTen.Text, cboGioiTinh.Text, dtpNgaySinh.Text, cboLop.Text);

            currentPage = int.MaxValue;
            HienThiDuLieuLenBang(txtTimKiem.Text);
            btnLamMoi_Click(sender, e);
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn xóa sinh viên này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                for (int i = dtToanBoSinhVien.Rows.Count - 1; i >= 0; i--)
                {
                    if (dtToanBoSinhVien.Rows[i]["MaSV"].ToString() == txtMaSV.Text)
                    {
                        dtToanBoSinhVien.Rows.RemoveAt(i);
                        break;
                    }
                }
                HienThiDuLieuLenBang(txtTimKiem.Text);
                btnLamMoi_Click(sender, e);
            }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtMaSV.Clear();
            txtHoTen.Clear();
            cboGioiTinh.SelectedIndex = -1;
            cboLop.SelectedIndex = -1;
            dtpNgaySinh.Value = DateTime.Now;

            txtMaSV.Enabled = true; // MỞ KHÓA LẠI Ô MÃ SV ĐỂ THÊM MỚI
            txtMaSV.Focus();
        }

        private void btnTim_Click(object sender, EventArgs e)
        {
            currentPage = 1;
            HienThiDuLieuLenBang(txtTimKiem.Text);
        }

        // === 6. CÁC NÚT PHÂN TRANG ===
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (currentPage < totalPages) { currentPage++; HienThiDuLieuLenBang(txtTimKiem.Text); }
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            if (currentPage > 1) { currentPage--; HienThiDuLieuLenBang(txtTimKiem.Text); }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            if (currentPage != 1) { currentPage = 1; HienThiDuLieuLenBang(txtTimKiem.Text); }
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            if (currentPage != totalPages) { currentPage = totalPages; HienThiDuLieuLenBang(txtTimKiem.Text); }
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