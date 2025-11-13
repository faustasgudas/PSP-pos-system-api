using Microsoft.EntityFrameworkCore;
using PsP.Models;

namespace PsP.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<GiftCard> GiftCards => Set<GiftCard>();
    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();
    public DbSet<Business> Businesses => Set<Business>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<GiftCard>().HasIndex(x => x.Code).IsUnique();
        mb.Entity<GiftCard>().Property(x => x.Balance).HasColumnType("NUMERIC(10,2)");
    }
}