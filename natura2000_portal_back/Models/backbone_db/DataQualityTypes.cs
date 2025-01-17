﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace natura2000_portal_back.Models.backbone_db
{
    public class DataQualityTypes : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? HabitatCode { get; set; }
        public string? SpeciesCode { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DataQualityTypes>()
                .ToTable("DataQualityTypes")
                .HasKey(c => c.Id);
        }
    }
}