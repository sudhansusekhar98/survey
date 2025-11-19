using Microsoft.EntityFrameworkCore;
using SurveyApp.Data;
using SurveyApp.Models;

namespace SurveyApp.Repo
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;

        public ItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ItemMasterModel>> GetAllAsync()
        {
            return await _context.ItemMaster
                .Include(x => x.ItemType)
                .Where(x => x.IsActive)
                .OrderBy(x => x.SqNo)
                .ThenBy(x => x.ItemName)
                .ToListAsync();
        }

        public async Task<IEnumerable<ItemMasterModel>> GetByTypeAsync(int typeId)
        {
            return await _context.ItemMaster
                .Include(x => x.ItemType)
                .Where(x => x.TypeId == typeId && x.IsActive)
                .OrderBy(x => x.SqNo)
                .ThenBy(x => x.ItemName)
                .ToListAsync();
        }

        public async Task<ItemMasterModel?> GetByIdAsync(int itemId)
        {
            return await _context.ItemMaster
                .Include(x => x.ItemType)
                .FirstOrDefaultAsync(x => x.ItemId == itemId);
        }

        public async Task<ItemMasterModel> CreateAsync(ItemMasterModel item)
        {
            item.CreatedOn = DateTime.Now;
            item.IsActive = true;

            _context.ItemMaster.Add(item);
            await _context.SaveChangesAsync();

            return item;
        }

        public async Task<ItemMasterModel> UpdateAsync(ItemMasterModel item)
        {
            var existing = await _context.ItemMaster.FindAsync(item.ItemId);
            if (existing == null)
            {
                throw new InvalidOperationException($"ItemMaster with ID {item.ItemId} not found.");
            }

            existing.TypeId = item.TypeId;
            existing.ItemName = item.ItemName;
            existing.ItemCode = item.ItemCode;
            existing.ItemDesc = item.ItemDesc;
            existing.IsActive = item.IsActive;
            existing.SqNo = item.SqNo;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int itemId)
        {
            var item = await _context.ItemMaster.FindAsync(itemId);
            if (item == null)
            {
                return false;
            }

            // Soft delete by setting IsActive to false
            item.IsActive = false;

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int itemId)
        {
            return await _context.ItemMaster.AnyAsync(x => x.ItemId == itemId);
        }
    }
}
