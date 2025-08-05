using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Services
{
    public interface IPhotoService
    {
        Task<List<Photo>> GetFamilyPhotosAsync(int familyId);
        Task<Photo?> GetPhotoByIdAsync(int id);
        Task<Photo> CreatePhotoAsync(Photo photo);
        Task<Photo?> UpdatePhotoAsync(int id, Photo photo);
        Task<bool> DeletePhotoAsync(int id);
    }
}