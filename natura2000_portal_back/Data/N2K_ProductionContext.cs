using Microsoft.EntityFrameworkCore;

namespace natura2000_portal_back.Data
{
    public class N2K_ProductionContext : BaseContext
    {
        public N2K_ProductionContext(DbContextOptions<N2K_ProductionContext> options) : base(options)
        {
            this.Database.SetCommandTimeout(36000);
        }

    }
}
