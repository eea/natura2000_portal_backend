using Microsoft.EntityFrameworkCore;

namespace natura2000_portal_back.Models
{
    public interface IEntityModel
    {
        static void  OnModelCreating(ModelBuilder builder) { }
    }

    public interface IEntityModelBackboneDB { }

    public interface IEntityModelVersioningDB { }

    public interface IEntityModelReleasesDB { }

    public interface IEntityModelBackboneReadOnlyDB { }
}