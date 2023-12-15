using SpaceIQ.ViewModels;
using System;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using System.Data.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SpaceIQ.Controllers
{
    public class AccountController : Controller
    {
        private readonly SpaceIQDBEntities spaceIQDB = new SpaceIQDBEntities();
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel loginView, string returnUrl)
        {  
                if (ModelState.IsValid)
                {
                    var user = spaceIQDB.Employees.Where(m => m.Email == loginView.Username).FirstOrDefault();
                    if (user == null)
                    {
                        ModelState.AddModelError("Invalid", "Invalid Credentials");
                        return View();
                    }

                    ObjectResult<Byte[]> hashedPassword = spaceIQDB.GetEncryptedPassword(loginView.Password);

                    
                    Byte[] expectedPassword = user.Password;
                    Byte[] enteredpassword = hashedPassword.FirstOrDefault();
                    if (expectedPassword.SequenceEqual(enteredpassword))
                    {
                    Session["username"] = user.EmpName.ToString();
                    FormsAuthentication.SetAuthCookie(loginView.Username, false);
                        string username = Session["username"].ToString();
                        if (returnUrl != null)
                        {
                            return Redirect(returnUrl);
                        }
                        else
                        {
                            return Redirect("~/Home/Index");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Invalid", "Invalid Credentials");
                        return View();
                    }
                }
           
            return View();
        }


        [Authorize]
        public ActionResult Logout()
        {
            Session.Abandon();
            Session.RemoveAll();
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.AppendCacheExtension("no-store, must-revalidate");
            Response.AppendHeader("Pragma", "no-cache");
            Response.AppendHeader("Expires", "0");
            return View();
        }


        [Authorize]
        [HttpPost]
        public ActionResult Logout(FormCollection coll)
        {
            //FormsAuthentication.SignOut();
            Session.Abandon();
            Session.RemoveAll();
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.AppendCacheExtension("no-store, must-revalidate");
            Response.AppendHeader("Pragma", "no-cache");
            Response.AppendHeader("Expires", "0");
            return View();
        }


        [Authorize]
        public ActionResult LoginProfile()
        {
          
                if (Session["Username"] != null)
                {
                    string username = Session["Username"].ToString();
                    var employee = spaceIQDB.Employees.Where(m => m.EmpName == username).FirstOrDefault();

                    return View(employee);
                }
            return View();   
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            string action = filterContext.RouteData.Values["action"].ToString();
            Exception e = filterContext.Exception;
            filterContext.ExceptionHandled = true;
            filterContext.Result = new ViewResult()
            {
                ViewName = "Error"
            };
        }
    }
}