using Microsoft.Data.SqlClient;
using NIEZ.Service;

namespace NIEZ.Models
{
    public class User
    {
        // REGISTER
        public bool Register(
            Db db,
            string fullName,
            string email,
            string password,
            out string message)
        { 
            try
            { 
                using (SqlConnection con = db.Connection())
                {
                    con.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email=@Email";

                    SqlCommand checkCmd = new SqlCommand(checkQuery, con);
                    checkCmd.Parameters.AddWithValue("@Email", email);

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        message = "Email already exists.";
                        return false;
                    }

                    string insertQuery = @"INSERT INTO Users
                                           (FullName, Email, Password)
                                           VALUES
                                           (@FullName, @Email, @Password)";

                    SqlCommand cmd = new SqlCommand(insertQuery, con);

                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    cmd.ExecuteNonQuery();

                    message = "Registration Successful!";
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        // LOGIN
        public bool Login(
            Db db,
            string email,
            string password,
            out int id,
            out string fullName,
            out string message)
        {
            id = 0;
            fullName = "";

            try
            {
                using (SqlConnection con = db.Connection())
                {
                    con.Open();

                    string query = @"SELECT Id, FullName, Email
                                     FROM Users
                                     WHERE Email=@Email
                                     AND Password=@Password";

                    SqlCommand cmd = new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        id = Convert.ToInt32(reader["Id"]);
                        fullName = reader["FullName"].ToString();
                        message = "Login Successful!";
                        return true;
                    }

                    message = "Invalid Email or Password.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }
    }
}