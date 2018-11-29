using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ContosoUniversity.Models
{
    public class Instructor {
        public int ID { get; set; }
        
        [StringLength(50)]
        [Required]
        [Display(Name= "Last Name")]
        public string LastName { get; set; }

        [StringLength(50)]
        [Required]
        [Display(Name= "First Name")]
        public string FirstMidName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString="{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name="Hire Date")]
        public DateTime HireDate { get; set; }

        [Display(Name="Full Name")]
        public string FullName {
            get
            {
                return $"{FirstMidName} {LastName}";
            }
        }

        public ICollection<CourseAssignment> CourseAssignments { get; set; }
        public OfficeAssignment OfficeAssignment { get; set; }
    }
}