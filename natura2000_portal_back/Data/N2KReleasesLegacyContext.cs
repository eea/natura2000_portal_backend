using Microsoft.EntityFrameworkCore;

namespace natura2000_portal_back.Data
{
    public class N2KReleasesLegacyContext : BaseContext
    {
        public N2KReleasesLegacyContext(DbContextOptions<N2KReleasesLegacyContext> options) : base(options)
        {
            this.Database.SetCommandTimeout(36000);
        }

    }
}