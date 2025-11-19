using SurveyApp.Models;

namespace SurveyApp.Repo
{
    public interface IItemTypeRepository
    {
        Task<IEnumerable<ItemTypeMasterModel>> GetAllAsync();
        Task<ItemTypeMasterModel?> GetByIdAsync(int id);
        Task<ItemTypeMasterModel> CreateAsync(ItemTypeMasterModel itemType);
        Task<ItemTypeMasterModel> UpdateAsync(ItemTypeMasterModel itemType);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
