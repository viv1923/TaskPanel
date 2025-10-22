using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaskPanel.Models;

namespace TaskPanel.Data;

public partial class DataContext : DbContext
{
    public DataContext()
    {
    }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<GenUser> GenUsers { get; set; }

    public virtual DbSet<GenTaskAssign> GenTaskAssigns { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=defConn");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // GenUser config
        modelBuilder.Entity<GenUser>(entity =>
        {
            entity.HasKey(e => e.NUserId).HasName("PK__GenUser__8C35B9A902B9A659");

            entity.ToTable("GenUser");

            entity.Property(e => e.NUserId).HasColumnName("nUserID");
            entity.Property(e => e.CDescription).HasColumnName("cDescription");
            entity.Property(e => e.CEmailId)
                .HasMaxLength(100)
                .HasColumnName("cEmailID");
            entity.Property(e => e.CPassword)
                .HasMaxLength(100)
                .HasColumnName("cPassword");
            entity.Property(e => e.CUserName)
                .HasMaxLength(100)
                .HasColumnName("cUserName");
            entity.Property(e => e.NMobileNo).HasColumnName("nMobileNo");
        });

        modelBuilder.Entity<GenTaskAssign>(entity =>
        {
            entity.HasKey(e => e.NTaskNo).HasName("PK__GenTaskA__3F1F420600566DB9");

            entity.ToTable("GenTaskAssign");

            entity.Property(e => e.NTaskNo).HasColumnName("nTaskNo");
            entity.Property(e => e.CFileName)
                .HasMaxLength(255)
                .HasColumnName("cFileName");
            entity.Property(e => e.CTask).HasColumnName("cTask");
            entity.Property(e => e.DApprove)
                .HasColumnType("datetime")
                .HasColumnName("dApprove");
            entity.Property(e => e.DCompleteDate)
                .HasColumnType("datetime")
                .HasColumnName("dCompleteDate");
            entity.Property(e => e.DDeadLine)
                .HasColumnType("datetime")
                .HasColumnName("dDeadLine");
            entity.Property(e => e.DTaskDate)
                .HasColumnType("datetime")
                .HasColumnName("dTaskDate");
            entity.Property(e => e.NApprove).HasColumnName("nApprove");
            entity.Property(e => e.NComplete).HasColumnName("nComplete");
            entity.Property(e => e.NFromUser).HasColumnName("nFromUser");
            entity.Property(e => e.NTaskType).HasColumnName("nTaskType");
            entity.Property(e => e.NToUser).HasColumnName("nToUser");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
