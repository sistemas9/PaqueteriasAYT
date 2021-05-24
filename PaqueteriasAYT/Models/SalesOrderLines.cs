using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaqueteriasAYT.Models
{
    public class SalesOrderLines
    {
        public string ItemId { get; set; }
        public string Name { get; set; }
        public float SalesQty { get; set; }
        public string SalesUnit { get; set; }
    }
}
