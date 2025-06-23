using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TSZH_Komarov.Models;

namespace TSZH_Komarov.Data;

public partial class TszhKomarovContext : DbContext
{
    public TszhKomarovContext()
    {
    }

    public TszhKomarovContext(DbContextOptions<TszhKomarovContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<Apartment> Apartments { get; set; }

    public virtual DbSet<AppUser> AppUsers { get; set; }

    public virtual DbSet<ForumCategory> ForumCategories { get; set; }

    public virtual DbSet<House> Houses { get; set; }

    public virtual DbSet<MeterReading> MeterReadings { get; set; }

    public virtual DbSet<MeterType> MeterTypes { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Poll> Polls { get; set; }

    public virtual DbSet<PollOption> PollOptions { get; set; }

    public virtual DbSet<PreAnnouncement> PreAnnouncements { get; set; }

    public virtual DbSet<PreTopic> PreTopics { get; set; }

    public virtual DbSet<ServiceRequest> ServiceRequests { get; set; }

    public virtual DbSet<ServiceType> ServiceTypes { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    public virtual DbSet<TopicComment> TopicComments { get; set; }

    public virtual DbSet<Tszh> Tszhs { get; set; }

    public virtual DbSet<Vote> Votes { get; set; }

    public virtual DbSet<VoteType> VoteTypes { get; set; }

   /* protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Tszh_Komarov;Trusted_Connection=True");*/

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasIndex(e => e.TszhId, "IX_Announcements_TszhId");

            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(800);
            entity.Property(e => e.Topic).HasMaxLength(100);

            entity.HasOne(d => d.Tszh).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.TszhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Announcements_Tszh");
        });

        modelBuilder.Entity<Apartment>(entity =>
        {
            entity.HasIndex(e => e.HouseId, "IX_Apartments_HouseId");

            entity.HasIndex(e => e.UserId, "IX_Apartments_UserId");

            entity.Property(e => e.Number).HasMaxLength(20);
            entity.Property(e => e.PersonalAccount).HasMaxLength(10);

            entity.HasOne(d => d.House).WithMany(p => p.Apartments)
                .HasForeignKey(d => d.HouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Apartments_Houses");

            entity.HasOne(d => d.User).WithMany(p => p.Apartments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Apartments_Users");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK_Users");

            entity.Property(e => e.ChatId).HasMaxLength(80);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Fullname).HasMaxLength(50);
            entity.Property(e => e.LastReminderSent).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Salt).HasMaxLength(50);
        });

        modelBuilder.Entity<ForumCategory>(entity =>
        {
            entity.HasKey(e => e.ForumCategorieId);

            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<House>(entity =>
        {
            entity.HasIndex(e => e.TszhId, "IX_Houses_TszhId");

            entity.Property(e => e.Address).HasMaxLength(50);

            entity.HasOne(d => d.Tszh).WithMany(p => p.Houses)
                .HasForeignKey(d => d.TszhId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Houses_Tszh1");
        });

        modelBuilder.Entity<MeterReading>(entity =>
        {
            entity.HasKey(e => e.MeterReadingsId);

            entity.HasIndex(e => e.ApartamentId, "IX_MeterReadings_ApartamentId");

            entity.HasIndex(e => e.MeterTypeId, "IX_MeterReadings_MeterTypeId");

            entity.Property(e => e.ReadingDate).HasColumnType("datetime");

            entity.HasOne(d => d.Apartament).WithMany(p => p.MeterReadings)
                .HasForeignKey(d => d.ApartamentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeterReadings_Apartments");

            entity.HasOne(d => d.MeterType).WithMany(p => p.MeterReadings)
                .HasForeignKey(d => d.MeterTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MeterReadings_MeterTypes");
        });

        modelBuilder.Entity<MeterType>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(50);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_Notifications_UserId");

            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .HasMaxLength(400)
                .IsFixedLength();
            entity.Property(e => e.Topic)
                .HasMaxLength(100)
                .IsFixedLength();

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Users");
        });

        modelBuilder.Entity<Poll>(entity =>
        {
            entity.HasIndex(e => e.VoteTypeId, "IX_Polls_VoteTypeId");

            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.VoteType).WithMany(p => p.Polls)
                .HasForeignKey(d => d.VoteTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Polls_VoteType");
        });

        modelBuilder.Entity<PollOption>(entity =>
        {
            entity.HasIndex(e => e.PollId, "IX_PollOptions_PollId");

            entity.Property(e => e.OptionText).HasMaxLength(100);

            entity.HasOne(d => d.Poll).WithMany(p => p.PollOptions)
                .HasForeignKey(d => d.PollId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PollOptions_Polls");
        });

        modelBuilder.Entity<PreAnnouncement>(entity =>
        {
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(350);
            entity.Property(e => e.Topic).HasMaxLength(70);
        });

        modelBuilder.Entity<PreTopic>(entity =>
        {
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(450);
            entity.Property(e => e.Name).HasMaxLength(120);
        });

        modelBuilder.Entity<ServiceRequest>(entity =>
        {
            entity.HasKey(e => e.ServiceRequestId).HasName("PK_ServiceRequests_1");

            entity.HasIndex(e => e.ServiceTypeId, "IX_ServiceRequests_ServiceTypeId");

            entity.HasIndex(e => e.UserId, "IX_ServiceRequests_UserId");

            entity.Property(e => e.AdminComment).HasMaxLength(400);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Note).HasMaxLength(400);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.ServiceType).WithMany(p => p.ServiceRequests)
                .HasForeignKey(d => d.ServiceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceRequests_ServiceType");

            entity.HasOne(d => d.User).WithMany(p => p.ServiceRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ServiceRequests_Users");
        });

        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.ToTable("ServiceType");

            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.Name).HasMaxLength(80);
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasIndex(e => e.ForumCategorieId, "IX_Topics_ForumCategorieId");

            entity.HasIndex(e => e.UserId, "IX_Topics_UserId");

            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.ForumCategorie).WithMany(p => p.Topics)
                .HasForeignKey(d => d.ForumCategorieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Topics_ForumCategories");

            entity.HasOne(d => d.User).WithMany(p => p.Topics)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Topics_Users");
        });

        modelBuilder.Entity<TopicComment>(entity =>
        {
            entity.HasIndex(e => e.TopicId, "IX_TopicComments_TopicId");

            entity.HasIndex(e => e.UserId, "IX_TopicComments_UserId");

            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(100);

            entity.HasOne(d => d.Topic).WithMany(p => p.TopicComments)
                .HasForeignKey(d => d.TopicId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TopicComments_Topics");

            entity.HasOne(d => d.User).WithMany(p => p.TopicComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TopicComments_Users");
        });

        modelBuilder.Entity<Tszh>(entity =>
        {
            entity.HasKey(e => e.TszhId).HasName("PK_Tszh_1");

            entity.ToTable("Tszh");

            entity.Property(e => e.Address).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.HasKey(e => e.VotesId);

            entity.HasIndex(e => e.PollOptionId, "IX_Votes_PollOptionId");

            entity.HasIndex(e => e.UserId, "IX_Votes_UserId");

            entity.HasOne(d => d.PollOption).WithMany(p => p.Votes)
                .HasForeignKey(d => d.PollOptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Votes_PollOptions");

            entity.HasOne(d => d.User).WithMany(p => p.Votes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Votes_Users");
        });

        modelBuilder.Entity<VoteType>(entity =>
        {
            entity.ToTable("VoteType");

            entity.Property(e => e.Name).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
