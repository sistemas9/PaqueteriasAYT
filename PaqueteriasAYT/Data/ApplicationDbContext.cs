using Microsoft.EntityFrameworkCore;
using PaqueteriasAYT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaqueteriasAYT.Data
{
    public class ApplicationDbContext : DbContext
    {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

        public DbSet<AppConfiguration> AppConfiguration { get; set; }

    }
}
