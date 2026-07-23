using Microsoft.Data.SqlClient;
using NIEZ.Service;

namespace NIEZ.Models
{
    public class User
    {
        //==========================================================
        // DYNAMIC VALIDATION METHOD
        //==========================================================
        private bool ValidateFields(
        string valueString,
        string fieldString,
        out string message)
        {
            string[] values = valueString.Split('|');
            string[] fields = fieldString.Split('|');

            for (int i = 0; i < values.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(values[i]))
                {
                    message = fields[i] + " is required.";
                    return false;
                }
            }

            message = "";
            return true;
        }

        //==========================================================
        // DYNAMIC SQL PREVIEW
        //==========================================================

        private string BuildInsertStatement(
        string table,
        string columns,
        string values)
        {
            string[] columnArray = columns.Split(',');
            string[] valueArray = values.Split(',');


            string sql = "";

            sql += "INSERT INTO " + table + "\n";
            sql += "(\n";


            // COLUMNS
            for (int i = 0; i < columnArray.Length; i++)
            {
                sql += "    " + columnArray[i].Trim();

                if (i < columnArray.Length - 1)
                {
                    sql += ",";
                }

                sql += "\n";
            }


            sql += ")\n";


            sql += "VALUES\n";


            sql += "(\n";


            // VALUES
            for (int i = 0; i < valueArray.Length; i++)
            {
                sql += "    '" + valueArray[i].Trim() + "'";

                if (i < valueArray.Length - 1)
                {
                    sql += ",";
                }

                sql += "\n";
            }


            sql += ");";


            return sql;
        }


        //==========================================================
        // REGISTER
        //==========================================================
        public bool Register(
            Db db,
            string fullName,
            string email,
            string password,
            out string message)
        {


            if (!ValidateFields(
     fullName + "|" + email + "|" + password,
     "Full Name|Email|Password",
     out message))
            {
                return false;
            }

            try
            {
                using (SqlConnection con = db.Connection())
                {
                    con.Open();

                    string checkQuery =
                        "SELECT COUNT(*) FROM Users WHERE Email=@Email";

                    SqlCommand checkCmd =
                        new SqlCommand(checkQuery, con);

                    checkCmd.Parameters.AddWithValue("@Email", email);

                    int count =
                        Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        message = "Email already exists.";
                        return false;
                    }

                    string insertQuery =
                    @"INSERT INTO Users
                    (
                        FullName,
                        Email,
                        Password
                    )
                    VALUES
                    (
                        @FullName,
                        @Email,
                        @Password
                    )";

                    SqlCommand cmd =
                        new SqlCommand(insertQuery, con);

                    cmd.Parameters.AddWithValue("@FullName", fullName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    cmd.ExecuteNonQuery();
                    message = BuildInsertStatement(

    "Users",

    "FullName,Email,Password",

    fullName + "," +
    email + "," +
    password

);


                    message += "\n\nRegistration Successful!";
                    return true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        //==========================================================
        // LOGIN
        //==========================================================
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
            if (!ValidateFields(
                email + "|" + password,
                "Email|Password",
                out message))
            {
                return false;
            }

            try
            {
                using (SqlConnection con = db.Connection())
                {
                    con.Open();

                    string query =
                    @"SELECT Id,
                             FullName
                      FROM Users
                      WHERE Email=@Email
                      AND Password=@Password";

                    SqlCommand cmd =
                        new SqlCommand(query, con);

                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    SqlDataReader reader =
                        cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        id =
                            Convert.ToInt32(reader["Id"]);

                        fullName =
                            reader["FullName"].ToString();

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