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



        // ==========================
        // REGISTER
        // ==========================

        [HttpPost]
        public JsonResult Register(
            string fullName,
            string email,
            string password)
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
        public JsonResult Login(
            string email,
            string password)
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

                HttpContext.Session.SetInt32(
                    "UserId",
                    id);


                HttpContext.Session.SetString(
                    "FullName",
                    fullName);


                HttpContext.Session.SetString(
                    "Email",
                    email);

            }



            return Json(new
            {
                success,
                message
            });

        }
        // ==========================
        // UNIVERSAL SELECT
        //
        // Used for any table
        //
        // ==========================

        [HttpGet]
        public JsonResult Select(
            string table,
            string[] columns)
        {
            User user = new User();

            string message;

            var data = user.Select(
                _db,
                table,
                columns,
                out message);

            return Json(new
            {
                success = true,
                message,
                data
            });
        }
        // ==========================
        // UNIVERSAL DELETE
        //
        // Used for any table
        //
        // ==========================

        [HttpPost]
        public JsonResult Delete(
            string table,
            string whereColumn,
            string id)
        {
            User user = new User();

            string message;

            bool success = user.Delete(
                _db,
                table,
                whereColumn,
                id,
                out message);

            return Json(new
            {
                success,
                message
            });
        }
        // ==========================
        // UNIVERSAL UPDATE
        //
        // Used for any table
        //
        // ==========================

        [HttpPost]
        public JsonResult Update(
            string table,
            string[] columns,
            string[] values,
            string whereColumn,
            string id)
        {
            User user = new User();

            string message;

            bool success = user.Update(
                _db,
                table,
                columns,
                values,
                whereColumn,
                id,
                out message);

            return Json(new
            {
                success,
                message
            });
        }




        // ==========================
        // UNIVERSAL INSERT
        //
        // Used for any table
        //
        // ==========================


        [HttpPost]
        public JsonResult Insert(
            string table,
            string[] columns,
            string[] values)
        {

            User user = new User();


            string message;



            bool success = user.Insert(
                _db,
                table,
                columns,
                values,
                out message);



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


            return RedirectToAction(
                "Login",
                "Home");

        }

    }
}