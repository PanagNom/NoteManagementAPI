using AutoMapper;

namespace NoteManagementAPI.Profiles
{
    public class TagProfile: Profile
    {
        public TagProfile() 
        {
            CreateMap<Models.Tag, DTOs.TagDTO>().ReverseMap();
            CreateMap<Models.Tag, DTOs.TagDTOCreate>().ReverseMap();
            CreateMap<Models.Tag, DTOs.TagUpdateDTO>().ReverseMap();
        }
    }
}
