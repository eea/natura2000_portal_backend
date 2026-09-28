using Microsoft.EntityFrameworkCore;

namespace natura2000_portal_back.Data
{
    public class N2KCatalogueContext : BaseContext
    {
        public N2KCatalogueContext(DbContextOptions<N2KCatalogueContext> options) :  base(options) {
            this.Database.SetCommandTimeout(6000); 
        }

    }
}
