using AutoMapper;
using Stories.Model;

namespace Stories.Services
{
    public class MappingConfiguration : Profile
    {
        public MappingConfiguration()
        {
            CreateMap<Story, StoryDto>()
                .ForMember(dto => dto.CommentsCount, opt => opt.MapFrom(story => story.Kids != null ? story.Kids.Length : 0))
                .ReverseMap()
                .IgnoreAllPropertiesWithAnInaccessibleSetter();
        }
    }
}
