using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace natura2000_portal_back.Models.catalogue_db
{
    [Keyless]
    public class species_photos : IEntityModel, IEntityModelCatalogueDB
    {
        [Key]
        public int? id_photo { get; set; }
        public int? id_species { get; set; }
        public string? scientific_name { get; set; }
        public string? Natura2000 { get; set; }
        public string? eunis_id { get; set; }
        public string? filename { get; set; }
        public string? filename_newName { get; set; }
        public string? author_name { get; set; }
        public string? attribution_copyright { get; set; }
        public string? ThumbnailURL { get; set; }
        public string? WebURL { get; set; }
        public int? main { get; set; }
        public int? sort_order { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<species_photos>()
                .ToTable("species_photos");
        }
    }

    [Keyless]
    public class species_information : IEntityModel, IEntityModelCatalogueDB
    {
        [Key]
        public int? id_eunis { get; set; }
        public string? code_2000 { get; set; }
        public string? scientific_name { get; set; }
        public string? author { get; set; }
        public string? species_group_name { get; set; }
        public string? common_name { get; set; }
        public string? common_name_list { get; set; }
        public int? number_sites { get; set; }
        public int? number_countries { get; set; }
        public string? threat_code_EU { get; set; }
        public string? threat_name_EU { get; set; }
        public string? threat_code_E27 { get; set; }
        public string? threat_name_E27 { get; set; }
        public string? threat_code_WO { get; set; }
        public string? threat_name_WO { get; set; }
        public string? picture_url { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<species_information>()
                .ToTable("species_information");
        }
    }
}