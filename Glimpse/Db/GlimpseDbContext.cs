using Glimpse.Db.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Glimpse.Db;

public class GlimpseDbContext : IdentityDbContext<User>
{
    private IDbContextTransaction _transaction;
    public GlimpseDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Entry> Entries { get; set; }

    public DbSet<Profile> Profiles { get; set; }

    public DbSet<Snapshot> Snapshots { get; set; }

    public DbSet<Subject> Subjects { get; set; }       

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        base.OnModelCreating(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>().AreUnicode(false);
        configurationBuilder.Properties<Enum>().HaveColumnType("varchar(255)");
        configurationBuilder.Properties<decimal>().HaveColumnType("decimal(19,4)");            
        base.ConfigureConventions(configurationBuilder);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        _transaction ??= await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }
    }
    public override int SaveChanges()
    {
        UpdateAuditInfo();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateAuditInfo();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateAuditInfo();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateAuditInfo()
    {
        var principal = Thread.CurrentPrincipal;
        var user = principal != null && principal.Identity.IsAuthenticated
            ? principal.Identity.Name
            : "Unknown";

        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Entity && (e.State == EntityState.Added || e.State == EntityState.Modified))
            .ToList();

        entries.ForEach(e =>
        {
            var entity = (Entity)e.Entity;

            e.Property(nameof(Entity.ModifiedOn)).OriginalValue = entity.ModifiedOn; // Set the original value for concurrency check

            if (e.State == EntityState.Added)
            {
                entity.CreatedOn = DateTime.UtcNow;
                entity.CreatedBy = user;
            }

            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = user;
        });
    }
}    
