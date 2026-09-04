using CarAuction.Domain.Entities;

namespace CarAuction.Application.Interfaces.Repositories;

public interface IImageRepository
{
    Task<int> CreateAsync(Image image);
    Task<Image?> GetByIdAsync(int id);
    Task DeleteAsync(int id);
    Task<Listing?> GetListingByIdAsync(int id);
    Task<bool> IsUserAdminAsync(int userId);
}
