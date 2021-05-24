using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PaqueteriasAYT.Models
{
    public class Parcel
    {
        public long Id { get; set; }
        public string Cot { get; set; }
        public string Ov { get; set; }
        public string ClientName { get; set; }
        public string Responsible { get; set; }
        public string SiteId { get; set; }
        public string LabelHTML { get; set; }
        public int Status { get; set; }
        public string StopReason { get; set; }
        public string GuideNumber { get; set; }
        public DateTime UpdateDateTime { get; set; }
        public DateTime InsertDateTime { get; set; }
        public bool CreditBlocked { get; set; }
        public DateTime LiberationDateTime { get; set; }
  }
}
