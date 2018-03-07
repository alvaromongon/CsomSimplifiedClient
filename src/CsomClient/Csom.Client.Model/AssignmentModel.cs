using System;
using Microsoft.ProjectServer.Client;

namespace Csom.Client.Model
{
    /// <summary>
    /// Define an assigment object
    /// https://msdn.microsoft.com/en-us/office/project/api/draftassignment
    /// </summary>
    public class AssignmentModel
    {
        // Basic properties
        public Guid Id { get; set; }
        public string Notes { get; set; }

        public Guid ResourceId { get; set; }                       
        public Guid TaskId { get; set; }

        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }

        // Extended properties        

        public bool IsLockedByManager { get; set; }
        public bool IsWorkResource { get; set; }

        public BookingType DefaultBookingType { get; set; }        

        /// <summary>
        /// Units of the resource assigned to the task
        /// </summary>
        public double ResourceCapacity { get; set; }

        public string Delay { get; set; }
        public TimeSpan DelayTimeSpan { get; set; }

        public int? PercentWorkComplete { get; set; }
        public string Work { get; set; }
        public TimeSpan WorkTimeSpan { get; set; }
        public string ActualWork { get; set; }
        public TimeSpan ActualWorkTimeSpan { get; set; }
        public string RegularWork { get; set; }
        public TimeSpan RegularWorkTimeSpan { get; set; }
        public string BudgetedWork { get; set; }
        public TimeSpan BudgetedWorkTimeSpan { get; set; }
        public string RemainingWork { get; set; }
        public TimeSpan RemainingWorkTimeSpan { get; set; }
        public string OvertimeWork { get; set; }
        public TimeSpan OvertimeWorkTimeSpan { get; set; }
        public string ActualOvertimeWork { get; set; }
        public TimeSpan ActualOvertimeWorkTimeSpan { get; set; }
        public string RemainingOvertimeWork { get; set; }
        public TimeSpan RemainingOvertimeWorkTimeSpan { get; set; }


        #region TODO: Check out if commented properies make sense here

        //public CostRateTableName CostRateTable { get; set; } //Our simplification of cost rate will always use the default table

        /// <summary>
        /// Gets the date and time that the assignment actually began, based on progress information that was entered. 
        /// </summary>
        // public DateTime ActualStart { get; set; } READONLY?

        /// <summary>
        /// Gets the date and time when the assignment is complete. 
        /// </summary>
        // public DateTime ActualFinish { get; set; } READONLY?

        /// <summary>
        /// Gets the total scheduled or projected cost for the assignment. 
        /// </summary>
        //public double Cost { get; set; } READONLY?

        /// <summary>
        /// Gets the costs incurred for work already performed on the assignment, together with any other recorded costs that are associated with the assignment. 
        /// </summary>
        //public double ActualCost { get; set; } READONLY?

        /// <summary>
        /// Gets the budgeted cost for the assignment. 
        /// </summary>
        //public double BudgetedCost { get; set; } READONLY?

        //public User Owner { get; set; }
        //public DraftAssignment Parent { get; }
        //public DraftProjectResource Resource { get; }        

        #endregion
    }
}
