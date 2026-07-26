using Microsoft.Data.SqlClient;
using NIEZ.Service;

namespace NIEZ.Models
{
    public class User
    {
        //==========================================================
        // OPEN CONNECTION
        //==========================================================
        private SqlConnection OpenConnection(Db db)
        {
            SqlConnection con = db.Connection();
            con.Open();
            return con;
        }
        //==========================================================
        // ADD PARAMETERS
        //==========================================================
        private void AddParameters(
            SqlCommand cmd,
            string[] names,
            string[] values)
        {
            for (int i = 0; i < names.Length; i++)
            {
                cmd.Parameters.AddWithValue(
                    names[i],
                    values[i]);
            }
        }
        //==========================================================
        // VALIDATE FIELDS
        //==========================================================
        private bool ValidateFields(
            string[] values,
            string[] fields,
            out string message)
        {
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
        // CHECK IF RECORD EXISTS
        //==========================================================
        private bool Exists(
            Db db,
            string table,
            string column,
            string value)
        {
            using (SqlConnection con = OpenConnection(db))
            {
                string query =
                    $"SELECT COUNT(*) FROM {table} WHERE {column}=@Value";

                SqlCommand cmd =
                    new SqlCommand(query, con);

                cmd.Parameters.AddWithValue(
                    "@Value",
                    value);

                return Convert.ToInt32(
                    cmd.ExecuteScalar()) > 0;
            }
        }
        //==========================================================
        // BUILD INSERT PREVIEW
        //==========================================================
        private string BuildInsertStatement(
            string table,
            string[] columns,
            string[] values)
        {
            string sql = "";

            sql += "INSERT INTO " + table + "\n";
            sql += "(\n";

            for (int i = 0; i < columns.Length; i++)
            {
                sql += "    " + columns[i];

                if (i < columns.Length - 1)
                    sql += ",";

                sql += "\n";
            }
            sql += ")\nVALUES\n(\n";

            for (int i = 0; i < values.Length; i++)
            {
                sql += "    '" + values[i] + "'";

                if (i < values.Length - 1)
                    sql += ",";

                sql += "\n";
            }

            sql += ");";

            return sql;
        }
        //==========================================================
        // UNIVERSAL INSERT
        //==========================================================

        public bool Insert(
            Db db,
            string table,
            string[] columns,
            string[] values,
            out string message)
        {
            try
            {
                using (SqlConnection con = OpenConnection(db))
                {
                    string query =
                        "INSERT INTO " + table + " (";

                    for (int i = 0; i < columns.Length; i++)
                    {
                        query += columns[i];

                        if (i < columns.Length - 1)
                            query += ",";
                    }

                    query += ") VALUES (";

                    string[] parameterNames =
                        new string[values.Length];

                    for (int i = 0; i < values.Length; i++)
                    {
                        parameterNames[i] =
                            "@value" + i;

                        query += parameterNames[i];

                        if (i < values.Length - 1)
                            query += ",";
                    }

                    query += ")";

                    SqlCommand cmd =
                        new SqlCommand(query, con);

                    AddParameters(
                        cmd,
                        parameterNames,
                        values);

                    cmd.ExecuteNonQuery();

                    message =
                        BuildInsertStatement(
                            table,
                            columns,
                            values);

                    return true;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }
        public List<Dictionary<string, object>> View(
         Db db,
         string table,
         string[] columns,
          out string message)
        {
            List<Dictionary<string, object>> rows =
                new List<Dictionary<string, object>>();

            message = "";

            try
            {
                using (SqlConnection con = OpenConnection(db))
                {
                    string query = "SELECT ";

                    if (columns.Length == 0)
                    {
                        query += "*";
                    }
                    else
                    {
                        for (int i = 0; i < columns.Length; i++)
                        {
                            query += columns[i];

                            if (i < columns.Length - 1)
                                query += ",";
                        }
                    }

                    query += " FROM " + table;

                    SqlCommand cmd =
                        new SqlCommand(query, con);

                    SqlDataReader reader =
                        cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Dictionary<string, object> row =
                            new Dictionary<string, object>();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(
                                reader.GetName(i),
                                reader.GetValue(i));
                        }

                        rows.Add(row);
                    }

                    message = query;

                    return rows;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return rows;
            }
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
            string[] values =
            {
                fullName,
                email,
                password
            };
            string[] fields =
            {
                "Full Name",
                "Email",
                "Password"
            };
            if (!ValidateFields(
                values,
                fields,
                out message))
            {
                return false;
            }
            if (Exists(
                db,
                "Users",
                "Email",
                email))
            {
                message = "Email already exists.";
                return false;
            }
            string[] columns =
            {
                "FullName",
                "Email",
                "Password"
            };

            if (!Insert(
                db,
                "Users",
                columns,
                values,
                out message))
            {
                return false;
            }

            message += "\n\nRegistration Successful!";
            return true;
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

            string[] values =
            {
                email,
                password
            };

            string[] fields =
            {
                "Email",
                "Password"
            };

            if (!ValidateFields(
                values,
                fields,
                out message))
            {
                return false;
            }

            using (SqlConnection con = OpenConnection(db))
            {
                string query =
                @"SELECT Id, FullName
                  FROM Users
                  WHERE Email=@Email
                  AND Password=@Password";

                SqlCommand cmd =
                    new SqlCommand(query, con);

                AddParameters(
                    cmd,
                    new[]
                    {
                        "@Email",
                        "@Password"
                    },
                    new[]
                    {
                        email,
                        password
                    });

                SqlDataReader reader =
                    cmd.ExecuteReader();

                if (!reader.Read())
                {
                    message = "Invalid Email or Password.";
                    return false;
                }

                id = Convert.ToInt32(
                    reader["Id"]);

                fullName =
                    reader["FullName"].ToString();

                message = "Login Successful!";
                return true;
            }
        }
    }
}