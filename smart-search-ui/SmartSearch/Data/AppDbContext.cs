using Microsoft.EntityFrameworkCore;
using SmartSearch.Models;

namespace SmartSearch.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet для всех сущностей
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            // Первичный ключ
            entity.HasKey(e => e.Id);

            // Индексы
            entity.HasIndex(e => e.UniqId).IsUnique();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Price);
            entity.HasIndex(e => new { e.Category, e.Price }); // Составной индекс

            // Ограничения и типы колонок
            entity.Property(e => e.Title)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(e => e.Description)
                .HasColumnType("text");

            entity.Property(e => e.Category)
                .HasMaxLength(200);

            entity.Property(e => e.SubCategory1)
                .HasMaxLength(200);

            entity.Property(e => e.SubCategory2)
                .HasMaxLength(200);

            entity.Property(e => e.SubCategory3)
                .HasMaxLength(200);

            entity.Property(e => e.Price)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.OriginalPrice)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Currency)
                .HasMaxLength(3)
                .HasDefaultValue("INR");

            entity.Property(e => e.Images)
                .HasColumnType("text");

            entity.Property(e => e.UniqId)
                .HasMaxLength(100);

            entity.Property(e => e.Url)
                .HasMaxLength(500);

            entity.Property(e => e.Pid)
                .HasMaxLength(100);

            entity.Property(e => e.Seller)
                .HasMaxLength(200);

            entity.Property(e => e.ReturnPolicy)
                .HasMaxLength(200);

            entity.Property(e => e.ScrapedAt)
                .HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<User>(entity =>
        {
            // Первичный ключ
            entity.HasKey(e => e.Id);

            // Индексы
            entity.HasIndex(e => e.Email).IsUnique();

            // Ограничения и типы колонок
            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()"); // PostgreSQL генерация UUID

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userEntries = ChangeTracker
            .Entries<User>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in userEntries)
        {
            if (entry.Entity.CreatedAt == default)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}