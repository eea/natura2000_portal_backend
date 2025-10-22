using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;

namespace natura2000_portal_back.Models.ViewModel
{

    [Keyless]
    public class CountrySubmissionsDB : IEntityModel
    {
        public string? CountryCode { get; set; }
        public int VersionID { get; set; }
        public string? ImportDate { get; set; }

        public static new void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CountrySubmissionsDB>();
        }
    }


    [Keyless]
    public class Submission : IEntityModel
    {
        public int VersionID { get; set; }
        public string? ImportDate { get; set; }
        public static new void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Submission>();
        }
    }




    [Keyless]
    public class CountrySubmissions : IEntityModel
    {
        public string? CountryCode { get; set; }
        public List<Submission>? Submissions { get; set; }

        public static new void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<CountrySubmissions>();
        }
    }
}
