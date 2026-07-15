using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NIEZ.Models;
using NIEZ.Service;

namespace NIEZ.Controllers
{
    public class AccountController : Controller
    {
        private readonly Db _db;

        public AccountController(Db db)
        {
            _db = db;
        }

        // ==========================
        // REGISTER
        // ==========================
        [HttpPost]
        public JsonResult Register(User user)
        {
            try
            {
                using (SqlConnection con = _db.Connection())
                {
                    con.Open();

                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email=@Email";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", user.Email);

                        int count = (int)checkCmd.ExecuteScalar();

                        if (count > 0)
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Email already exists."
                            });
                        }
                    }

                    string insertQuery = @"INSERT INTO Users
                                          (FullName,Email,Password)
                                          VALUES
                                          (@FullName,@Email,@Password)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@FullName", user.FullName);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Password", user.Password);

                        cmd.ExecuteNonQuery();
                    }

                    return Json(new
                    {
                        success = true,
                        message = "Registration Successful!"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ==========================
        // LOGIN
        // ==========================
        [HttpPost]
        public JsonResult Login(User user)
        {
            try
            {
                using (SqlConnection con = _db.Connection())
                {
                    con.Open();

                    string query = @"SELECT Id,
                                            FullName,
                                            Email
                                     FROM Users
                                     WHERE Email=@Email
                                     AND Password=@Password";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Password", user.Password);

                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            HttpContext.Session.SetInt32("UserId",
                                Convert.ToInt32(reader["Id"]));

                            HttpContext.Session.SetString("FullName",
                                reader["FullName"].ToString());

                            HttpContext.Session.SetString("Email",
                                reader["Email"].ToString());

                            return Json(new
                            {
                                success = true,
                                message = "Login Successful!"
                            });
                        }
                        else
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Invalid Email or Password."
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ==========================
        // LOGOUT
        // ==========================
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Home");
        }
    }
}