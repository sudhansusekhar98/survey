using Microsoft.EntityFrameworkCore;
using SurveyApp.Data;
using SurveyApp.Models;

namespace SurveyApp.Repo
{
    public class ItemTypeRepository : IItemTypeRepository
    {
        private readonly AppDbContext _context;

        public ItemTypeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ItemTypeMasterModel>> GetAllAsync()
        {
            return await _context.ItemTypeMaster
                .Where(x => x.IsActive)
                .OrderBy(x => x.TypeName)
                .ToListAsync();
        }

        public async Task<ItemTypeMasterModel?> GetByIdAsync(int id)
        {
            return await _context.ItemTypeMaster
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ItemTypeMasterModel> CreateAsync(ItemTypeMasterModel itemType)
        {
            itemType.CreatedOn = DateTime.Now;
            itemType.IsActive = true;

            _context.ItemTypeMaster.Add(itemType);
            await _context.SaveChangesAsync();

            return itemType;
        }

        public async Task<ItemTypeMasterModel> UpdateAsync(ItemTypeMasterModel itemType)
        {
            var existing = await _context.ItemTypeMaster.FindAsync(itemType.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"ItemTypeMaster with ID {itemType.Id} not found.");
            }

            existing.TypeName = itemType.TypeName;
            existing.TypeDesc = itemType.TypeDesc;
            existing.GroupName = itemType.GroupName;
            existing.IsActive = itemType.IsActive;
            existing.ModifiedDate = DateTime.Now;
            existing.ModifiedBy = itemType.ModifiedBy;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var itemType = await _context.ItemTypeMaster.FindAsync(id);
            if (itemType == null)
            {
                return false;
            }

            // Soft delete by setting IsActive to false
            itemType.IsActive = false;
            itemType.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ItemTypeMaster.AnyAsync(x => x.Id == id);
        }
    }
}
