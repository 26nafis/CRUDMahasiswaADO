using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CRUDMahasiswaADO
{
    public partial class Form1 : Form
    {
        // Pengaturan koneksi sesuai server kamu
        private readonly SqlConnection conn;
        private readonly string connectionString = "Data Source=NAFIS\\NAFISCOY; Initial Catalog=DBAkademikADO; Integrated Security=True";
        private BindingSource bindingSource = new BindingSource();
        private DataTable dtMahasiswa = new DataTable();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Inisialisasi ComboBox Jenis Kelamin
            cmbJK.DataSource = new string[] { "L", "P" };

            // Konfigurasi DataGridView agar rapi dan aman
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.MultiSelect = false;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Load data pertama kali saat form dibuka
            LoadData();
        }

        private void SimpanLog(string pesan)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"INSERT INTO LogError (Waktu, Pesan) VALUES (GETDATE(), @pesan)";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@pesan", pesan);
                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Mengisolasi kegagalan logging agar tidak memicu crash beruntun
            }
        }

        private void LoadData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            dtMahasiswa = new DataTable();
                            da.Fill(dtMahasiswa);

                            bindingSource.DataSource = dtMahasiswa;
                            dataGridView1.DataSource = bindingSource;

                            // DataBinding otomatis mengatur sinkronisasi Grid -> TextBox saat baris dipilih
                            BindControls();
                        }
                    }
                }
                HitungTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HitungTotal()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter outputParam = new SqlParameter("@Total", SqlDbType.Int);
                        outputParam.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outputParam);

                        connection.Open();
                        cmd.ExecuteNonQuery();

                        lblTotal.Text = "Total Mahasiswa: " + (outputParam.Value ?? 0).ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghitung total: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BindControls()
        {
            // Bersihkan binding lama untuk menghindari tumpukan data binding
            txtNIM.DataBindings.Clear();
            txtNama.DataBindings.Clear();
            cmbJK.DataBindings.Clear();
            dtpTanggalLahir.DataBindings.Clear();
            txtAlamat.DataBindings.Clear();
            txtkodeProdi.DataBindings.Clear();

            // Menerapkan binding baru secara otomatis mendeteksi perubahan baris di DataGridView
            txtNIM.DataBindings.Add("Text", bindingSource, "NIM", true, DataSourceUpdateMode.Never);
            txtNama.DataBindings.Add("Text", bindingSource, "Nama", true, DataSourceUpdateMode.Never);
            cmbJK.DataBindings.Add("Text", bindingSource, "JenisKelamin", true, DataSourceUpdateMode.Never);
            dtpTanggalLahir.DataBindings.Add("Value", bindingSource, "TanggalLahir", true, DataSourceUpdateMode.Never);
            txtAlamat.DataBindings.Add("Text", bindingSource, "Alamat", true, DataSourceUpdateMode.Never);
            txtkodeProdi.DataBindings.Add("Text", bindingSource, "KodeProdi", true, DataSourceUpdateMode.Never);
        }

        private void ClearForm()
        {
            // Memutus binding sementara agar form bisa dikosongkan untuk input data baru
            txtNIM.DataBindings.Clear();
            txtNama.DataBindings.Clear();
            cmbJK.DataBindings.Clear();
            dtpTanggalLahir.DataBindings.Clear();
            txtAlamat.DataBindings.Clear();
            txtkodeProdi.DataBindings.Clear();

            txtNIM.Clear();
            txtNama.Clear();
            cmbJK.SelectedIndex = -1;
            txtAlamat.Clear();
            txtkodeProdi.Clear();
            dtpTanggalLahir.Value = DateTime.Now;
            txtNIM.Focus();
        }

        private bool IsInputValid()
        {
            if (!Regex.IsMatch(txtNIM.Text, @"^\d+$"))
            {
                MessageBox.Show("NIM harus berupa angka saja!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNIM.Focus();
                return false;
            }
            if (txtNIM.Text.Length > 11)
            {
                MessageBox.Show("NIM tidak boleh lebih dari 11 karakter!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Nama Mahasiswa wajib diisi!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (cmbJK.SelectedIndex == -1 || string.IsNullOrEmpty(cmbJK.Text))
            {
                MessageBox.Show("Pilih Jenis Kelamin!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtkodeProdi.Text))
            {
                MessageBox.Show("Kode Prodi wajib diisi!", "Validasi Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    MessageBox.Show("Koneksi ke database berhasil!", "Status Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Koneksi gagal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (!IsInputValid()) return;

            DialogResult dialogResult = MessageBox.Show($"Yakin ingin menambah data Mahasiswa:\nNIM: {txtNIM.Text}\nNama: {txtNama.Text}?",
                                                        "Konfirmasi Tambah Data", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.No) return;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction trans = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmdInsert = new SqlCommand("sp_InsertMahasiswa", connection, trans))
                        {
                            cmdInsert.CommandType = CommandType.StoredProcedure;

                            cmdInsert.Parameters.AddWithValue("@NIM", txtNIM.Text);
                            cmdInsert.Parameters.AddWithValue("@Nama", txtNama.Text);
                            cmdInsert.Parameters.AddWithValue("@JenisKelamin", cmbJK.Text);
                            cmdInsert.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                            cmdInsert.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                            cmdInsert.Parameters.AddWithValue("@KodeProdi", txtkodeProdi.Text);

                            cmdInsert.ExecuteNonQuery();
                        }

                        string logQuery = @"INSERT INTO LogAktivitas (aktivitas, waktu) VALUES (@aktivitas, GETDATE())";
                        using (SqlCommand cmdLog = new SqlCommand(logQuery, connection, trans))
                        {
                            cmdLog.Parameters.AddWithValue("@aktivitas", "INSERT MAHASISWA : " + txtNIM.Text);
                            cmdLog.ExecuteNonQuery();
                        }

                        trans.Commit();

                        MessageBox.Show("Data berhasil ditambahkan", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearForm();
                        LoadData();
                    }
                    catch (SqlException ex)
                    {
                        trans.Rollback();
                        SimpanLog("ROLLBACK INSERT : " + ex.Message);

                        if (ex.Number == 2627)
                            MessageBox.Show("NIM sudah ada di database!", "Primary Key Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        else if (ex.Number == 547)
                            MessageBox.Show("Kode Prodi tidak terdaftar!", "Foreign Key Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        else
                            MessageBox.Show("SQL Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        SimpanLog("GENERAL ERROR : " + ex.Message);
                        MessageBox.Show("General Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!IsInputValid()) return;

            DialogResult dialogResult = MessageBox.Show($"Simpan perubahan untuk NIM: {txtNIM.Text}?",
                                                        "Konfirmasi Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.No) return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_UpdateMahasiswa", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@NIM", txtNIM.Text);
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                        cmd.Parameters.AddWithValue("@JenisKelamin", cmbJK.Text);
                        cmd.Parameters.AddWithValue("@TanggalLahir", dtpTanggalLahir.Value.Date);
                        cmd.Parameters.AddWithValue("@Alamat", txtAlamat.Text);
                        cmd.Parameters.AddWithValue("@KodeProdi", txtkodeProdi.Text);

                        connection.Open();
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Data berhasil diperbarui!", "Update Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                        }
                        else
                        {
                            MessageBox.Show("Data NIM tersebut tidak ditemukan.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("SQL Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                SimpanLog(ex.Message);
                MessageBox.Show("General Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNIM.Text))
            {
                MessageBox.Show("Pilih atau isi NIM yang ingin dihapus!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (MessageBox.Show($"Apakah anda yakin ingin menghapus permanen data NIM: {txtNIM.Text}?",
                "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", connection))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@NIM", SqlDbType.VarChar, 50).Value = txtNIM.Text;

                            connection.Open();
                            int result = cmd.ExecuteNonQuery();

                            if (result > 0)
                            {
                                MessageBox.Show("Data berhasil dihapus selamanya.", "Hapus Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                ClearForm();
                                LoadData();
                            }
                            else
                            {
                                MessageBox.Show("Data NIM tidak ditemukan atau sudah terhapus.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("SQL Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    SimpanLog(ex.Message);
                    MessageBox.Show("General Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnResetData_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"
                        IF OBJECT_ID('dbo.Mahasiswa_Backup') IS NOT NULL
                        BEGIN
                            DELETE FROM dbo.Mahasiswa;
                            INSERT INTO dbo.Mahasiswa
                            SELECT * FROM dbo.Mahasiswa_Backup;
                        END";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Data berhasil direset", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reset gagal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // PERBAIKAN: Mengamankan Query dari bahaya SQL Injection menggunakan Parameterized Query
        private void btnTestInjection_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Mahasiswa SET Nama = @Nama WHERE NIM = @NIM";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                        cmd.Parameters.AddWithValue("@NIM", txtNIM.Text);

                        connection.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Update via Parameterized Query (Aman dari SQL Injection) berhasil", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // PERBAIKAN: Mengatur navigasi antar form agar aplikasi tidak menggantung saat Form3 ditutup
        private void btnRekapData_Click(object sender, EventArgs e)
        {
            Form3 formRekap = new Form3();
            // Menampilkan Form3 sebagai dialog modal (Form1 akan terkunci sampai Form3 ditutup)
            formRekap.ShowDialog();

            // Setelah Form3 ditutup, segarkan data di Form1 secara otomatis
            LoadData();
        }
    }
}