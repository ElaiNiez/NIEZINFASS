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
        public JsonResult Register(string fullName, string email, string password)
        {
            User user = new User();

            string message;

            bool success = user.Register(
                _db,
                fullName,
                email,
                password,
                out message);

            return Json(new
            {
                success,
                message
            });
        }

        // ==========================
        // LOGIN
        // ==========================
        [HttpPost]
        public JsonResult Login(string email, string password)
        {
            User user = new User();

            int id;
            string fullName;
            string message;

            bool success = user.Login(
                _db,
                email,
                password,
                out id,
                out fullName,
                out message);

            if (success)
            {
                HttpContext.Session.SetInt32("UserId", id);
                HttpContext.Session.SetString("FullName", fullName);
                HttpContext.Session.SetString("Email", email);
            }

            return Json(new
            {
                success,
                message
            });
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