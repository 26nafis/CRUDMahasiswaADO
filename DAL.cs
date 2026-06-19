using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CRUDMahasiswaADO
{
    public class DAL
    {
        private string connectionString = "Data Source=NAFIS\\NAFISCOY;Initial Catalog=DBAkademikADO;User ID=sa;Password=PasswordSA;";

        // ✅ HITUNG MAHASISWA
        public int CountMhs()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_CountMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter output = new SqlParameter("@pCount", SqlDbType.Int);
                output.Direction = ParameterDirection.Output;

                cmd.Parameters.Add(output);

                conn.Open();
                cmd.ExecuteNonQuery();

                return Convert.ToInt32(output.Value);
            }
        }

        // ✅ GET DATA MAHASISWA
        public DataTable GetMhs()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_GetMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        // ✅ INSERT
        public void InsertMhs(string nim, string nama, string alamat, string jk, DateTime tgl, string kodeProdi, byte[] foto)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_InsertMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@pNIM", nim);
                cmd.Parameters.AddWithValue("@pNama", nama);
                cmd.Parameters.AddWithValue("@pAlamat", alamat);
                cmd.Parameters.AddWithValue("@pJenisKelamin", jk);
                cmd.Parameters.AddWithValue("@pTanggalLahir", tgl);
                cmd.Parameters.AddWithValue("@pKodeProdi", kodeProdi);
                cmd.Parameters.AddWithValue("@pFoto", (object)foto ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ✅ UPDATE
        public void UpdateMhs(string nim, string nama, string alamat, string jk, DateTime tgl, string kodeProdi, byte[] foto)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_UpdateMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@pNIM", nim);
                cmd.Parameters.AddWithValue("@pNama", nama);
                cmd.Parameters.AddWithValue("@pAlamat", alamat);
                cmd.Parameters.AddWithValue("@pJenisKelamin", jk);
                cmd.Parameters.AddWithValue("@pTanggalLahir", tgl);
                cmd.Parameters.AddWithValue("@pKodeProdi", kodeProdi);
                cmd.Parameters.AddWithValue("@pFoto", (object)foto ?? DBNull.Value);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ✅ DELETE
        public void DeleteMhs(string nim)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_DeleteMahasiswa", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@pNIM", nim);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ✅ REPORT
        public DataTable getDataRekap(string prodi, DateTime tgl)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Report", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@inProdi", prodi);
                cmd.Parameters.AddWithValue("@inTglMsuk", tgl.Year.ToString());

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        // ✅ LOG ERROR
        public void InsertLog(string message)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_LogMessage", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@psn", message);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ✅ DASHBOARD (SEMUA DATA)
        public DataTable getAllDataChart()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_DashBoard", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        // ✅ DASHBOARD BY TAHUN
        public DataTable getDataChartByTahun(DateTime tgl)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_DashBoardByTahun", conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@inTglMsuk", tgl.Year);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        public static string GetLocalIPAddress()
        {
            string localIP = string.Empty;

            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());

                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error getting local IP address: " + ex.Message);
            }

            return localIP;
        }
    }
}
