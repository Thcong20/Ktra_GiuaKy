using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace KtraGiuaKy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // dgvSinhvien.DataSource = sinhviens;
            try
            {
                Model1 context = new Model1();
                List<Lop> listLop = context.Lops.ToList(); //lấy các khoa
                List<Sinhvien> listSinhvien = context.Sinhviens.ToList(); //lấy sinh viên
                FillLopCombobox(listLop);
                BindGrid(listSinhvien);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }



        }
        //Hàm binding list có tên hiện thị là tên khoa, giá trị là Mã khoa
        private void FillLopCombobox(List<Lop> listLop)
        {
            this.cbLop.DataSource = listLop;
            this.cbLop.DisplayMember = "TenLop";
            this.cbLop.ValueMember = "MaLop";
        }
        //Hàm binding gridView từ list sinh viên
        private void BindGrid(List<Sinhvien> listSinhvien)
        {
            dgvSinhvien.Rows.Clear();
            foreach (var item in listSinhvien)
            {
                int index = dgvSinhvien.Rows.Add();
                dgvSinhvien.Rows[index].Cells[0].Value = item.MaSV;
                dgvSinhvien.Rows[index].Cells[1].Value = item.HotenSV;
                dgvSinhvien.Rows[index].Cells[2].Value = item.NgaySinh;
                dgvSinhvien.Rows[index].Cells[3].Value = item.Lop.TenLop;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Ban co muon thoat khong ", "Thong bao",
                              MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                Application.Exit();
            }

        }

        private void btn_them_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_ma.Text) ||
                string.IsNullOrWhiteSpace(txt_ten.Text) ||
                cbLop.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin sinh viên!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Khởi tạo context để làm việc với database

                Model1 context = new Model1();
                // Lấy danh sách tất cả sinh viên trong CSDL
                List<Sinhvien> SinhvientList = context.Sinhviens.ToList();

                // Kiểm tra trùng mã sinh viên
                if (SinhvientList.Any(s => s.MaSV == txt_ma.Text))
                {
                    MessageBox.Show("Mã SV đã tồn tại. Vui lòng nhập một mã khác.",
                                    "Thông báo",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);
                    return;
                }

                // Tạo đối tượng sinh viên mới
                var newStudent = new Sinhvien
                {
                    MaSV = txt_ma.Text,
                    HotenSV = txt_ten.Text,
                    NgaySinh = date.Value,
                    MaLop = cbLop.SelectedValue.ToString(),

                };

                // Thêm sinh viên vào CSDL
                context.Sinhviens.Add(newStudent);
                context.SaveChanges();

                // Hiển thị lại danh sách sinh viên sau khi thêm
                BindGrid(context.Sinhviens.ToList());

                // Thông báo thành công
                MessageBox.Show("Thêm sinh viên thành công!",
                                "Thông báo",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Hiển thị thông báo lỗi khi thêm dữ liệu
                MessageBox.Show($"Lỗi khi thêm dữ liệu: {ex.Message}",
                                "Thông báo",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void btn_xoa_Click(object sender, EventArgs e)
        {
            DialogResult res = MessageBox.Show("Ban co muon thoat khong ", "Thong bao",
                             MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                try
                {
                    using (Model1 context = new Model1())
                    {
                        // Lấy mã sinh viên từ dòng được chọn trong DataGridView
                        if (dgvSinhvien.CurrentRow != null)
                        {
                            string maSinhVien = dgvSinhvien.CurrentRow.Cells[0].Value.ToString();

                            // Tìm sinh viên trong database
                            var student = context.Sinhviens.FirstOrDefault(s => s.MaSV == maSinhVien);

                            if (student != null)
                            {
                                // Xóa sinh viên
                                context.Sinhviens.Remove(student);
                                context.SaveChanges();

                                // Load lại dữ liệu vào DataGridView
                                BindGrid(context.Sinhviens.ToList());

                                MessageBox.Show("Sinh viên đã được xóa thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Sinh viên không tồn tại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Vui lòng chọn sinh viên cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa sinh viên: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        

        private void dgvSinhvien_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectRow = dgvSinhvien.Rows[e.RowIndex];
                txt_ma.Text = selectRow.Cells[0].Value?.ToString();
                txt_ten.Text = selectRow.Cells[1].Value?.ToString();

                // Kiểm tra và chuyển đổi ngày tháng an toàn
                if (DateTime.TryParse(selectRow.Cells[2].Value?.ToString(), out DateTime ngaySinh))
                {
                    date.Value = ngaySinh; // Gán cho DateTimePicker
                }
                else
                {
                    MessageBox.Show("Ngày sinh không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                cbLop.Text = selectRow.Cells[3].Value?.ToString();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string searchMSSV = txtSearch.Text.Trim();

                if (string.IsNullOrWhiteSpace(searchMSSV))
                {
                    MessageBox.Show("Vui lòng nhập MSSV cần tìm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Model1 context = new Model1();
                var sinhvien = context.Sinhviens.FirstOrDefault(s => s.MaSV == searchMSSV);

                if (sinhvien != null)
                {
                    txt_ma.Text = sinhvien.MaSV;
                    txt_ten.Text = sinhvien.HotenSV;
                    date.Value = (DateTime)sinhvien.NgaySinh;
                    cbLop.SelectedValue = sinhvien.MaLop;
                    BindGrid(new List<Sinhvien> { sinhvien });

                    MessageBox.Show("Tìm thấy sinh viên!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Không tìm thấy sinh viên với MSSV này!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tìm sinh viên: {ex.Message}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}