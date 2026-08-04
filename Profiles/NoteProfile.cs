using AutoMapper;

namespace NoteManagementAPI.Profiles
{
    public class NoteProfile : Profile
    {
        public NoteProfile()
        {
            CreateMap<Models.Note, DTOs.NoteDTO>();
            CreateMap<Models.Note, DTOs.NoteWithoutTagsDTO>();
            CreateMap<DTOs.NoteCreationDTO, Models.Note>()
                .ForMember(destination => destination.Id, options => options.Ignore())
                .ForMember(destination => destination.Tags, options => options.Ignore())
                .ForMember(destination => destination.OwnerUserId, options => options.Ignore())
                .ForMember(destination => destination.Owner, options => options.Ignore())
                .ForMember(destination => destination.CreatedBy, options => options.Ignore())
                .ForMember(destination => destination.CreatedAt, options => options.Ignore())
                .ForMember(destination => destination.ModifiedBy, options => options.Ignore())
                .ForMember(destination => destination.ModifiedAt, options => options.Ignore())
                .ForMember(destination => destination.IsDeleted, options => options.Ignore());
            CreateMap<DTOs.NoteUpdateDTO, Models.Note>()
                .ForMember(destination => destination.Id, options => options.Ignore())
                .ForMember(destination => destination.Tags, options => options.Ignore())
                .ForMember(destination => destination.OwnerUserId, options => options.Ignore())
                .ForMember(destination => destination.Owner, options => options.Ignore())
                .ForMember(destination => destination.CreatedBy, options => options.Ignore())
                .ForMember(destination => destination.CreatedAt, options => options.Ignore())
                .ForMember(destination => destination.ModifiedBy, options => options.Ignore())
                .ForMember(destination => destination.ModifiedAt, options => options.Ignore())
                .ForMember(destination => destination.IsDeleted, options => options.Ignore());
        }
    }
}
