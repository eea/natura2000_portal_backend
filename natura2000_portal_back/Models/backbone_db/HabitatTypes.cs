using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace natura2000_portal_back.Models.backbone_db
{
    public class HabitatTypes : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public string Code { get; set; } = string.Empty;
        public string? HDName { get; set; }
        public string? Name { get; set; }
        public string? Realm { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HabitatTypes>()
                .ToTable("HabitatTypes")
                .HasKey(c => new { c.Code });
        }
    }
}