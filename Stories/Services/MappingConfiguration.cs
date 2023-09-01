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
                .ForMember(dto => dto.PostedBy, opt => opt.MapFrom(story => story.By))
                .ForMember(dto => dto.Uri, opt => opt.MapFrom(story => story.Url))
                .ForMember(dto => dto.Time, opt => opt.MapFrom(story => story.Time.ToDateTime()))
                .ReverseMap()
                .IgnoreAllPropertiesWithAnInaccessibleSetter();
        }
    }
}
