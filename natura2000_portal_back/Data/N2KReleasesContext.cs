using Microsoft.EntityFrameworkCore;

namespace natura2000_portal_back.Data
{
    public class N2KReleasesContext : BaseContext
    {
        public N2KReleasesContext(DbContextOptions<N2KReleasesContext> options) : base(options)
        {
            this.Database.SetCommandTimeout(6000);
        }

    }
}