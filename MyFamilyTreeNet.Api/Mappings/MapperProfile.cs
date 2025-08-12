using AutoMapper;
using MyFamilyTreeNet.Data.Models;
using MyFamilyTreeNet.Api.DTOs;

namespace MyFamilyTreeNet.Api.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();
            
            // Family mappings
            CreateMap<Family, FamilyDto>()
                .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.FamilyMembers.Count))
                .ForMember(dest => dest.PhotoCount, opt => opt.MapFrom(src => src.Photos.Count))
                .ForMember(dest => dest.StoryCount, opt => opt.MapFrom(src => src.Stories.Count));
            
            // Member mappings
            CreateMap<FamilyMember, FamilyMemberDto>()
                .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : ""))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => 
                    src.DateOfBirth.HasValue 
                        ? (src.DateOfDeath.HasValue 
                            ? (int?)(src.DateOfDeath.Value - src.DateOfBirth.Value).TotalDays / 365
                            : (int?)(DateTime.Now - src.DateOfBirth.Value).TotalDays / 365)
                        : null));
            
            // Photo mappings
            CreateMap<Photo, PhotoDto>()
                .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : ""))
                .ForMember(dest => dest.UploadedByUserId, opt => opt.MapFrom(src => src.UploadedByUserId));
            
            // Story mappings
            CreateMap<Story, StoryDto>()
                .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.Name : ""));
        }
    }
}