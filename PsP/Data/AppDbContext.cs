using Microsoft.EntityFrameworkCore;
using PsP.Models;

namespace PsP.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<GiftCard> GiftCards => Set<GiftCard>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<GiftCard>()
            .HasIndex(x => x.Code)
            .IsUnique();

        mb.Entity<GiftCard>()
            .Property(x => x.Balance)
            .HasColumnType("BIGINT");

        mb.Entity<Payment>()
            .HasOne(p => p.GiftCard)
            .WithMany(g => g.Payments)  
            .HasForeignKey(p => p.GiftCardId)
            .OnDelete(DeleteBehavior.SetNull);
        
        mb.Entity<Payment>()
            .HasOne(p => p.Business)
            .WithMany(b => b.Payments)
            .HasForeignKey(p => p.BusinessId);
    }


}