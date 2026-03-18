// <copyright file="AppDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using LNUBookShare.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace LNUBookShare.Infrastructure;

public partial class AppDbContext : IdentityDbContext<User, Role, int>
{
    //public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public virtual DbSet<Book> Books { get; set; }
    public virtual DbSet<BookReview> BookReviews { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<ChatMessage> ChatMessages { get; set; }
    public virtual DbSet<EmailConfirmation> EmailConfirmations { get; set; }
    public virtual DbSet<Faculty> Faculties { get; set; }
    public virtual DbSet<Favorite> Favorites { get; set; }
    public virtual DbSet<Image> Images { get; set; }
    public virtual DbSet<ReservationQueue> ReservationQueues { get; set; }
    public override DbSet<Role> Roles { get; set; }
    public override DbSet<User> Users { get; set; }


    public virtual DbSet<UserReview> UserReviews { get; set; }

        // ВАЖЛИВО для Identity
        base.OnModelCreating(modelBuilder);
            entity.HasKey(e => e.BookId).HasName("book_pkey");
            entity.HasKey(e => e.BookId).HasName("book_pkey");

        // --- USER (Identity) ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");
            entity.Property(e => e.Id).HasColumnName("user_id");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.EmailConfirmed).HasColumnName("is_email_confirmed");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FacultyId).HasColumnName("faculty_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id").HasDefaultValue(2);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);

            entity.HasOne(d => d.Faculty).WithMany(p => p.Users).HasForeignKey(d => d.FacultyId);
            entity.HasOne(d => d.Role).WithMany(p => p.Users).HasForeignKey(d => d.RoleId);
        });

        // --- ROLE (Identity) ---
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("role");
            entity.Property(e => e.Id).HasColumnName("role_id");
            entity.Property(e => e.Name).HasColumnName("role_name");
        });

        // --- CHAT MESSAGE ---
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("chat_message_pkey");
            entity.ToTable("chat_message");
            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.Content).HasColumnName("content_");
            entity.Property(e => e.SentAt).HasColumnName("sent_at").HasDefaultValueSql("now()");

            entity.HasOne(d => d.Receiver).WithMany(p => p.ChatMessageReceivers).HasForeignKey(d => d.ReceiverId).OnDelete(DeleteBehavior.ClientSetNull);
            entity.HasOne(d => d.Sender).WithMany(p => p.ChatMessageSenders).HasForeignKey(d => d.SenderId).OnDelete(DeleteBehavior.ClientSetNull);
        });

        // --- BOOK ---
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("book_pkey");
            entity.ToTable("book");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Year).HasColumnName("year_");

            entity.HasOne(d => d.Owner).WithMany(p => p.Books).HasForeignKey(d => d.OwnerId);
            entity.HasOne(d => d.Category).WithMany(p => p.Books).HasForeignKey(d => d.CategoryId);
        });

        // --- BOOK REVIEW ---
        modelBuilder.Entity<BookReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("book_review_pkey");
            entity.ToTable("book_review");
            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Comment).HasColumnName("comment_");

            entity.HasOne(d => d.Book).WithMany(p => p.BookReviews).HasForeignKey(d => d.BookId);
            entity.HasOne(d => d.Reviewer).WithMany(p => p.BookReviews).HasForeignKey(d => d.ReviewerId);
        });

        // --- USER REVIEW ---
        modelBuilder.Entity<UserReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("user_review_pkey");
            entity.ToTable("user_review");
            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");

            entity.HasOne(d => d.Owner).WithMany(p => p.UserReviewOwners).HasForeignKey(d => d.OwnerId);
            entity.HasOne(d => d.Reviewer).WithMany(p => p.UserReviewReviewers).HasForeignKey(d => d.ReviewerId);
        });

        // --- FAVORITE ---
        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("favorite_pkey");
            entity.ToTable("favorite");
            entity.Property(e => e.FavoriteId).HasColumnName("favorite_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.BookId).HasColumnName("book_id");

            entity.HasOne(d => d.Book).WithMany(p => p.Favorites).HasForeignKey(d => d.BookId);
            entity.HasOne(d => d.User).WithMany(p => p.Favorites).HasForeignKey(d => d.UserId);
        });

        // --- RESERVATION QUEUE ---
        modelBuilder.Entity<ReservationQueue>(entity =>
        {
            entity.HasKey(e => e.QueueId).HasName("reservation_queue_pkey");
            entity.ToTable("reservation_queue");
            entity.Property(e => e.QueueId).HasColumnName("queue_id");
            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.HasOne(d => d.Book).WithMany(p => p.ReservationQueues).HasForeignKey(d => d.BookId);
            entity.HasOne(d => d.User).WithMany(p => p.ReservationQueues).HasForeignKey(d => d.UserId);
                .HasColumnName("role_name");
                .HasColumnName("role_name");
        // --- FACULTY & CATEGORY ---
        modelBuilder.Entity<Faculty>(entity => {
            entity.HasKey(e => e.FacultyId);
            entity.ToTable("faculty");
                .HasColumnName("email");
                .HasColumnName("email");
            entity.Property(e => e.FacultyId).HasColumnName("faculty_id");
            entity.Property(e => e.FacultyName).HasColumnName("faculty_name");
        });

        modelBuilder.Entity<Category>(entity => {
            entity.HasKey(e => e.CategoryId);
            entity.ToTable("category");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName).HasColumnName("category_name");
        });

        // --- IMAGE & EMAIL CONFIRMATION ---
        modelBuilder.Entity<Image>(entity => {
            entity.HasKey(e => e.ImageId);
            entity.ToTable("image");
            entity.Property(e => e.ImageId).HasColumnName("image_id");
        });

        modelBuilder.Entity<EmailConfirmation>(entity => {
            entity.HasKey(e => e.ConfirmationId);
            entity.ToTable("email_confirmation");
            entity.Property(e => e.ConfirmationId).HasColumnName("confirmation_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<Faculty>().HasData(
            new Faculty { FacultyId = 1, FacultyName = "Прикладна математика та інформатика" });

        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, CategoryName = "Програмування" });

        modelBuilder.Entity<Role>().HasData(
           new Role
           {
               Id = 2,
               Name = "authorized",
               NormalizedName = "AUTHORIZED",
               ConcurrencyStamp = "STATIC-ROLE-STAMP-0000",
           });

        // var hasher = new PasswordHasher<User>();
        var testUser = new User
        {
            Id = 1,
            Email = "student1@lnu.edu.ua",
            UserName = "student1@lnu.edu.ua",
            NormalizedEmail = "STUDENT1@LNU.EDU.UA",
            NormalizedUserName = "STUDENT1@LNU.EDU.UA",
            FirstName = "Іван",
            LastName = "Франко",
            FacultyId = 1,
            RoleId = 2,
            IsActive = true,
            IsEmailConfirmed = true,
            PasswordHash = "AQAAAAIAAYagAAAAEKA2vL5nQ69q4rQxG+E+mO2e8q1b9wXYZ...",

            SecurityStamp = "STATIC-STAMP-1111-2222-3333",
            ConcurrencyStamp = "STATIC-USER-STAMP-4444",
        };

        // testUser.PasswordHash = hasher.HashPassword(testUser, "Password123!");
        modelBuilder.Entity<User>().HasData(testUser);

        modelBuilder.Entity<Book>().HasData(
            new Book
            {
                BookId = 1,
                Title = "Чиста Архітектура",
                Author = "Роберт Мартін",
                Isbn = "978-0134494166",
                Year = 2017,
                CategoryId = 1,
                OwnerId = testUser.Id,
                Status = "available",
                Language = "Українська",
                Publisher = "Фабула",
            });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}