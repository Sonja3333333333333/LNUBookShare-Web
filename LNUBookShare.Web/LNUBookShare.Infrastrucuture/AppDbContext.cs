// <copyright file="AppDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using LNUBookShare.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure;

public partial class AppDbContext : IdentityDbContext<User, Role, int>
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }
    public virtual DbSet<BookReview> BookReviews { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<ChatMessage> ChatMessages { get; set; }
    public virtual DbSet<EmailConfirmation> EmailConfirmations { get; set; }
    public virtual DbSet<Faculty> Faculties { get; set; }
    public virtual DbSet<Favorite> Favorites { get; set; }
    public virtual DbSet<Image> Images { get; set; }
    public virtual DbSet<ReservationQueue> ReservationQueues { get; set; }
    public virtual DbSet<UserReview> UserReviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. ТАБЛИЦЯ: book
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("book");
            entity.HasKey(e => e.BookId).HasName("book_pkey");

            entity.Property(e => e.BookId).HasColumnName("book_id");
            entity.Property(e => e.Title).HasMaxLength(150).HasColumnName("title").IsRequired();
            entity.Property(e => e.Author).HasMaxLength(100).HasColumnName("author").IsRequired();
            entity.Property(e => e.Isbn).HasMaxLength(20).HasColumnName("isbn");
            entity.Property(e => e.Year).HasColumnName("year_").IsRequired();
            entity.Property(e => e.Publisher).HasMaxLength(100).HasColumnName("publisher");
            entity.Property(e => e.Language).HasMaxLength(50).HasColumnName("language_");
            entity.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired();
            entity.Property(e => e.OwnerId).HasColumnName("owner_id").IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValueSql("'available'::character varying").HasColumnName("status").IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at").IsRequired();
            entity.Property(e => e.CoverId).HasColumnName("cover_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Books).HasForeignKey(d => d.CategoryId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("book_category_id_fkey");
            entity.HasOne(d => d.Cover).WithMany(p => p.Books).HasForeignKey(d => d.CoverId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("book_cover_id_fkey");
            entity.HasOne(d => d.Owner).WithMany(p => p.Books).HasForeignKey(d => d.OwnerId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("book_owner_id_fkey");
        });

        // 2. ТАБЛИЦЯ: book_review
        modelBuilder.Entity<BookReview>(entity =>
        {
            entity.ToTable("book_review");
            entity.HasKey(e => e.ReviewId).HasName("book_review_pkey");
            entity.HasIndex(e => new { e.ReviewerId, e.BookId }, "book_review_reviewer_id_book_id_key").IsUnique();

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.BookId).HasColumnName("book_id").IsRequired();
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id").IsRequired();
            entity.Property(e => e.Rating).HasColumnName("rating").IsRequired();
            entity.Property(e => e.Comment).HasColumnName("comment_");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at").IsRequired();

            entity.HasOne(d => d.Book).WithMany(p => p.BookReviews).HasForeignKey(d => d.BookId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("book_review_book_id_fkey");
            entity.HasOne(d => d.Reviewer).WithMany().HasForeignKey(d => d.ReviewerId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("book_review_reviewer_id_fkey");
        });

        // 3. ТАБЛИЦЯ: category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("category");
            entity.HasKey(e => e.CategoryId).HasName("category_pkey");
            entity.HasIndex(e => e.CategoryName, "category_category_name_key").IsUnique();

            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CategoryName).HasMaxLength(100).HasColumnName("category_name").IsRequired();
        });

        // 4. ТАБЛИЦЯ: chat_message
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.ToTable("chat_message");
            entity.HasKey(e => e.MessageId).HasName("chat_message_pkey");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id").IsRequired();
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id").IsRequired();
            entity.Property(e => e.Content).HasColumnName("content_").IsRequired();
            entity.Property(e => e.SentAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("sent_at").IsRequired();

            entity.HasOne(d => d.Receiver).WithMany().HasForeignKey(d => d.ReceiverId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("chat_message_receiver_id_fkey");
            entity.HasOne(d => d.Sender).WithMany().HasForeignKey(d => d.SenderId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("chat_message_sender_id_fkey");
        });

        // 5. ТАБЛИЦЯ: email_confirmation
        modelBuilder.Entity<EmailConfirmation>(entity =>
        {
            entity.ToTable("email_confirmation");
            entity.HasKey(e => e.ConfirmationId).HasName("email_confirmation_pkey");
            entity.HasIndex(e => e.ConfirmationToken, "email_confirmation_confirmation_token_key").IsUnique();
            entity.HasIndex(e => e.UserId, "email_confirmation_user_id_key").IsUnique();

            entity.Property(e => e.ConfirmationId).HasColumnName("confirmation_id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.ConfirmationToken).HasMaxLength(100).HasColumnName("confirmation_token").IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at").IsRequired();
            entity.Property(e => e.ExpiresAt).HasColumnType("timestamp without time zone").HasColumnName("expires_at").IsRequired();

            entity.HasOne(d => d.User).WithOne().HasForeignKey<EmailConfirmation>(d => d.UserId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("email_confirmation_user_id_fkey");
        });

        // 6. ТАБЛИЦЯ: faculty
        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.ToTable("faculty");
            entity.HasKey(e => e.FacultyId).HasName("faculty_pkey");
            entity.HasIndex(e => e.FacultyName, "faculty_faculty_name_key").IsUnique();

            entity.Property(e => e.FacultyId).HasColumnName("faculty_id");
            entity.Property(e => e.FacultyName).HasMaxLength(100).HasColumnName("faculty_name").IsRequired();
        });

        // 7. ТАБЛИЦЯ: favorite
        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.ToTable("favorite");
            entity.HasKey(e => e.FavoriteId).HasName("favorite_pkey");
            entity.HasIndex(e => new { e.UserId, e.BookId }, "favorite_user_id_book_id_key").IsUnique();

            entity.Property(e => e.FavoriteId).HasColumnName("favorite_id");
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.BookId).HasColumnName("book_id").IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at").IsRequired();

            entity.HasOne(d => d.Book).WithMany(p => p.Favorites).HasForeignKey(d => d.BookId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("favorite_book_id_fkey");
            entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("favorite_user_id_fkey");
        });

        // 8. ТАБЛИЦЯ: image
        modelBuilder.Entity<Image>(entity =>
        {
            entity.ToTable("image");
            entity.HasKey(e => e.ImageId).HasName("image_pkey");
            entity.HasIndex(e => e.ImagePath, "image_image_path_key").IsUnique();

            entity.Property(e => e.ImageId).HasColumnName("image_id");
            entity.Property(e => e.ImagePath).HasMaxLength(255).HasColumnName("image_path").IsRequired();
            entity.Property(e => e.ImageType).HasMaxLength(20).HasColumnName("image_type").IsRequired();
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("uploaded_at").IsRequired();
        });

        // 9. ТАБЛИЦЯ: reservation_queue
        modelBuilder.Entity<ReservationQueue>(entity =>
        {
            entity.ToTable("reservation_queue");
            entity.HasKey(e => e.QueueId).HasName("reservation_queue_pkey");
            entity.HasIndex(e => new { e.UserId, e.BookId }, "reservation_queue_user_id_book_id_key").IsUnique();

            entity.Property(e => e.QueueId).HasColumnName("queue_id");
            entity.Property(e => e.BookId).HasColumnName("book_id").IsRequired();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at").IsRequired();

            entity.HasOne(d => d.Book).WithMany(p => p.ReservationQueues).HasForeignKey(d => d.BookId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("reservation_queue_book_id_fkey");
            entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("reservation_queue_user_id_fkey");
        });

        // 10. ТАБЛИЦЯ: role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("role");
            entity.HasKey(e => e.Id).HasName("role_pkey");

            entity.Property(e => e.Id).HasColumnName("role_id");
            entity.Property(e => e.Name).HasMaxLength(50).HasColumnName("role_name");
            entity.Property(e => e.NormalizedName).HasMaxLength(256).HasColumnName("NormalizedName");
            entity.Property(e => e.ConcurrencyStamp).HasColumnName("ConcurrencyStamp");
        });

        // 11. ТАБЛИЦЯ: user
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(e => e.Id).HasName("user_pkey");

            entity.Property(e => e.Id).HasColumnName("user_id");
            entity.Property(e => e.FirstName).HasMaxLength(50).HasColumnName("first_name").IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).HasColumnName("last_name").IsRequired();
            entity.Property(e => e.FacultyId).HasColumnName("faculty_id").IsRequired();
            entity.Property(e => e.RoleId).HasDefaultValue(2).HasColumnName("role_id").IsRequired();
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active").IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at").IsRequired();

            // Мапінг стандартних полів Identity
            entity.Property(e => e.Email).HasMaxLength(100).HasColumnName("email");
            entity.Property(e => e.PasswordHash).HasMaxLength(255).HasColumnName("password_hash");
            entity.Property(e => e.UserName).HasColumnName("UserName");
            entity.Property(e => e.NormalizedUserName).HasColumnName("NormalizedUserName");
            entity.Property(e => e.NormalizedEmail).HasColumnName("NormalizedEmail");
            entity.Property(e => e.EmailConfirmed).HasColumnName("EmailConfirmed");
            entity.Property(e => e.SecurityStamp).HasColumnName("SecurityStamp");
            entity.Property(e => e.ConcurrencyStamp).HasColumnName("ConcurrencyStamp");
            entity.Property(e => e.PhoneNumber).HasColumnName("PhoneNumber");
            entity.Property(e => e.PhoneNumberConfirmed).HasColumnName("PhoneNumberConfirmed");
            entity.Property(e => e.TwoFactorEnabled).HasColumnName("TwoFactorEnabled");
            entity.Property(e => e.LockoutEnd).HasColumnName("LockoutEnd");
            entity.Property(e => e.LockoutEnabled).HasColumnName("LockoutEnabled");
            entity.Property(e => e.AccessFailedCount).HasColumnName("AccessFailedCount");

            entity.HasOne(d => d.Faculty).WithMany(p => p.Users).HasForeignKey(d => d.FacultyId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("user_faculty_id_fkey");
            entity.HasOne(d => d.Role).WithMany(p => p.Users).HasForeignKey(d => d.RoleId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("user_role_id_fkey");
        });

        // 12. ТАБЛИЦЯ: user_review
        modelBuilder.Entity<UserReview>(entity =>
        {
            entity.ToTable("user_review");
            entity.HasKey(e => e.ReviewId).HasName("user_review_pkey");
            entity.HasIndex(e => new { e.OwnerId, e.ReviewerId }, "user_review_owner_id_reviewer_id_key").IsUnique();

            entity.Property(e => e.ReviewId).HasColumnName("review_id");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id").IsRequired();
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id").IsRequired();
            entity.Property(e => e.Rating).HasColumnName("rating").IsRequired();
            entity.Property(e => e.Comment).HasColumnName("comment_");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnType("timestamp without time zone").HasColumnName("created_at").IsRequired();

            entity.HasOne(d => d.Owner).WithMany().HasForeignKey(d => d.OwnerId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("user_review_owner_id_fkey");
            entity.HasOne(d => d.Reviewer).WithMany().HasForeignKey(d => d.ReviewerId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("user_review_reviewer_id_fkey");
        });

        // ------------------ SEEDING (Тестові Дані) ------------------
        modelBuilder.Entity<Faculty>().HasData(new Faculty { FacultyId = 1, FacultyName = "Прикладна математика та інформатика" });
        modelBuilder.Entity<Category>().HasData(new Category { CategoryId = 1, CategoryName = "Програмування" });
        modelBuilder.Entity<Role>().HasData(new Role { Id = 2, Name = "authorized", NormalizedName = "AUTHORIZED", ConcurrencyStamp = "STATIC-ROLE-STAMP-0000" });

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
            EmailConfirmed = true,
            PasswordHash = "AQAAAAIAAYagAAAAEKA2vL5nQ69q4rQxG+E+mO2e8q1b9wXYZ...",
            SecurityStamp = "STATIC-STAMP-1111-2222-3333",
            ConcurrencyStamp = "STATIC-USER-STAMP-4444",
        };
        modelBuilder.Entity<User>().HasData(testUser);

        modelBuilder.Entity<Book>().HasData(new Book
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
    }
}