using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestMakerApp.Data.Models
{
    public class Result
    {
        // constructor
        public Result()
        {

        }
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public int QuizId { get; set; }
        [Required]
        public string Text { get; set; }
        public string Notes { get; set; }
        public int? MinValue { get; set; }
        public int? MaxValue { get; set; }
        [DefaultValue(0)]
        public int Type { get; set; }
        [DefaultValue(0)]
        public int Flags { get; set; }
        [DefaultValue(0)]
        [Required]
        public DateTime CreatedDate { get; set; }
        [Required]
        public DateTime LastModifiedDate { get; set; }

        // will be loaded by EF lazy-loading feature
        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; }
    }
}
