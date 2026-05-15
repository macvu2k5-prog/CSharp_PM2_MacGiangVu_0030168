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
        // === 1. KHAI BÁO CÁC BIẾN TOÀN CỤC ===
        private int currentPage = 1;
        private int pageSize = 5;
        private int totalRecords = 0;
        private int totalPages = 1;
        private DataTable dtToanBoLopHoc = new DataTable();

        public FormLopHoc()
        {
            InitializeComponent();

            // -------------------------------------------------------------
            // THỦ THUẬT: Ép buộc kết nối sự kiện (Trị dứt điểm lỗi đứt gãy nút bấm)
            // -------------------------------------------------------------
            this.btnSua.Click -= new System.EventHandler(this.btnSua_Click);
            this.btnSua.Click += new System.EventHandler(this.btnSua_Click);

            // Ép bảng dùng CellClick (bấm đâu trong ô cũng ăn) thay vì CellContentClick
            this.dgvLopHoc.CellClick -= new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvLopHoc_CellContentClick);
            this.dgvLopHoc.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvLopHoc_CellContentClick);
            // -------------------------------------------------------------

            LoadDuLieuGia(); // Khởi tạo khung dữ liệu
            HienThiDuLieuLenBang("");
        }

        // === 2. CÁC HÀM XỬ LÝ DỮ LIỆU ===
        private void LoadDuLieuGia()
        {
            // Tạo cấu trúc cột cho DataTable
            dtToanBoLopHoc.Columns.Add("MaID");
            dtToanBoLopHoc.Columns.Add("MaLop");
            dtToanBoLopHoc.Columns.Add("TenLop");
            dtToanBoLopHoc.Columns.Add("GhiChu");

            // Khởi tạo sẵn 2 lớp giống trong ảnh mẫu của ông để dễ test
            dtToanBoLopHoc.Rows.Add("1", "68PM1", "Lớp 68PM1", "abc");
            dtToanBoLopHoc.Rows.Add("2", "68PM2", "Lớp 68PM2", "xyz");
        }

        private void HienThiDuLieuLenBang(string tuKhoa)
        {
            dgvLopHoc.Rows.Clear();

            // Lọc dữ liệu theo từ khóa tìm kiếm
            var duLieuDaLoc = dtToanBoLopHoc.AsEnumerable().Where(row =>
                row["MaID"].ToString().ToLower().Contains(tuKhoa.ToLower()) ||
                row["MaLop"].ToString().ToLower().Contains(tuKhoa.ToLower()) ||
                row["TenLop"].ToString().ToLower().Contains(tuKhoa.ToLower())
            ).ToList();

            // Phân trang
            totalRecords = duLieuDaLoc.Count;
            totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            if (totalPages == 0) totalPages = 1;

            if (currentPage > totalPages) currentPage = totalPages;

            int soDongCanBoQua = (currentPage - 1) * pageSize;
            var duLieuTrangHienTai = duLieuDaLoc.Skip(soDongCanBoQua).Take(pageSize);

            foreach (var row in duLieuTrangHienTai)
            {
                dgvLopHoc.Rows.Add(row["MaID"], row["MaLop"], row["TenLop"], row["GhiChu"]);
            }
        }

        // === 3. LOGIC CHÍNH: BẤM VÀO BẢNG -> DỮ LIỆU VỌT LÊN GROUPBOX ===
        private void dgvLopHoc_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int i = e.RowIndex;

            // Kiểm tra xem có bấm đúng vào dòng có dữ liệu không
            if (i >= 0 && i < dgvLopHoc.Rows.Count)
            {
                DataGridViewRow row = dgvLopHoc.Rows[i];

                if (row.Cells[0].Value != null)
                {
                    txtMaID.Text = row.Cells[0].Value.ToString();
                    txtMaLop.Text = row.Cells[1].Value?.ToString();
                    txtTenLop.Text = row.Cells[2].Value?.ToString();
                    txtGhiChu.Text = row.Cells[3].Value?.ToString();

                    // Khóa ô Mã ID lại để lúc sửa không lỡ tay gõ nhầm làm hỏng liên kết
                    txtMaID.Enabled = false;
                }
            }
        }

        // === 4. CÁC NÚT CHỨC NĂNG (CRUD) ===
        private void btnThem_Click(object sender, EventArgs e)
        {
            if (txtMaID.Text == "" || txtMaLop.Text == "")
            {
                MessageBox.Show("Vui lòng nhập đủ Mã ID và Mã lớp!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra trùng mã
            bool trungMa = dtToanBoLopHoc.AsEnumerable().Any(row => row["MaID"].ToString() == txtMaID.Text);
            if (trungMa)
            {
                MessageBox.Show("Mã ID này đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            dtToanBoLopHoc.Rows.Add(txtMaID.Text, txtMaLop.Text, txtTenLop.Text, txtGhiChu.Text);

            currentPage = int.MaxValue;
            HienThiDuLieuLenBang(txtTimKiem.Text);
            btnLamMoi_Click(sender, e);
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            string maID = txtMaID.Text.Trim();

            if (string.IsNullOrEmpty(maID))
            {
                MessageBox.Show("Vui lòng bấm vào 1 lớp trên bảng dữ liệu trước!", "Nhắc nhở", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool daCapNhat = false;

            // Quét dữ liệu gốc để cập nhật thông tin mới
            foreach (DataRow row in dtToanBoLopHoc.Rows)
            {
                if (row["MaID"].ToString() == maID)
                {
                    row["MaLop"] = txtMaLop.Text.Trim();
                    row["TenLop"] = txtTenLop.Text.Trim();
                    row["GhiChu"] = txtGhiChu.Text.Trim();

                    daCapNhat = true;
                    break;
                }
            }

            if (daCapNhat)
            {
                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                HienThiDuLieuLenBang(txtTimKiem.Text);
                btnLamMoi_Click(sender, e); // Làm mới ô nhập và mở khóa Mã ID
            }
            else
            {
                MessageBox.Show("Không tìm thấy lớp học trong cơ sở dữ liệu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn xóa lớp học này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                for (int i = dtToanBoLopHoc.Rows.Count - 1; i >= 0; i--)
                {
                    if (dtToanBoLopHoc.Rows[i]["MaID"].ToString() == txtMaID.Text)
                    {
                        dtToanBoLopHoc.Rows.RemoveAt(i);
                        break;
                    }
                }
                HienThiDuLieuLenBang(txtTimKiem.Text);
                btnLamMoi_Click(sender, e);
            }
        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtMaID.Clear();
            txtMaLop.Clear();
            txtTenLop.Clear();
            txtGhiChu.Clear();

            txtMaID.Enabled = true; // MỞ KHÓA LẠI Ô MÃ ID ĐỂ THÊM MỚI
            txtMaID.Focus();
        }

        private void btnTim_Click(object sender, EventArgs e)
        {
            currentPage = 1;
            HienThiDuLieuLenBang(txtTimKiem.Text);
        }

        private void btnXemDSSV_Click(object sender, EventArgs e)
        {
            if (txtMaLop.Text == "")
            {
                MessageBox.Show("Vui lòng chọn 1 lớp để xem danh sách sinh viên!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show($"Tính năng này sẽ được code để tự động mở trang Quản lý Sinh viên và lọc ra những bạn thuộc {txtMaLop.Text}.", "Đang phát triển", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // === 5. CÁC NÚT PHÂN TRANG ===
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

        // === 6. NHỮNG HÀM RÁC (Lỡ nhấp đúp - Giữ nguyên để tránh lỗi giao diện) ===
        private void txtMaID_TextChanged(object sender, EventArgs e) { }
        private void txtMaLop_TextChanged(object sender, EventArgs e) { }
        private void txtTenLop_TextChanged(object sender, EventArgs e) { }
        private void txtGhiChu_TextChanged(object sender, EventArgs e) { }
        private void txtTimKiem_TextChanged(object sender, EventArgs e) { }
    }
}