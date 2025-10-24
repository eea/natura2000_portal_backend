using natura2000_portal_back.Models.ViewModel;

namespace natura2000_portal_back.Services
{
    public interface IInfoService
    {
        Task<List<ReleasesCatalog>> GetOfficialReleases(Boolean initialValidation, Boolean internalViewers, Boolean internalBarometer, Boolean internalPortalSDFSensitive, Boolean publicViewers, Boolean publicBarometer, Boolean sdfPublic, Boolean naturaOnlineList, Boolean productsCreated, Boolean jediDimensionCreated);
        Task<List<HabitatsParametered>> GetParameteredHabitats(long? releaseId, string? habitatGroup, string? country, string? bioregion, string? habitat);
        Task<List<SitesParametered>> GetParameteredSites(long? releaseId, string? siteType, string? country, string? bioregion, string? site, string? habitat, string? species, Boolean? sensitive);
        Task<List<SpeciesParametered>> GetParameteredSpecies(long? releaseId, string? speciesGroup, string? country, string? bioregion, string? species, Boolean? sensitive);
        Task<ReleaseCounters> GetLatestReleaseCounters();

        Task<List<CountrySubmissions>> GetSubmissions();
    }
}
