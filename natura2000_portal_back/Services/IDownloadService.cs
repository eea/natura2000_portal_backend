using natura2000_portal_back.Models.ViewModel;

namespace natura2000_portal_back.Services
{
    public interface IDownloadService
    {
        Task<int> ComputingSAC(long releaseId, string email);
    }
}