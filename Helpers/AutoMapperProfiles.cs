using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
                .ForMember(targetObject => targetObject.Longtitude, memberOptions => memberOptions.MapFrom(prop => prop.Location.X));
            CreateMap<MovieTheaterCreationDTO, MovieTheater>()
                .ForMember(targetObject => targetObject.Location, memberOptions => memberOptions.MapFrom(sourceObject =>
                    geometryFactory.CreatePoint(new Coordinate(sourceObject.Longtitude, sourceObject.Latitude))));
            CreateMap<MovieTheaterEditingDTO, MovieTheater>()
                .ForMember(targetObject => targetObject.Location, memberOptions => memberOptions.MapFrom(sourceObject =>
                    geometryFactory.CreatePoint(new Coordinate(sourceObject.Longtitude, sourceObject.Latitude))));

            CreateMap<MovieCreationDTO, Movie>()
                .ForMember(targetObject=> targetObject.Poster, memberOptions => memberOptions.Ignore())
                .ForMember(targetObject => targetObject.MoviesGenres, memberOptions => memberOptions.MapFrom(MapMoviesGenres))
                .ForMember(targetObject => targetObject.MovieTheatersMovies, memberOptions => memberOptions.MapFrom(MapMovieTheatersMovies))
                .ForMember(targetObject => targetObject.MoviesActors, memberOptions => memberOptions.MapFrom(MapMoviesActors));
            CreateMap<Movie, MovieDTO>()
                .ForMember(m => m.Genres, memberOptions => memberOptions.MapFrom(MapMoviesGenres))
                .ForMember(m => m.MovieTheaters, memberOptions => memberOptions.MapFrom(MapMovieTheatersMovies))
                .ForMember(m => m.Actors, memberOptions => memberOptions.MapFrom(MapMoviesActors));

            CreateMap<IdentityUser, UserDTO>();
        }

        private List<MovieActorDTO> MapMoviesActors(Movie movie, MovieDTO movieDTO)
        {
            var result = new List<MovieActorDTO>();

            if (movie.MoviesActors != null)
            {
                foreach (var moviesActors in movie.MoviesActors)
                {
                    result.Add(new MovieActorDTO()
                    {
                        Id = moviesActors.ActorId,
                        Name = moviesActors.Actor.Name,
                        Character = moviesActors.Character,
                        Picture = moviesActors.Actor.Picture,
                        Order = moviesActors.Order
                    });
                }
            }

            return result;
        }

        private List<MovieTheaterDTO> MapMovieTheatersMovies(Movie movie, MovieDTO movieDTO)
        {
            var result = new List<MovieTheaterDTO>();

            if (movie.MovieTheatersMovies != null)
            {
                foreach (var movieTheaterMovies in movie.MovieTheatersMovies)
                {
                    result.Add(new MovieTheaterDTO() { Id = movieTheaterMovies.MovieTheaterId,
                        Name = movieTheaterMovies.MovieTheater.Name,
                        Latitude = movieTheaterMovies.MovieTheater.Location.Y,
                        Longtitude = movieTheaterMovies.MovieTheater.Location.X
                    });
                }
            }

            return result;
        }

        private List<GenreDTO> MapMoviesGenres(Movie movie, MovieDTO movieDTO)
        {
            var result = new List<GenreDTO>();

            if (movie.MoviesGenres != null)
            {
                foreach (var genre in movie.MoviesGenres)
                {
                    result.Add(new GenreDTO() { Id = genre.GenreId, Name = genre.Genre.Name });
                }
            }

            return result;
        }

        private List<MoviesGenres> MapMoviesGenres(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MoviesGenres>();

            if (movieCreationDTO.GenresIds == null)
            {
                return result;
            }

            foreach (var id in movieCreationDTO.GenresIds)
            {
                result.Add(new MoviesGenres() { GenreId = id });
            }

            return result;
        }

        private List<MovieTheatersMovies> MapMovieTheatersMovies(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MovieTheatersMovies>();

            if (movieCreationDTO.MovieTheatersIds == null)
            {
                return result;
            }

            foreach (var id in movieCreationDTO.MovieTheatersIds)
            {
                result.Add(new MovieTheatersMovies() { MovieTheaterId = id });
            }

            return result;
        }

        private List<MoviesActors> MapMoviesActors(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MoviesActors>();

            if (movieCreationDTO.MovieTheatersIds == null)
            {
                return result;
            }

            foreach (var actor in movieCreationDTO.Actors)
            {
                result.Add(new MoviesActors() { ActorId = actor.Id, Character = actor.Character });
            }

            return result;
        }
    }
}
