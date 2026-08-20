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


        //==========================================================
        // REGISTER
        //==========================================================

        [HttpPost]
        public IActionResult Register(
            string fullName,
            string email,
            string password)
        {
            User user = new User();

            bool success = user.Register(
                _db,
                fullName,
                email,
                password,
                out string message);

            return Json(new
            {
                success,
                message
            });
        }


        //==========================================================
        // LOGIN
        //==========================================================

        [HttpPost]
        public IActionResult Login(
            string email,
            string password)
        {
            User user = new User();

            bool success = user.Login(
                _db,
                email,
                password,
                out int id,
                out string fullName,
                out string message);

            if (success)
            {
                HttpContext.Session.SetInt32("UserId", id);
                HttpContext.Session.SetString("FullName", fullName);
            }

            return Json(new
            {
                success,
                message
            });
        }
        [HttpGet]
        public IActionResult GetUsers()
        {
            try
            {
                using (SqlConnection con = _db.Connection())
                {
                    con.Open();

                    string query = @"
                SELECT Id, FullName, Email
                FROM Users";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<object> users = new List<object>();

                        while (reader.Read())
                        {
                            users.Add(new
                            {
                                id = Convert.ToInt32(reader["Id"]),
                                fullName = reader["FullName"].ToString(),
                                email = reader["Email"].ToString()
                            });
                        }

                        return Json(new
                        {
                            success = true,
                            data = users
                        });
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

        //==========================================================
        // LOGOUT
        //==========================================================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    }
}