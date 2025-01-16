using  Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace natura2000_portal_back.Models.backbone_db
{
    public class OwnerShipTypes : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? Code { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<OwnerShipTypes>()
                .ToTable("OwnerShipTypes")
                .HasKey("Id");
        }
    }
}