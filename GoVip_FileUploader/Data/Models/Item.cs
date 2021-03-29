using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GoVip_FileUploader.Data.Models
{
    public class Item
    {
        // constructor
        public Item()
        {

        }

        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string Filepath { get; set; }
        [Required]
        public string UploadedBy { get; set; }
        [Required]
        public string UpdatedBy { get; set; }
        public DateTime DateTimeCreated { get; set; }
        public DateTime DateTimeUpdated { get; set; }

        // will be loaded by EF lazy-loading feature
        [ForeignKey("UploadedBy")]
        public virtual ApplicationUser User { get; set; }
    }
}
