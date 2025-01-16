using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace natura2000_portal_back.Models.backbone_db
{
    public class Countries : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public string Code { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool isEUCountry { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Countries>()
                .ToTable("Countries")
                .HasKey(c => new { c.Code });
        }
    }
}
