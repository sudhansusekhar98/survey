using SurveyApp.Models;

namespace SurveyApp.Repo
{
    public interface IItemRepository
    {
        Task<IEnumerable<ItemMasterModel>> GetAllAsync();
        Task<IEnumerable<ItemMasterModel>> GetByTypeAsync(int typeId);
        Task<ItemMasterModel?> GetByIdAsync(int itemId);
        Task<ItemMasterModel> CreateAsync(ItemMasterModel item);
        Task<ItemMasterModel> UpdateAsync(ItemMasterModel item);
        Task<bool> DeleteAsync(int itemId);
        Task<bool> ExistsAsync(int itemId);
    }
}
