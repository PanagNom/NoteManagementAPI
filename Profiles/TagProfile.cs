using AutoMapper;

namespace NoteManagementAPI.Profiles
{
    public class TagProfile: Profile
    {
        public TagProfile() 
        {
            CreateMap<Models.Tag, DTOs.TagDTO>();
            CreateMap<Models.Tag, DTOs.TagInNoteDTO>();
            CreateMap<DTOs.TagDTOCreate, Models.Tag>()
                .ForMember(destination => destination.Id, options => options.Ignore())
                .ForMember(destination => destination.Notes, options => options.Ignore())
                .ForMember(destination => destination.CreatedBy, options => options.Ignore())
                .ForMember(destination => destination.CreatedDate, options => options.Ignore())
                .ForMember(destination => destination.ModifiedBy, options => options.Ignore())
                .ForMember(destination => destination.ModifiedDate, options => options.Ignore());
            CreateMap<DTOs.TagUpdateDTO, Models.Tag>()
                .ForMember(destination => destination.Id, options => options.Ignore())
                .ForMember(destination => destination.Notes, options => options.Ignore())
                .ForMember(destination => destination.CreatedBy, options => options.Ignore())
                .ForMember(destination => destination.CreatedDate, options => options.Ignore())
                .ForMember(destination => destination.ModifiedBy, options => options.Ignore())
                .ForMember(destination => destination.ModifiedDate, options => options.Ignore());
        }
    }
}
