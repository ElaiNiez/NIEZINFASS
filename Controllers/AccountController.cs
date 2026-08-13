using Microsoft.AspNetCore.Mvc;
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