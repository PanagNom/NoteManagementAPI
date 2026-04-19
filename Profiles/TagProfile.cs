using AutoMapper;

namespace NoteManagementAPI.Profiles
{
    public class TagProfile: Profile
    {
        public TagProfile() 
        {
            CreateMap<Models.Tag, DTOs.TagDTO>().ReverseMap();
        }
    }
}
