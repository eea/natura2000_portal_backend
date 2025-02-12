using natura2000_portal_back.Models.ViewModel;

namespace natura2000_portal_back.Services
{
    public interface ISDFService
    {
        Task<ReleaseSDF> GetReleaseData(string SiteCode, int ReleaseId, Boolean initialValidation, Boolean internalViewers, Boolean internalBarometer, Boolean internalPortalSDFSensitive, Boolean publicViewers, Boolean publicBarometer, Boolean sdfPublic, Boolean naturaOnlineList, Boolean productsCreated, Boolean jediDimensionCreated, bool showSensitive = true);
    }
}
