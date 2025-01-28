﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace natura2000_portal_back.Models.backbone_db
{
    public class ProcessTimeLog : IEntityModel, IEntityModelBackboneDB
    {
        [Key]
        public long Id { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string ActionPerformed { get; set; } = string.Empty;
        public DateTime StampTime { get; set; }

        public static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ProcessTimeLog>()
                .ToTable("ProcessTimeLog")
                .HasKey(c => c.Id);
        }
    }
}