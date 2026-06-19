using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public partial class Form4 : Form
    {
        // Menyimpan Connection String di tempat yang aman
        private readonly string connectionString = "Data Source=F; Initial Catalog=DBAkademikADO; Integrated Security=True";
        private BindingSource bindingSource = new BindingSource();
        private DataTable dtMahasiswa = new DataTable();

        // Instance dari file Crystal Report (.rpt) yang kamu buat tadi
        private ListMahasisaw listMahasisaw = new ListMahasisaw();

        private string prodi;
        private DateTime tglmasuk;

        // Constructor diubah agar menerima lemparan data dari Form3
        public Form4(string Prodi, DateTime TglMasuk)
        {
            InitializeComponent();
            conn = new SqlConnection(connectionString);

            this.prodi = Prodi;
            this.tglmasuk = TglMasuk;
            try
            {
                if (conn.State == ConnectionState.Closed) conn.Open();

                SqlCommand cmd = new SqlCommand("sp_Report", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                // PERBAIKAN 1: Jadikan tahun sebagai String agar SQL tidak bingung
                cmd.Parameters.AddWithValue("@inProdi", prodi);
                cmd.Parameters.AddWithValue("@inTglMsuk", tglmasuk.Year.ToString());

                da = new SqlDataAdapter(cmd);

                // PERBAIKAN 2: Wajib beri nama DataTable sama persis dengan nama Class (Data)
                dtMahasiswa = new DataTable();
                dtMahasiswa.TableName = "Data";

                da.Fill(dtMahasiswa);
                conn.Close();

                // (Opsional) Intip apakah Form4 berhasil narik data
                MessageBox.Show("Data berhasil ditarik: " + dtMahasiswa.Rows.Count + " baris");

                // Masukkan data dari SQL Server ke dalam Crystal Report
                listMahasisaw.SetDataSource(dtMahasiswa);
                crystalReportViewer1.ReportSource = listMahasisaw;
                crystalReportViewer1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat laporan: " + ex.Message);
            }
        }

        private void Form4_Load(object sender, EventArgs e)
        {
           
        }

        private void crystalReportViewer2_Load(object sender, EventArgs e)
        {

        }
    }
}