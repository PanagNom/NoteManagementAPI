using AutoMapper;

namespace NoteManagementAPI.Profiles
{
    public class NoteProfile : Profile
    {
        public NoteProfile()
        {
            CreateMap<Models.Note, DTOs.NoteDTO>().ReverseMap();
            CreateMap<Models.Note, DTOs.NoteWithoutTagsDTO>().ReverseMap();
            CreateMap<Models.Note, DTOs.NoteCreationDTO>().ReverseMap();
        }
    }
}
