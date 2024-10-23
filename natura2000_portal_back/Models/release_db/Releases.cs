﻿using Microsoft.EntityFrameworkCore;

namespace natura2000_portal_back.Models.release_db
{
    public class ReleasesInputParam : IEntityModel, IEntityModelReleasesDB
    {
        public string Name { get; set; } = string.Empty;
        public Boolean? Final { get; set; }
        public string? Character { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ReleasesInputParam>()
                //.ToTable("Releases")
                .HasNoKey();
        }
    }

    public class Releases : IEntityModel, IEntityModelReleasesDB
    {
        public long ID { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public DateTime? CreateDate { get; set; }
        public Boolean? Final { get; set; }
        public string? ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
        public string? Character { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Releases>()
                .ToTable("Releases")
                .HasKey(c => new { c.ID });
        }
    }
}