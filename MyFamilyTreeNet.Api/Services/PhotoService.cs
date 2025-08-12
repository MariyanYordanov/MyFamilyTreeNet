using Microsoft.EntityFrameworkCore;
using MyFamilyTreeNet.Api.Contracts;
using MyFamilyTreeNet.Data;
using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly AppDbContext _context;

        public PhotoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Photo>> GetFamilyPhotosAsync(int familyId)
        {
            return await _context.Photos
                .Where(p => p.FamilyId == familyId)
                .ToListAsync();
        }

        public async Task<Photo?> GetPhotoByIdAsync(int id)
        {
            return await _context.Photos
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Photo> CreatePhotoAsync(Photo photo)
        {
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();
            
            return photo;
        }

        public async Task<Photo?> UpdatePhotoAsync(int id, Photo photo)
        {
            var existing = await _context.Photos.FindAsync(id);
            if (existing == null)
            {
                return null;
            }

            existing.Title = photo.Title;
            existing.Description = photo.Description;
            existing.ImageUrl = photo.ImageUrl;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeletePhotoAsync(int id)
        {
            var photo = await _context.Photos.FindAsync(id);
            if (photo == null)
            {
                return false;
            }

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}