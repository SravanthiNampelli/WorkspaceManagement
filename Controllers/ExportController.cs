using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SpaceIQ.Controllers
{
    public class ExportController : Controller
    {
      
        [HttpPost]
        public FileResult Export()
        {
            SpaceIQDBEntities spaceIQDB = new SpaceIQDBEntities();
            string notAssigned= "Not yet assigned";
            List<object> employees =  (from employee in spaceIQDB.Employees.ToList().Take(10)
                                                   select new object[]
                                                   {
                  employee.EmpId.ToString(),
                  employee.EmpName,
                  employee.Email,
                  employee.Workspace != null ? employee.Workspace.WorkspaceNumber : notAssigned
                                                   }).ToList<object>();

            employees.Insert(0, new string[4] { "Employee Id", "Employee Name", "Email", "Workspace Number" });

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < employees.Count; i++)
            {
                if (employees[i] is object[] employee)
                {
                    for (int j = 0; j < employee.Length; j++)
                    {
                        sb.Append(employee[j].ToString() + ',');
                    }
                    sb.Append("\r\n");
                }   
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "EmployeeWorkspaceDetails.csv");
        }
    }
}