using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace natura2000_portal_back.Models.ViewModel
{
    [NotMapped]
    public class SDF : IEntityModel
    {
        public SiteInfo SiteInfo { get; set; } = new SiteInfo();
        public SiteIdentification SiteIdentification { get; set; } = new SiteIdentification();
        public SiteLocation SiteLocation { get; set; } = new SiteLocation();
        public EcologicalInformation EcologicalInformation { get; set; } = new EcologicalInformation();
        public SiteDescription SiteDescription { get; set; } = new SiteDescription();
        public SiteManagement SiteManagement { get; set; } = new SiteManagement();
        public GeospatialRepresentation GeospatialRepresentation { get; set; } = new GeospatialRepresentation();

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<SDF>().HasNoKey();
        }
    }

    [NotMapped]
    public class SiteInfo
    {
        public string? SiteName { get; set; }
        public string? SiteType { get; set; }
        public string? SiteCode { get; set; }
    }

    [NotMapped]
    public class SiteIdentification
    {
        public string? F_1_1_site_type { get; set; }
        public string? F_1_2_site_code { get; set; }
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
    }

    [NotMapped]
    public class SiteLocation
    {
        public decimal? longitude { get; set; }
        public decimal? latitude { get; set; }
        public decimal? F_2_1_1_site_area { get; set; }
        public string? F_2_1_2_a_area_diff { get; set; }
        public string? F_2_1_3_a_area_diff_text { get; set; }
        public List<AdministrativeRegion> F_2_2_administrative_region { get; set; } = new List<AdministrativeRegion>();
        public List<BiogeographicalRegions> F_2_3_biogeographical_regions { get; set; } = new List<BiogeographicalRegions>();
    }

    [NotMapped]
    public class BiogeographicalRegions
    {
        public string? F_2_3_1_biogeo_marine_code { get; set; }
        public double? F_2_3_2_biogeo_marine_percent { get; set; }
        public string? F_2_3_3_marine_relationship_biogeo { get; set; }
    }

    [NotMapped]
    public class AdministrativeRegion
    {
        public string? F_2_2_1_site_nutscode { get; set; }
        public string? F_2_2_2_site_nutsname { get; set; }
    }

    [NotMapped]
    public class EcologicalInformation
    {
        public List<EssentialInformationHabitat> F_3_1_a_essential_information { get; set; } = new List<EssentialInformationHabitat>();
        public List<SiteAssessmentHabitat> F_3_1_b_site_assessment { get; set; } = new List<SiteAssessmentHabitat>();
        public List<EssentialInformationSpecies> F_3_2_a_essential_information { get; set; } = new List<EssentialInformationSpecies>();
        public List<SiteAssessmentSpecies> F_3_2_b_site_assessment { get; set; } = new List<SiteAssessmentSpecies>();
        public List<OtherSpeciesSDF> F_3_3_other_species { get; set; } = new List<OtherSpeciesSDF>();
        public List<SpeciesMotivation> F_3_3_8_species_motivation { get; set; } = new List<SpeciesMotivation>();
    }

    [NotMapped]
    public class EssentialInformationHabitat
    {
        public string? F_3_1_1_habitat_code { get; set; }
        public string? habitat_name { get; set; }
        public string? F_3_1_2_habitat_priority { get; set; }
        public string? F_3_1_3_not_present { get; set; }
        public decimal? F_3_1_4_habitat_cover { get; set; }
        public int? F_3_1_5_habitat_caves { get; set; }
        public string? F_3_1_6_habitat_method { get; set; }
        public string? F_3_1_7_habitat_date { get; set; }
    }

    [NotMapped]
    public class SiteAssessmentHabitat
    {
        public string? F_3_1_1_habitat_code { get; set; }
        public string? habitat_name { get; set; }
        public string? F_3_1_2_habitat_priority { get; set; }
        public string? F_3_1_8_habitat_significance { get; set; }
        public string? F_3_1_9_habitat_representivity { get; set; }
        public string? F_3_1_10_habitat_relative_surface { get; set; }
        public string? F_3_1_12_1_habitat_conserv_degree { get; set; }
        public string? F_3_1_12_2_habitat_area_condition { get; set; }
        public string? F_3_1_12_3_habitat_conserv_method { get; set; }
        public string? F_3_1_13_habitat_objectives { get; set; }
        public string? F_3_1_14_a_habitat_objectives_text { get; set; }
        public string? F_3_1_15_habitat_global { get; set; }
        public DateOnly? F_3_1_16_habitat_date { get; set; }
    }

    [NotMapped]
    public class EssentialInformationSpecies
    {
        public string? F_3_2_1_species_group { get; set; }
        public string? F_3_2_2_species_code { get; set; }
        public string? F_3_2_3_species_name { get; set; }
        public string? F_3_2_4_species_sensitive { get; set; }
        public string? F_3_2_5_species_NP { get; set; }
        public string? F_3_2_6_species_poptype { get; set; }
        public int? F_3_2_7_1_a_species_pop_min { get; set; }
        public int? F_3_2_7_1_b_species_pop_max { get; set; }
        public string? F_3_2_7_2_species_pop_unit { get; set; }
        public string? F_3_2_8_species_abundance { get; set; }
        public string? F_3_2_9_species_method { get; set; }
        public string? F_3_2_10_species_date { get; set; }
        public string? SpeciesEunisId { get; set; }
    }

    [NotMapped]
    public class SiteAssessmentSpecies
    {
        public string? F_3_2_1_species_group { get; set; }
        public string? F_3_2_2_species_code { get; set; }
        public string? F_3_2_3_species_name { get; set; }
        public string? F_3_2_6_species_poptype { get; set; }
        public string? F_3_2_11_species_significance { get; set; }
        public string? F_3_2_12_species_SPA_class { get; set; }
        public string? F_3_2_13_species_rel_pop { get; set; }
        public string? F_3_2_14_a_species_rel_pop_text { get; set; }
        public string? F_3_2_15_1_species_conserv_degree { get; set; }
        public string? F_3_2_15_2_species_qual { get; set; }
        public string? F_3_2_15_3_species_qual_class { get; set; }
        public string? F_3_2_16_species_objectives { get; set; }
        public string? F_3_2_17_a_species_objectives_text { get; set; }
        public string? F_3_2_18_species_isolation { get; set; }
        public string? F_3_2_19_species_global { get; set; }
        public DateOnly? F_3_2_20_species_date { get; set; }
        public string? SpeciesEunisId { get; set; }
    }

    [NotMapped]
    public class OtherSpeciesSDF
    {
        public string? F_3_3_1_species_group { get; set; }
        public string? F_3_3_2_species_code { get; set; }
        public string? F_3_3_3_species_name { get; set; }
        public string? F_3_3_4_species_sensitive { get; set; }
        public string? F_3_3_5_species_NP { get; set; }
        public int? F_3_3_6_1_a_species_pop_min { get; set; }
        public int? F_3_3_6_1_b_species_pop_max { get; set; }
        public string? F_3_3_6_2_species_pop_unit { get; set; }
        public string? F_3_3_7_species_abundance { get; set; }
        public string? SpeciesEunisId { get; set; }
    }

    [NotMapped]
    public class SpeciesMotivation
    {
        public string? F_3_3_2_species_code { get; set; }
        public string? F_3_3_3_species_name { get; set; }
        public string? F_3_3_8_hd_annexii { get; set; }
        public string? F_3_3_8_hd_annexiv { get; set; }
        public string? F_3_3_8_hd_annexv { get; set; }
        public string? F_3_3_8_bd_annexi { get; set; }
        public string? F_3_3_8_migratory { get; set; }
        public string? F_3_3_8_cfp_prohib { get; set; }
        public string? F_3_3_8_national_red_list { get; set; }
        public string? F_3_3_8_eu_red_list { get; set; }
        public string? F_3_3_8_global_red_list { get; set; }
        public string? F_3_3_8_endemic { get; set; }
        public string? F_3_3_8_international_convention { get; set; }
        public string? F_3_3_8_hd_annexi { get; set; }
        public string? F_3_3_8_cwr_frg { get; set; }
        public string? F_3_3_8_ias_union { get; set; }
        public string? F_3_3_8_other { get; set; }
        public string? SpeciesEunisId { get; set; }
    }

    [NotMapped]
    public class SiteDescription
    {
        public string? F_4_1_site_characteristics { get; set; }
        public string? F_4_2_site_quality { get; set; }
        public List<Pressures> F_4_3_pressures { get; set; } = new List<Pressures>();
        public DateTime? F_4_3_5_pressure_date { get; set; }
        public string? F_4_4_a_documentation { get; set; }
        public string? F_4_4_1_documentation_URI { get; set; }
        public DateTime? F_4_4_2_documentation_date { get; set; }
    }

    [NotMapped]
    public class Pressures
    {
        public string? F_4_3_1_pressure_code { get; set; }
        public string? F_4_3_2_pressure_rank { get; set; }
        public string? F_4_3_3_pressure_location { get; set; }
        public string? F_4_3_4_a_pressure_text { get; set; }
    }

    [NotMapped]
    public class SiteManagement
    {
        public string? F_5_1_1_a_management_body { get; set; }
        public string? F_5_1_2_management_body_contact { get; set; }
        public string? F_5_1_3_a_management_body_address { get; set; }
        public string? F_5_1_4_management_body_email { get; set; }
        public string? F_5_1_5_management_URI { get; set; }
        public string? F_5_2_1_management_plans_exist { get; set; }
        public List<ManagementList> F_5_2_2_management_list { get; set; } = new List<ManagementList>();
        public string? F_5_2_3_a_management_text { get; set; }
        public string? F_5_3_1_a_measures_MP_included { get; set; }
        public List<MeasuresList> F_5_3_1_measures_list { get; set; } = new List<MeasuresList>();
        public string? F_5_3_1_d_measures_text { get; set; }
        public string? F_5_3_2_a_measures_established { get; set; }
        public string? F_5_3_2_b_measures_implemented { get; set; }
        public string? F_5_4_a_measures_effectiveness { get; set; }
        public string? F_5_4_b_measures_results { get; set; }
    }

    [NotMapped]
    public class ManagementList
    {
        public string? F_5_2_2_a_management_plan_name { get; set; }
        public string? F_5_2_2_b_management_plan_URI { get; set; }
        public DateOnly? F_5_2_2_c_management_startdate { get; set; }
        public int? F_5_2_2_d_management_duration { get; set; }
    }

    [NotMapped]
    public class MeasuresList
    {
        public string? F_5_3_1_b_measures_title { get; set; }
        public string? F_5_3_1_c_measures_URI { get; set; }
    }

    [NotMapped]
    public class GeospatialRepresentation
    {
        public string? F_6_1_1_inspire_namespace { get; set; }
        public string? F_6_1_2_inspire_local_id { get; set; }
        public string? F_6_1_3_inspire_version { get; set; }
    }
}