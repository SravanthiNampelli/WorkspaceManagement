using SpaceIQ.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using System.Net.Mail;
using System.Net;

namespace SpaceIQ.Controllers
{
    public class HomeController : Controller
    {
        private readonly SpaceIQDBEntities spaceIQDB = new SpaceIQDBEntities();

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }
        
        [Authorize]
        public ActionResult Dashboard(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParam = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "name";
            ViewBag.IdSortParam = sortOrder == "id" ? "id_desc" : "id";
            ViewBag.WorkspaceIdSortParam = sortOrder == "wsid" ? "wsid_desc" : "wsid";
            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            var employees = from s in spaceIQDB.Employees
                            select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(s => s.EmpName.Contains(searchString));
                if (!employees.Any())
                {
                    ViewData["NoDataFound"] = "No Matching Results Found";
                }
            }

            switch (sortOrder)
            {
                case "name_desc":
                    employees = employees.OrderByDescending(s => s.EmpName);
                    break;
                case "name":
                    employees = employees.OrderBy(s => s.EmpName);
                    break;
                case "id_desc":
                    employees = employees.OrderByDescending(s => s.EmpId);
                    break;
                case "id":
                    employees = employees.OrderBy(s => s.EmpId);
                    break;
                case "wsid_desc":
                    employees = employees.OrderByDescending(s => s.WorkspaceId);
                    break;
                case "wsid":
                    employees = employees.OrderBy(s => s.WorkspaceId);
                    break;

                default:
                    employees = employees.OrderBy(s => s.WorkspaceId);
                    break;
            }
            int pageSize = 4;
            int pageNumber = (page ?? 1);
            return View(employees.ToPagedList(pageNumber, pageSize));


        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int empid)
        {
            using (var spaceIQDB = new SpaceIQDBEntities())
            {
                var employeeDetails = spaceIQDB.Employees
                    .Where(e => e.EmpId == empid)
                    .Select(e => new EditViewModel
                    {
                        EmpId = e.EmpId,
                        EmpName = e.EmpName,
                    })
                    .FirstOrDefault();
                int isOccupied = 0;
                //var workspacenumber = from e in spaceIQDB.Employees
                //                      join w in spaceIQDB.Workspaces on e.WorkspaceId equals w.WorkspaceId
                //                      where e.EmpId == empid
                //                      select w.WorkspaceNumber;

                var workspacenumber = spaceIQDB.Employees
                        .Where(e => e.EmpId == empid)
                        .Join(
                         spaceIQDB.Workspaces,
                         e => e.WorkspaceId,
                         w => w.WorkspaceId,
                         (e, w) => w.WorkspaceNumber )
                         .FirstOrDefault();

                var workspaceOptions = spaceIQDB.Workspaces.AsEnumerable()
                    .Where(w => w.IsOccupied == isOccupied)
                    .Select(w => new SelectListItem
                    {
                        Value = w.WorkspaceId.ToString(),
                        Text = w.WorkspaceNumber
                    })
                    .ToList();

                var viewModel = new EditViewModel
                {
                    EmpId = employeeDetails.EmpId,
                    EmpName = employeeDetails.EmpName,
                    WorkspaceOptions = workspaceOptions,
                    WorkspaceNumber = workspacenumber
                };
                return View(viewModel);
            }
        }

        [Authorize(Roles ="Admin")]
        [HttpPost]
        public ActionResult Edit(EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                int empId = model.EmpId;
                int workspaceId = int.Parse(model.SelectedWorkspace);
                var employee = spaceIQDB.Employees.Where(e => e.EmpId == empId).FirstOrDefault();
                var workspace = spaceIQDB.Workspaces.Where(e => e.WorkspaceId == workspaceId).FirstOrDefault();
                if (employee != null && workspace != null)
                {
                    if (employee.WorkspaceId != null)
                    {
                        int previousWorkspaceId = (int)employee.WorkspaceId;
                        var previousWorkspace = spaceIQDB.Workspaces.Where(e => e.WorkspaceId == previousWorkspaceId).FirstOrDefault();
                        previousWorkspace.IsOccupied = 0;
                    }
                    employee.WorkspaceId = workspaceId;
                    workspace.IsOccupied = 1;
                    spaceIQDB.SaveChanges();
                    //Session["Email"] = employee.Email.ToString();
                    string email = employee.Email.ToString();
                    SendEmail(employee.EmpName, workspace.WorkspaceNumber,email);
                    TempData["SuccessMessage"] = "Successfully edited the workspace!";
                    return RedirectToAction("Edit", new { empid = empId });
                }
            }
            return View();
        }

        [NonAction]
        public void SendEmail(string username, string wsid,string email)
        {
            string receiver = email;
            string subject = "New Workspace Allocated";
            string message = $"Dear {username},\n\nCongratulations! You've been allocated a new workspace.\n\nDetails:\n- Workspace Number: [{wsid}]\n- Location: ZELIS INDIA [ Aurobindo Galaxy, A Block, Floor 19, Hyderabad, Telangana ]\n\nLet me know if you have any questions.\n\nRegards,\nSatishwar Amuraji";
            try
            {
                if (ModelState.IsValid)
                {
                    var senderEmail = new MailAddress("sravanthinampelli123@gmail.com", "SpaceIQ");
                    var receiverEmail = new MailAddress(receiver, "Receiver");
                    var password = "ibwo shrv zjxk amyn";
                    var sub = subject;
                    var body = message;
                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderEmail.Address, password)
                    };
                    using (var mess = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = subject,
                        Body = body
                    })
                    {
                        smtp.Send(mess);
                    }
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Some Error";
            }            
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Assign(int empid)
        {
            using (var spaceIQDB = new SpaceIQDBEntities())
            {
                var employeeDetails = spaceIQDB.Employees
                    .Where(e => e.EmpId == empid)
                    .Select(e => new EditViewModel
                    {
                        EmpId = e.EmpId,
                        EmpName = e.EmpName,

                    })
                    .FirstOrDefault();
                int isOccupied = 0;
                var workspaceOptions = spaceIQDB.Workspaces.AsEnumerable()
                    .Where(w => w.IsOccupied == isOccupied)
                    .Select(w => new SelectListItem
                    {
                        Value = w.WorkspaceId.ToString(),
                        Text = w.WorkspaceNumber
                    })
                    .ToList();
                var viewModel = new EditViewModel
                {
                    EmpId = employeeDetails.EmpId,
                    EmpName = employeeDetails.EmpName,
                    WorkspaceNumber = employeeDetails.WorkspaceNumber,
                    WorkspaceOptions = workspaceOptions
                };
                return View(viewModel);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Assign(EditViewModel model)
        {
            if (ModelState.IsValid)
            {
                int empid = model.EmpId;
                int wsid = int.Parse(model.SelectedWorkspace);
                var employee = spaceIQDB.Employees.Where(e => e.EmpId == empid).FirstOrDefault();
                var workspace = spaceIQDB.Workspaces.Where(e => e.WorkspaceId == wsid).FirstOrDefault();
                if (employee != null && workspace != null)
                {
                    employee.WorkspaceId = wsid;
                    workspace.IsOccupied = 1;
                    employee.ModifiedBy = 1005;
                    employee.ModifiedDate = DateTime.Now;
                    spaceIQDB.SaveChanges();
                    //Session["Email"] = employee.Email.ToString();
                    string email = employee.Email.ToString();
                    SendEmail(employee.EmpName, workspace.WorkspaceNumber,email);
                    TempData["SuccessMessage"] = "Successfully assigned workspace!";
                    return RedirectToAction("Edit", new { empid = empid });
                }
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
