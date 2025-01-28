using Microsoft.EntityFrameworkCore;

namespace natura2000_portal_back.Data
{
    public class N2KBackboneContext : BaseContext
    {
        public N2KBackboneContext(DbContextOptions<N2KBackboneContext> options) :  base(options) {
            this.Database.SetCommandTimeout(6000);
        }
    }
}