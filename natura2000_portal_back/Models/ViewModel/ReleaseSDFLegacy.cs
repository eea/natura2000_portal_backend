using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace natura2000_portal_back.Models.ViewModel
{
    [NotMapped]
    public class ReleaseSDFLegacy : IEntityModel
    {
        public SiteInfoReleaseLegacy SiteInfo { get; set; } = new SiteInfoReleaseLegacy();
        public SiteIdentificationLegacy SiteIdentification { get; set; } = new SiteIdentificationLegacy();
        public SiteLocationLegacy SiteLocation { get; set; } = new SiteLocationLegacy();
        public EcologicalInformationLegacy EcologicalInformation { get; set; } = new EcologicalInformationLegacy();
        public SiteDescriptionLegacy SiteDescription { get; set; } = new SiteDescriptionLegacy();
        public SiteProtectionStatus SiteProtectionStatus { get; set; } = new SiteProtectionStatus();
        public SiteManagementLegacy SiteManagement { get; set; } = new SiteManagementLegacy();
        public MapOfTheSite MapOfTheSite { get; set; } = new MapOfTheSite();

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ReleaseSDFLegacy>().HasNoKey();
        }
    }

    [NotMapped]
    public class SiteInfoReleaseLegacy : SiteInfoLegacy
    {
        public List<ReleaseInfo>? Releases { get; set; } = new List<ReleaseInfo>();
    }

    [NotMapped]
    public class ReleaseInfo
    {
        public long? ReleaseId { get; set; }
        public string? ReleaseName { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public Boolean? Legacy { get; set; }
    }
}
