using Microsoft.EntityFrameworkCore;

namespace natura2000_portal_back.Models.production_db
{
    [Keyless]
    public class NaturaDirectiveSpeciesList : IEntityModel, IEntityModelProductionDB
    {
        public string? species_code { get; set; }
        public string? species_name { get; set; }
        public string? complementary_info { get; set; }
        public string? taxonomical_group { get; set; }
        public string? species_group { get; set; }
        public string? HD_name { get; set; }
        public string? HD_priority { get; set; }
        public string? HD_annexII { get; set; }
        public string? HD_annexIV { get; set; }
        public string? HD_annexV { get; set; }
        public string? BD_name { get; set; }
        public string? BD_annexI { get; set; }
        public string? BD_annexII_partA { get; set; }
        public string? BD_annexII_partB { get; set; }
        public string? BD_annexIII_partA { get; set; }
        public string? BD_annexIII_partB { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<NaturaDirectiveSpeciesList>()
                .ToTable("NaturaDirectiveSpeciesList", schema: "reference");
        }
    }
}