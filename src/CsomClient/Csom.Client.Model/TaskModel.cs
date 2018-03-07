using System;
using System.Collections.Generic;
using Microsoft.ProjectServer.Client;

namespace Csom.Client.Model
{
    /// <summary>
    /// Define a task object
    /// </summary>
    public class TaskModel
    {
        // Basic properties
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Notes { get; set; }        

        public Guid ParentId { get; set; }
        public Guid AddAfterId { get; set; }               

        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
        public string Duration { get; set; }

        public bool IsManual { get; set; }                

        // Extended properties
        public int Priority { get; set; }

        public DateTime Deadline { get; set; }

        public TimeSpan DurationTimeSpan { get; set; }
        public string RemainingDuration { get; set; }
        public TimeSpan RemainingDurationTimeSpan { get; set; }
        
        public double FixedCost { get; set; }
        public double ActualCost { get; set; }

        public string Work { get; set; }
        public TimeSpan WorkTimeSpan { get; set; }
        public string ActualWork { get; set; }
        public TimeSpan ActualWorkTimeSpan { get; set; }

        public DateTime ConstraintStartEnd { get; set; }
        public ConstraintType ConstraintType { get; set; }

        public IDictionary<CustomField, object> CustomFields { get; set; }



        #region TODO: Check out if commented properies make sense here

        // Basic Properies
        //public User StatusManager { get; set; }

        // Extended properies
        //public bool IsLockedByManager { get; }        
        //public bool IsManual { get; }        
        //public bool IsMarked { get; }       
        //public bool IsMilestone { get; }        
        //public bool LevelingAdjustsAssignments { get; }        
        //public bool LevelingCanSplit { get; }        
        //public string Name { get; }        
        //public int OutlineLevel { get; }        
        //public PublishedTask Parent { get; }

        //public int PercentPhysicalWorkComplete { get; }        
        //public PublishedTaskLinkCollection Predecessors { get; }

        //public DateTime Start { get; }        
        //public string StartText { get; } 
        //public DateTime ActualStart { get; }

        //public DateTime Finish { get; }
        //public string FinishText { get; }
        //public DateTime ActualFinish { get; }

        //public PublishedTaskLinkCollection Successors { get; }        
        //public TaskType TaskType { get; }        
        //public bool UsePercentPhysicalWorkComplete { get; }        
        //public bool IsActive { get; }

        //public Dictionary<string, object> FieldValues { get; }

        //public PublishedAssignmentCollection Assignments { get; }

        //public Calendar Calendar { get; }       

        //public string BudgetWork { get; set; } READ ONLY
        //public TimeSpan BudgetWorkTimeSpan { get; set; } READ ONLY
        //public DateTime Completion { get; set; } READ ONLY
        //public int PercentComplete { get; set; } READ ONLY
        
        //public double Cost { get; set; }  CALCULATED FROM RESOURCES?

        #endregion
    }
}
