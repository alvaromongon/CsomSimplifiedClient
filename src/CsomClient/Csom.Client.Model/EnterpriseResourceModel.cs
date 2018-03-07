using System;
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;

namespace Csom.Client.Model
{
    /// <summary>
    /// Define an enterprise resource object
    /// https://msdn.microsoft.com/en-us/office/project/api/enterpriseresource
    /// </summary>
    public class EnterpriseResourceModel
    {
        // Basic properties
        public Guid Id { get; set; }        
        public bool IsBudget { get; set; }        
        public bool IsGeneric { get; set; }        
        public bool IsInactive { get; set; }        
        public string Name { get; set; }        
        public EnterpriseResourceType ResourceType { get; set; }

        // Extended properties    

        /// <summary>
        /// Gets or sets any code, abbreviation, or number that is entered as an external identifier for an enterprise resource. 
        /// </summary>
        public string ExternalId { get; set; }

        /// <summary>
        /// Gets or sets the abbreviation for the name of an enterprise resource. 
        /// </summary>
        public string Initials { get; set; }

        /// <summary>
        /// Gets or sets the email address of an enterprise resource. 
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the name of a group to which an enterprise resource belongs.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Gets or sets any code, abbreviation, or number that is entered as part of the information about an enterprise resource. 
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the default booking type for an enterprise resource. 
        /// </summary>
        public BookingType DefaultBookingType { get; set; }

        /// <summary>
        /// Gets or sets the unit of measure that is entered for supplies or other consumable items that are used to complete tasks in a project. 
        /// </summary>
        public string MaterialLabel { get; set; }

        /// <summary>
        /// Gets or sets the date and time of hire for an enterprise resource. 
        /// </summary>
        public DateTime HireDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time after which the resource can no longer be used. 
        /// </summary>
        public DateTime TerminationDate { get; set; }

        /// <summary>
        /// Gets or sets any code, abbreviation, or number that is entered as cost center information for an enterprise resource. 
        /// </summary>
        public string CostCenter { get; set; }

        /// <summary>
        /// Gets or sets a value that represents how and when to charge enterprise resource costs to the cost of a task. 
        /// </summary>
        public AccrueAt CostAccrual { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether resource leveling can be performed on an enterprise resource. 
        /// </summary>
        public bool CanLevel { get; set; }

        //public EnterpriseResourceCostRateTableCollection CostRateTables { get; }
        /// <summary>
        /// Simplification of the cost rate table collections.
        /// Gets or sets the Cost rate for the default cost rate table.
        /// NOTE: Value will be assign as the first line of the default cost rate table
        /// </summary>
        public CostRateCreationInformation CostRate { get; set; }

        /// <summary>
        /// Gets the default name that is entered into the Assignment Owner field when an enterprise resource is assigned to a task.
        /// </summary>
        //public User DefaultAssignmentOwner { get; set; }

        /// <summary>
        /// Gets the SharePoint user that is linked to the Enterprise Resource.
        /// </summary>
        //public User User { get; set; }        

        #region TODO: Check out if commented properies make sense here

        //public User TimesheetManager { get; set; }        
        //public Calendar BaseCalendar { get; set; }                                              
        //public ResourceEngagementCollection Engagements { get; }
        //public StatusAssignmentCollection Assignments { get; }
        //public string Phonetics { get; set; }
        //public bool RequiresEngagements { get; set; }

        #endregion
    }
}
