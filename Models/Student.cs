using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models {
    public class Student {
        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage="First Name Cannot be Longer than 50 Characters.")]
        [Column("FirstName")]
        public string FirstMidName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString="{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name="Enrollment Date")]
        public DateTime EnrollmentDate { get; set; }

        [Display(Name="Full Name")]
        public String FullName {
            get
            {
                return $"{LastName}, {FirstMidName}";
            }
        }

        public ICollection<Enrollment> Enrollments { get; set; }
    }
}