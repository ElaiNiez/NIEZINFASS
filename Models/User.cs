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
        // BUILD SQL STATEMENT
        //
        // Dynamically builds INSERT, SELECT,
        // UPDATE, and DELETE queries.
        //
        // This method is reused by all CRUD methods.
        //
        //==========================================================

        private string BuildStatement(
            string action,
            string table,
            string[] columns,
            string[] values,
            string whereColumn = "")
        {
            string sql = "";

            switch (action.ToUpper())
            {
                //==================================================
                // INSERT
                //==================================================

                case "INSERT":

                    sql += "INSERT INTO " + table + " (";

                    for (int i = 0; i < columns.Length; i++)
                    {
                        sql += columns[i];

                        if (i < columns.Length - 1)
                            sql += ",";
                    }

                    sql += ") VALUES (";

                    for (int i = 0; i < values.Length; i++)
                    {
                        sql += "@value" + i;

                        if (i < values.Length - 1)
                            sql += ",";
                    }

                    sql += ")";

                    break;

                //==================================================
                // SELECT
                //==================================================

                case "SELECT":

                    sql += "SELECT ";

                    if (columns.Length == 0)
                    {
                        sql += "*";
                    }
                    else
                    {
                        for (int i = 0; i < columns.Length; i++)
                        {
                            sql += columns[i];

                            if (i < columns.Length - 1)
                                sql += ",";
                        }
                    }

                    sql += " FROM " + table;

                    break;

                //==================================================
                // UPDATE
                //==================================================

                case "UPDATE":

                    sql += "UPDATE " + table + " SET ";

                    for (int i = 0; i < columns.Length; i++)
                    {
                        sql += columns[i] + "=@value" + i;

                        if (i < columns.Length - 1)
                            sql += ",";
                    }

                    sql += " WHERE " + whereColumn + "=@id";

                    break;

                //==================================================
                // DELETE
                //==================================================

                case "DELETE":

                    sql +=
                        "DELETE FROM " +
                        table +
                        " WHERE " +
                        whereColumn +
                        "=@id";

                    break;
            }

            return sql;
        }//==========================================================
         // EXECUTE NON QUERY
         //
         // Used by:
         //      INSERT
         //      UPDATE
         //      DELETE
         //
         //==========================================================

        private bool ExecuteNonQuery(
            Db db,
            string query,
            string[] parameterNames,
            string[] values,
            out string message)
        {
            try
            {
                using (SqlConnection con = OpenConnection(db))
                {
                    SqlCommand cmd =
                        new SqlCommand(query, con);

                    AddParameters(
                        cmd,
                        parameterNames,
                        values);

                    cmd.ExecuteNonQuery();

                    message = query;

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
        // INSERT
        //
        // Works for ANY TABLE.
        //
        //==========================================================

        public bool Insert(
            Db db,
            string table,
            string[] columns,
            string[] values,
            out string message)
        {
            string query =
                BuildStatement(
                    "INSERT",
                    table,
                    columns,
                    values);

            string[] names =
                new string[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                names[i] =
                    "@value" + i;
            }

            return ExecuteNonQuery(
                db,
                query,
                names,
                values,
                out message);
        }
        //==========================================================
        // UPDATE
        //
        // Works for ANY TABLE.
        //
        //==========================================================

        //==========================================================
        // UPDATE USER
        //==========================================================
        public bool UpdateUser(
            string id,
            out string message)
        {
            string[] columns =
            {
        "FullName",
        "Email",
        "Password"
    };

            string[] values =
            {
        "John Doe",
        "john@gmail.com",
        "123456"
    };

            message = BuildStatement(
                "UPDATE",
                "Users",
                columns,
                values,
                "Id");

            message = message.Replace("@id", id);

            return true;
        }
        //==========================================================
        // DELETE
        //
        // Works for ANY TABLE.
        //
        //==========================================================

        //==========================================================
        // DELETE USER
        //==========================================================
        public bool DeleteUser(
            string id,
            out string message)
        {
            message = BuildStatement(
                "DELETE",
                "Users",
                new string[0],
                new string[0],
                "Id");

            message = message.Replace("@id", id);

            return true;
        }
        //==========================================================
        // VIEW USERS
        //==========================================================
        public bool ViewUsers(
            out string message)
        {
            message = BuildStatement(
                "SELECT",
                "Users",
                new string[0],
                new string[0]);

            return true;
        }
        //==========================================================
        // SELECT
        //
        // Works for ANY TABLE.
        //
        //==========================================================

        public List<Dictionary<string, object>> Select(
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
                    string query =
                        BuildStatement(
                            "SELECT",
                            table,
                            columns,
                            new string[0]);

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

            if (!ValidateFields(values, fields, out message))
            {
                return false;
            }

            if (Exists(db, "Users", "Email", email))
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

            //==================================================
            // ACTUAL INSERT
            //==================================================

            /*
            if (!Insert(
                db,
                "Users",
                columns,
                values,
                out message))
            {
                return false;
            }
            */

            //==================================================
            // SHOW INSERT QUERY
            //==================================================

            message = BuildStatement(
                "INSERT",
                "Users",
                columns,
                values);

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