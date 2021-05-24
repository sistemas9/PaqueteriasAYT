using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PaqueteriasAYT.Models
{
    public class AppConfiguration
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string User { get; set; }
        [Required]
        public string SiteId { get; set; }
        [Required]
        public string PrinterPath { get; set; }
        [Required]
        public string PrinterZPLPath { get; set; }
    }
}
