using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace natura2000_portal_back.Models.ViewModel
{
    [NotMapped]
    public class ReleaseSDF : IEntityModel
    {
        public SiteInfoRelease SiteInfo { get; set; } = new SiteInfoRelease();
        public SiteIdentification SiteIdentification { get; set; } = new SiteIdentification();
        public SiteLocation SiteLocation { get; set; } = new SiteLocation();
        public EcologicalInformation EcologicalInformation { get; set; } = new EcologicalInformation();
        public SiteDescription SiteDescription { get; set; } = new SiteDescription();
        public SiteManagement SiteManagement { get; set; } = new SiteManagement();
        public GeospatialRepresentation GeospatialRepresentation { get; set; } = new GeospatialRepresentation();

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ReleaseSDF>().HasNoKey();
        }
    }

    [NotMapped]
    public class SiteInfoRelease : SiteInfo
    {
        public List<ReleaseInfo>? Releases { get; set; } = new List<ReleaseInfo>();
    }
}
