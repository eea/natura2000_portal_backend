using natura2000_portal_back.Models.ViewModel;

namespace natura2000_portal_back.Services
{
    public interface ISDFService
    {
        Task<ReleaseSDF> GetReleaseData(string SiteCode, int ReleaseId = -1, bool showSensitive = true);
    }
}
