using Microsoft.EntityFrameworkCore;

namespace natura2000_portal_back.Models.release_db
{
    public class AdministrativeRegion : IEntityModel, IEntityModelReleasesDB
    {
        public long id { get; set; }
        public long releaseId { get; set; }
        public string site_code { get; set; } = string.Empty;
        public string? F_2_2_1_site_nutscode { get; set; }
        public string? F_2_2_2_site_nutsname { get; set; }

        private string dbConnection = string.Empty;

        public AdministrativeRegion() { }

        public AdministrativeRegion(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AdministrativeRegion>()
                .ToTable("AdministrativeRegion", schema: "data")
                .HasKey(c => new { c.id });
        }
    }

    public class BiogeographicalMarineRegions : IEntityModel, IEntityModelReleasesDB
    {
        public long id { get; set; }
        public long releaseId { get; set; }
        public string site_code { get; set; } = string.Empty;
        public string F_2_3_1_biogeo_marine_code { get; set; } = string.Empty;
        public double? F_2_3_2_biogeo_marine_percent { get; set; }
        public string? F_2_3_3_marine_relationship_biogeo { get; set; }

        private string dbConnection = string.Empty;

        public BiogeographicalMarineRegions() { }

        public BiogeographicalMarineRegions(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BiogeographicalMarineRegions>()
                .ToTable("BiogeographicalMarineRegions", schema: "data")
                .HasKey(c => new { c.id });
        }
    }

    public class Habitats : IEntityModel, IEntityModelReleasesDB
    {
        public long id { get; set; }
        public long releaseId { get; set; }
        public string site_code { get; set; } = string.Empty;
        public string? F_3_1_1_habitat_code { get; set; }
        public string? habitat_name { get; set; }
        public Boolean? F_3_1_2_habitat_priority { get; set; }
        public int? F_3_1_3_habitat_notPresent { get; set; }
        public decimal? F_3_1_4_habitat_cover { get; set; }
        public string? F_3_1_5_habitat_caves { get; set; }
        public string? F_3_1_6_habitat_method { get; set; }
        public string? F_3_1_7_a_habitat_startdate { get; set; }
        public string? F_3_1_7_b_habitat_enddate { get; set; }
        public string? F_3_1_7_c_habitat_before2022 { get; set; }
        public string? F_3_1_8_habitat_significance { get; set; }
        public string? F_3_1_9_habitat_representivity { get; set; }
        public string? F_3_1_10_habitat_relative_surface { get; set; }
        public string? F_3_1_11_a_habitat_relative_surface_text { get; set; }
        public string? F_3_1_12_1_habitat_conserv_degree { get; set; }
        public float? F_3_1_12_2_a_habitat_area_condition_good { get; set; }
        public float? F_3_1_12_2_b_habitat_area_condition_notgood { get; set; }
        public float? F_3_1_12_2_c_habitat_area_condition_unknown { get; set; }
        public string? F_3_1_12_3_habitat_conserv_method { get; set; }
        public string? F_3_1_13_habitat_objectives { get; set; }
        public string? F_3_1_14_a_habitat_objectives_text { get; set; }
        public string? F_3_1_15_habitat_global { get; set; }
        public string? F_3_1_16_habitat_date { get; set; }

        private string dbConnection = string.Empty;

        public Habitats() { }

        public Habitats(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Habitats>()
                .ToTable("Habitats", schema: "data")
                .HasKey(c => new { c.id });
        }
    }

    public class ManagementBody : IEntityModel, IEntityModelReleasesDB
    {
        public long id { get; set; }
        public long releaseId { get; set; }
        public string site_code { get; set; } = string.Empty;
        public string? F_5_1_1_a_management_body { get; set; }
        public string? F_5_1_2_management_body_contact { get; set; }
        public string? F_5_1_3_a_management_body_address { get; set; }
        public string? F_5_1_4_management_body_email { get; set; }
        public string? F_5_1_5_management_URI { get; set; }

        private string dbConnection = string.Empty;

        public ManagementBody() { }

        public ManagementBody(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ManagementBody>()
                .ToTable("ManagementBody", schema: "data")
                .HasKey(c => new { c.id });
        }
    }

    public class ManagementInfo : IEntityModel, IEntityModelReleasesDB
    {
        public long id { get; set; }
        public long releaseId { get; set; }
        public string site_code { get; set; } = string.Empty;
        public string? F_5_2_1_management_plans_exist { get; set; }
        public string? F_5_2_3_a_management_text { get; set; }
        public string? F_5_3_1_a_measures_MP_included { get; set; }
        public string? F_5_3_1_b_measures_title { get; set; }
        public string? F_5_3_1_c_measures_URI { get; set; }
        public string? F_5_3_1_d_measures_text { get; set; }
        public string? F_5_3_2_a_measures_established { get; set; }
        public string? F_5_3_2_b_measures_implemented { get; set; }
        public string? F_5_4_a_measures_effectiveness { get; set; }
        public string? F_5_4_b_measures_results { get; set; }

        private string dbConnection = string.Empty;

        public ManagementInfo() { }

        public ManagementInfo(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ManagementInfo>()
                .ToTable("ManagementInfo", schema: "data")
                .HasKey(c => new { c.id });
        }
    }

    public class ManagementPlanList : IEntityModel, IEntityModelReleasesDB
    {
        public long id { get; set; }
        public long releaseId { get; set; }
        public string site_code { get; set; } = string.Empty;
        public string? F_5_2_2_a_management_plan_name { get; set; }
        public string? F_5_2_2_b_management_plan_URI { get; set; }
        public string? F_5_2_2_c_management_startdate { get; set; }
        public int? F_5_2_2_d_management_duration { get; set; }
        public string? F_5_2_2_e_management_duration_undefined { get; set; }

        private string dbConnection = string.Empty;

        public ManagementPlanList() { }

        public ManagementPlanList(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ManagementPlanList>()
                .ToTable("ManagementPlanList", schema: "data")
                .HasKey(c => new { c.id });
        }
    }

    public class Natura2000Sites : IEntityModel, IEntityModelReleasesDB
    {
        public long releaseId { get; set; }
        public string country_code { get; set; } = string.Empty;
        public string? F_1_1_site_type { get; set; }
        public string F_1_2_site_code { get; set; } = string.Empty;
        public int? version { get; set; }
        public string? F_1_3_site_name { get; set; }
        public string? F_1_3_1_site_name_nonLatin { get; set; }
        public string? F_1_4_1_respond_name { get; set; }
        public string? F_1_4_2_respond_contact { get; set; }
        public string? F_1_4_3_respond_address { get; set; }
        public string? F_1_4_4_respond_email { get; set; }
        public string? F_1_4_5_respond_URI { get; set; }
        public DateTime? F_1_5_1_SPA_date { get; set; }
        public string? F_1_5_2_a_SPA_act_URI { get; set; }
        public string? F_1_5_2_b_SPA_act_freetext { get; set; }
        public DateTime? F_1_5_3_pSCI_date { get; set; }
        public DateTime? cSCI_date { get; set; }
        public DateTime? F_1_5_4_SAC_date { get; set; }
        public string? F_1_5_5_a_SAC_act_URI { get; set; }
        public string? F_1_5_5_b_SAC_act_freetext { get; set; }
        public string? F_1_5_6_site_text { get; set; }
        public decimal? longitude { get; set; }
        public decimal? latitude { get; set; }
        public decimal? F_2_1_1_site_area { get; set; }
        public string? F_2_1_2_a_area_diff { get; set; }
        public string? F_2_1_2_b_area_diff_reason { get; set; }
        public string? F_2_1_3_a_area_diff_text { get; set; }
        public string? F_4_1_site_characteristics { get; set; }
        public string? F_4_2_site_quality { get; set; }
        public string? F_4_4_a_documentation { get; set; }
        public string? F_4_4_1_documentation_URI { get; set; }
        public string? F_4_4_2_documentation_date { get; set; }
        public string? F_4_3_5_pressure_date { get; set; }
        public string? F_6_1_1_inspire_namespace { get; set; }
        public string? F_6_1_2_inspire_local_id { get; set; }
        public string? F_6_1_3_inspire_version { get; set; }
        public DateTime? release_date { get; set; }

        private string dbConnection = string.Empty;

        public Natura2000Sites() { }

        public Natura2000Sites(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Natura2000Sites>()
                .ToTable("Natura2000Sites", schema: "data")
                .HasKey(c => new { c.releaseId, c.F_1_2_site_code });
        }
    }

    public class OtherSpecies : IEntityModel, IEntityModelReleasesDB
    {
        public long id { get; set; }
        public long releaseId { get; set; }
        public string site_code { get; set; } = string.Empty;
        public string? F_3_3_1_species_group { get; set; }
        public string? F_3_3_2_species_code { get; set; }
        public string? F_3_3_3_species_name { get; set; }
        public Boolean? F_3_3_4_species_sensitive { get; set; }
        public Boolean? F_3_3_5_species_notPresent { get; set; }
        public int? F_3_3_6_1_a_species_pop_min { get; set; }
        public int? F_3_3_6_1_b_species_pop_max { get; set; }
        public string? F_3_3_6_2_species_pop_unit { get; set; }
        public string? F_3_3_7_species_abundance { get; set; }
        public string? F_3_3_8_species_motivation { get; set; }

        private string dbConnection = string.Empty;

        public OtherSpecies() { }

        public OtherSpecies(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<OtherSpecies>()
                .ToTable("OtherSpecies", schema: "data")
                .HasKey(c => new { c.id });
        }
    }

    public class Pressure : IEntityModel, IEntityModelReleasesDB
    {
        public long id { get; set; }
        public long releaseId { get; set; }
        public string site_code { get; set; } = string.Empty;
        public string? F_4_3_1_pressure_code { get; set; }
        public string? F_4_3_2_pressure_rank { get; set; }
        public string? F_4_3_3_pressure_location { get; set; }
        public string? F_4_3_4_a_pressure_text { get; set; }

        private string dbConnection = string.Empty;

        public Pressure() { }

        public Pressure(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Pressure>()
                .ToTable("Pressure", schema: "data")
                .HasKey(c => new { c.id });
        }
    }

    public class Species : IEntityModel, IEntityModelReleasesDB
    {
        public long id { get; set; }
        public long releaseId { get; set; }
        public string site_code { get; set; } = string.Empty;
        public string? F_3_2_1_species_group { get; set; }
        public string? F_3_2_2_species_code { get; set; }
        public string? F_3_2_3_species_name { get; set; }
        public Boolean? F_3_2_4_species_sensitive { get; set; }
        public Boolean? F_3_2_5_species_notPresent { get; set; }
        public string? F_3_2_6_species_poptype { get; set; }
        public int? F_3_2_7_1_a_species_pop_min { get; set; }
        public int? F_3_2_7_1_b_species_pop_max { get; set; }
        public string? F_3_2_7_2_species_pop_unit { get; set; }
        public string? F_3_2_8_species_abundance { get; set; }
        public string? F_3_2_9_species_method { get; set; }
        public string? F_3_2_10_a_species_startdate { get; set; }
        public string? F_3_2_10_b_species_enddate { get; set; }
        public string? F_3_2_10_c_species_before_2022 { get; set; }
        public string? F_3_2_11_species_significance { get; set; }
        public string? F_3_2_12_species_SPA_class { get; set; }
        public string? F_3_2_13_species_rel_pop { get; set; }
        public string? F_3_2_14_a_species_rel_pop_text { get; set; }
        public string? F_3_2_15_1_species_conserv_degree { get; set; }
        public float? F_3_2_15_2_a_species_qual_suf { get; set; }
        public float? F_3_2_15_2_b_species_qual_non_suf { get; set; }
        public float? F_3_2_15_2_c_species_qual_unknown { get; set; }
        public string? F_3_2_15_3_a_species_qual_class_suf { get; set; }
        public string? F_3_2_15_3_b_species_qual_class_non_suf { get; set; }
        public string? F_3_2_15_3_c_species_qual_class_unknown { get; set; }
        public string? F_3_2_16_species_objectives { get; set; }
        public string? F_3_2_17_a_species_objectives_text { get; set; }
        public string? F_3_2_18_species_isolation { get; set; }
        public string? F_3_2_19_species_global { get; set; }
        public string? F_3_2_20_species_date { get; set; }

        private string dbConnection = string.Empty;

        public Species() { }

        public Species(string db)
        {
            dbConnection = db;
        }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Species>()
                .ToTable("Species", schema: "data")
                .HasKey(c => new { c.id });
        }
    }
}
