using Microsoft.EntityFrameworkCore;
using SurveyApp.Models;

namespace SurveyApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<ItemTypeMasterModel> ItemTypeMaster { get; set; }
        public DbSet<ItemMasterModel> ItemMaster { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ItemTypeMasterModel
            modelBuilder.Entity<ItemTypeMasterModel>(entity =>
            {
                entity.ToTable("ItemTypeMaster");
                entity.HasKey(e => e.Id);

                // Convert bool to char(1) for IsActive
                entity.Property(e => e.IsActive)
                    .HasConversion(
                        v => v ? 'Y' : 'N',
                        v => v == 'Y')
                    .HasColumnType("char(1)")
                    .HasColumnName("IsActive");

                entity.Property(e => e.TypeName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("TypeName");

                entity.Property(e => e.TypeDesc)
                    .HasMaxLength(300)
                    .HasColumnName("TypeDesc");

                entity.Property(e => e.GroupName)
                    .HasMaxLength(100)
                    .HasColumnName("GroupName");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("CreatedOn")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("CreatedBy");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnName("ModifiedDate");

                entity.Property(e => e.ModifiedBy)
                    .HasColumnName("ModifiedBy");
            });

            // Configure ItemMasterModel
            modelBuilder.Entity<ItemMasterModel>(entity =>
            {
                entity.ToTable("ItemMaster");
                entity.HasKey(e => e.ItemId);

                // Convert bool to char(1) for IsActive
                entity.Property(e => e.IsActive)
                    .HasConversion(
                        v => v ? 'Y' : 'N',
                        v => v == 'Y')
                    .HasColumnType("char(1)")
                    .HasColumnName("IsActive");

                entity.Property(e => e.ItemName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("ItemName");

                entity.Property(e => e.ItemCode)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("ItemCode");

                entity.Property(e => e.ItemDesc)
                    .HasMaxLength(300)
                    .HasColumnName("ItemDesc");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("CreatedOn")
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.CreatedBy)
                    .HasColumnName("CreatedBy");

                entity.Property(e => e.SqNo)
                    .HasColumnName("SqNo");

                // Foreign key relationship
                entity.HasOne(e => e.ItemType)
                    .WithMany(t => t.Items)
                    .HasForeignKey(e => e.TypeId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK_ItemMaster_ItemTypeMaster");
            });
        }
    }
}
