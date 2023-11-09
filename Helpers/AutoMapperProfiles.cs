using AutoMapper;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using NetTopologySuite.Geometries;

namespace MoviesAPI.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            CreateMap<GenreDTO, Genre>().ReverseMap();
            CreateMap<GenreCreationDTO, Genre>();

            CreateMap<ActorDTO, Actor>().ReverseMap();
            CreateMap<ActorCreationDTO, Actor>();
            CreateMap<ActorEditingDTO, Actor>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<MovieTheater, MovieTheaterDTO>()
                .ForMember(targetObject => targetObject.Latitude, memberOptions => memberOptions.MapFrom(prop => prop.Location.Y))
                .ForMember(targetObject => targetObject.Longitude, memberOptions => memberOptions.MapFrom(prop => prop.Location.X));
            CreateMap<MovieTheaterCreationDTO, MovieTheater>()
                .ForMember(targetObject => targetObject.Location, memberOptions => memberOptions.MapFrom(sourceObject =>
                    geometryFactory.CreatePoint(new Coordinate(sourceObject.Latitude, sourceObject.Longtitude))));
        }
    }
}
