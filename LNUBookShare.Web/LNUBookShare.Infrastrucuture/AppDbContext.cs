using LNUBookShare.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure;

public partial class AppDbContext : IdentityDbContext<User, Role, int>
{
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

    public override DbSet<User> Users { get; set; }

    public override DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");

            // Твої кастомні поля
            entity.Property(e => e.Id).HasColumnName("user_id");
            entity.Property(e => e.FirstName).HasMaxLength(50).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasMaxLength(50).HasColumnName("last_name");
            entity.Property(e => e.FacultyId).HasColumnName("faculty_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id").HasDefaultValue(2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()").HasColumnName("created_at");

            // 👇 Ось те саме поле IsActive, через яке падала база
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            // --- МАПІНГ ТІНЬОВОГО КЛЮЧА ДЛЯ АВАТАРКИ ---
            entity.Property<int?>("ImageId").HasColumnName("avatar_id");

            // --- МАПІНГ ПОЛІВ IDENTITY ---
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
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

            entity.HasOne(d => d.Faculty)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("role");
            entity.Property(e => e.Id).HasColumnName("role_id");
            entity.Property(e => e.Name).HasMaxLength(50).HasColumnName("role_name");

            entity.Property(e => e.NormalizedName).HasColumnName("NormalizedName");
            entity.Property(e => e.ConcurrencyStamp).HasColumnName("ConcurrencyStamp");
        });

        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.HasKey(e => e.FacultyId);
            entity.ToTable("faculty");
            entity.Property(e => e.FacultyId).HasColumnName("faculty_id");
            entity.Property(e => e.FacultyName).HasColumnName("faculty_name");
        });

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.MessageId);
            entity.ToTable("chat_message");
            entity.Property(e => e.MessageId).HasColumnName("message_id");
        });

        modelBuilder.Entity<BookReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId);
            entity.ToTable("book_review");
            entity.Property(e => e.ReviewId).HasColumnName("review_id");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId);
            entity.ToTable("category");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId);
            entity.ToTable("book");
            entity.Property(e => e.BookId).HasColumnName("book_id");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId);
            entity.ToTable("favorite");
            entity.Property(e => e.FavoriteId).HasColumnName("favorite_id");
        });

        modelBuilder.Entity<Image>(entity =>
        {
            entity.HasKey(e => e.ImageId);
            entity.ToTable("image");
            entity.Property(e => e.ImageId).HasColumnName("image_id");
        });

        modelBuilder.Entity<ReservationQueue>(entity =>
        {
            entity.HasKey(e => e.QueueId);
            entity.ToTable("reservation_queue");
            entity.Property(e => e.QueueId).HasColumnName("queue_id");
        });

        modelBuilder.Entity<EmailConfirmation>(entity =>
        {
            entity.HasKey(e => e.ConfirmationId);
            entity.ToTable("email_confirmation");
            entity.Property(e => e.ConfirmationId).HasColumnName("confirmation_id");
        });

        modelBuilder.Entity<UserReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId);
            entity.ToTable("user_review");
            entity.Property(e => e.ReviewId).HasColumnName("review_id");
        });

        modelBuilder.Entity<Faculty>().HasData(
            new Faculty
            {
                FacultyId = 1,
                FacultyName = "Прикладна математика та інформатика",
            });

        modelBuilder.Entity<Role>().HasData(new Role
        {
            Id = 2,
            Name = "authorized",
            NormalizedName = "AUTHORIZED",
        });

        this.OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}