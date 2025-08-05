using MyFamilyTreeNet.Data.Models;

namespace MyFamilyTreeNet.Api.Contracts;

public interface IFamilyService
{
    Task<IEnumerable<Family>> GetAllFamiliesAsync();
}