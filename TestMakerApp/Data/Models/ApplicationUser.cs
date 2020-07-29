using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestMakerApp.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {

        }

        public string DisplayName { get; set; }
        public string Notes { get; set; }
        public string Type { get; set; }
        public string Flags { get; set; }
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public DateTime LastModifiedDate { get; set; }

        ///<summary></summary> 
        ///will be loaded by EF lazy-loading feature
        ///<summary>
        //list of all quizzes created by this user
        public virtual List<Quiz> Quizzes { get; set; }
        // list of all refresh tokens issued for this user
        public virtual List<Token> Tokens { get; set; }

    }
}
