//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SpaceIQ
{
    using System;
    using System.Collections.Generic;
    
    public partial class Workspace
    {
        public Workspace()
        {
            this.Employees = new HashSet<Employee>();
        }
    
        public int WorkspaceId { get; set; }
        public string WorkspaceNumber { get; set; }
        public int IsOccupied { get; set; }
        public int CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
    
        public virtual ICollection<Employee> Employees { get; set; }
    }
}
