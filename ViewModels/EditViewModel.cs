using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SpaceIQ.ViewModels
{
    public class EditViewModel
    {
        public int EmpId { get; set; }
        public string EmpName { get; set; }
        public int WorkspaceId { get; set; }
        public string WorkspaceNumber { get; set; }

       
        [Range(100, Double.MaxValue, ErrorMessage = "Please select a workspace")]
        public string SelectedWorkspace { get; set; }
      
        public IEnumerable<SelectListItem> WorkspaceOptions { get; set; }

    }

}
