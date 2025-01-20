using Microsoft.AspNetCore.Mvc;

namespace natura2000_portal_back.Services
{
    public interface IDownloadService
    {
        Task<int> ComputingSAC(long releaseId, string email);
        Task<FileContentResult> HabitatsSearchResults(long? releaseId, string? habitatGroup, string? country, string? bioregion, string? habitat);
        Task<FileContentResult> SitesSearchResults(long? releaseId, string? siteType, string? country, string? bioregion, string? site, string? habitat, string? species, Boolean? sensitive);
        Task<FileContentResult> SpeciesSearchResults(long? releaseId, string? speciesGroup, string? country, string? bioregion, string? species, Boolean? sensitive);


        Task<FileContentResult> DownloadFromCwsfiles(long? releaseId);
    }
}