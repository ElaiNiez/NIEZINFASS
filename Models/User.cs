using Microsoft.Data.SqlClient;
using NIEZ.Service;

namespace NIEZ.Models
{
    public class User
    {


        //==========================================================
        // DYNAMIC VALIDATION
        //
        // Works with any number of fields
        //
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

                    message =
                    fields[i] + " is required.";

                    return false;

                }

            }


            message = "";

            return true;

        }





        //==========================================================
        // DYNAMIC SQL PREVIEW
        //
        // Works for any table
        //
        //==========================================================


        private string BuildInsertStatement(
            string table,
            string[] columns,
            string[] values)
        {

            string sql = "";



            sql += "INSERT INTO " + table + "\n";


            sql += "(\n";



            // DISPLAY COLUMNS

            for (int i = 0; i < columns.Length; i++)
            {

                sql += "    " + columns[i];


                if (i < columns.Length - 1)
                {
                    sql += ",";
                }


                sql += "\n";

            }



            sql += ")\n";


            sql += "VALUES\n";


            sql += "(\n";



            // DISPLAY VALUES

            for (int i = 0; i < values.Length; i++)
            {

                sql += "    '" + values[i] + "'";


                if (i < values.Length - 1)
                {
                    sql += ",";
                }


                sql += "\n";

            }



            sql += ");";



            return sql;

        }


        public bool Insert(
            Db db,
            string table,
            string[] columns,
            string[] values,
            out string message)
        {


            try
            {

                using (SqlConnection con = db.Connection())
                {

                    con.Open();



                    string query =
                    "INSERT INTO " + table + " (";




                    // CREATE COLUMN PART

                    for (int i = 0; i < columns.Length; i++)
                    {

                        query += columns[i];


                        if (i < columns.Length - 1)
                        {
                            query += ",";
                        }

                    }



                    query += ") VALUES (";




                    // CREATE PARAMETER PART

                    for (int i = 0; i < values.Length; i++)
                    {

                        query += "@value" + i;


                        if (i < values.Length - 1)
                        {
                            query += ",";
                        }

                    }



                    query += ")";




                    SqlCommand cmd =
                    new SqlCommand(query, con);




                    // ADD VALUES

                    for (int i = 0; i < values.Length; i++)
                    {

                        cmd.Parameters.AddWithValue(
                            "@value" + i,
                            values[i]);

                    }




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




            try
            {

                using (SqlConnection con = db.Connection())
                {

                    con.Open();



                    string check =
                    "SELECT COUNT(*) FROM Users WHERE Email=@Email";



                    SqlCommand checkCmd =
                    new SqlCommand(check, con);



                    checkCmd.Parameters.AddWithValue(
                        "@Email",
                        email);



                    int count =
                    Convert.ToInt32(
                    checkCmd.ExecuteScalar());



                    if (count > 0)
                    {

                        message =
                        "Email already exists.";

                        return false;

                    }



                }



                // USE UNIVERSAL INSERT METHOD

                string[] columns =
                {
                    "FullName",
                    "Email",
                    "Password"
                };



                bool result =
                Insert(
                    db,
                    "Users",
                    columns,
                    values,
                    out message);



                if (result)
                {

                    message +=
                    "\n\nRegistration Successful!";

                }



                return result;


            }
            catch (Exception ex)
            {

                message = ex.Message;

                return false;

            }

        }


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



            using (SqlConnection con = db.Connection())
            {

                con.Open();



                string query =
                @"SELECT Id,FullName
                  FROM Users
                  WHERE Email=@Email
                  AND Password=@Password";



                SqlCommand cmd =
                new SqlCommand(query, con);



                cmd.Parameters.AddWithValue(
                    "@Email",
                    email);



                cmd.Parameters.AddWithValue(
                    "@Password",
                    password);



                SqlDataReader reader =
                cmd.ExecuteReader();



                if (reader.Read())
                {

                    id =
                    Convert.ToInt32(
                    reader["Id"]);



                    fullName =
                    reader["FullName"].ToString();



                    message =
                    "Login Successful!";


                    return true;

                }
                message =
                "Invalid Email or Password.";


                return false;

            }

        }


    }
}