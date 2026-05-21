using Microsoft.EntityFrameworkCore;

namespace natura2000_portal_back.Models.production_db
{
    [Keyless]
    public class HabitatsList : IEntityModel, IEntityModelProductionDB
    {
        public string? habitat_code { get; set; }
        public string? habitat_priority { get; set; }
        public string? habitat_priority_text { get; set; }
        public string? habitat_description { get; set; }
        public string? label { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<HabitatsList>()
                .ToTable("HabitatsList", schema: "reference");
        }
    }
}