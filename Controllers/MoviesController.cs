using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoviesAPI.DTOs;
using MoviesAPI.Helpers;
using MoviesAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/movies")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy ="IsAdmin")]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly UserManager<IdentityUser> userManager;
        private string containerName = "movies";

        public MoviesController(ApplicationDbContext context, IMapper mapper, IFileStorageService fileStorageService, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
            this.userManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<LandingPageDTO>> GetLandingPageMovies()
        {
            var top = 6;
            var today = DateTime.Today;

            var upcomingReleases = await context.Movies
                .Where(m => m.ReleaseDate > today)
                .OrderBy(m => m.ReleaseDate)
                .Take(top)
                .ToListAsync();

            var inTheaters = await context.Movies
                .Where(m => m.InTheaters)
                .OrderBy(m => m.ReleaseDate)
                .Take(top)
                .ToListAsync();

            var landingPageDTO = new LandingPageDTO();
            landingPageDTO.UpcomingReleases = mapper.Map<List<MovieDTO>>(upcomingReleases);
            landingPageDTO.InTheaters = mapper.Map<List<MovieDTO>>(inTheaters);

            return landingPageDTO;
        }

        [HttpGet("filter")]
        [AllowAnonymous]
        public async Task<ActionResult<List<MovieDTO>>> Filter([FromQuery] FilterMoviesDTO filterMoviesDTO)
        {
            var moviesQueryable = context.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(filterMoviesDTO.Title))
            {
                moviesQueryable = moviesQueryable.Where(m => m.Title.Contains(filterMoviesDTO.Title));
            }

            if (filterMoviesDTO.InTheaters)
            {
                moviesQueryable = moviesQueryable.Where(m => m.InTheaters);
            }

            if (filterMoviesDTO.UpcomingReleases)
            {
                var today = DateTime.Today;
                moviesQueryable = moviesQueryable.Where(m => m.ReleaseDate > today);
            }

            if (filterMoviesDTO.GenreId != 0)
            {
                moviesQueryable = moviesQueryable
                    .Where(m => m.MoviesGenres.Select(mg => mg.GenreId)
                    .Contains(filterMoviesDTO.GenreId));
            }

            await HttpContext.InsertPaginationToHeader(moviesQueryable);
            var movies = await moviesQueryable.OrderBy(m => m.Title).Paginate(filterMoviesDTO.PaginationDTO)
                .ToListAsync();

            return mapper.Map<List<MovieDTO>>(movies);
        }

        [HttpGet("postget")]
        public async Task<ActionResult<MoviePostGetDTO>> PostGet()
        {
            var movieTheaters = await context.MovieTheaters.ToListAsync();
            var genres = await context.Genres.ToListAsync();

            var movieTheatersDTO = mapper.Map<List<MovieTheaterDTO>>(movieTheaters);
            var genresDTO = mapper.Map<List<GenreDTO>>(genres);

            return new MoviePostGetDTO() { Genres = genresDTO, MovieTheaters = movieTheatersDTO };
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<MovieDTO>> GetMovieById(int id)
        {
            var movie = await context.Movies.Include(m => m.MoviesGenres).ThenInclude(m => m.Genre)
                   .Include(m => m.MovieTheatersMovies).ThenInclude(m => m.MovieTheater)
                   .Include(m => m.MoviesActors).ThenInclude(m => m.Actor)
                   .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            var averageVote = 0.0;
            var userVote = 0;

            if (await context.Ratings.AnyAsync(r => r.MovieId == id))
            {
                averageVote = await context.Ratings.Where(r => r.MovieId == id).AverageAsync(r => r.Rate);

                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    var email = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "email").Value;
                    var user = await userManager.FindByEmailAsync(email);
                    var userId = user.Id;

                    var ratingDb = await context.Ratings.FirstOrDefaultAsync(r => r.MovieId == id && r.UserId == userId);

                    if (ratingDb != null)
                    {
                        userVote = ratingDb.Rate;
                    }
                }
            }

            var movieDTO = mapper.Map<MovieDTO>(movie);
            movieDTO.AverageVote = averageVote;
            movieDTO.UserVote = userVote;
            movieDTO.Actors = movieDTO.Actors.OrderBy(a => a.Order).ToList();

            return movieDTO;
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateMovie([FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = mapper.Map<Movie>(movieCreationDTO);

            if (movieCreationDTO.Poster != null)
            {
                movie.Poster = await fileStorageService.SaveFile(containerName, movieCreationDTO.Poster);
            }

            AnnotateActorsOrder(movie);
            await context.Movies.AddAsync(movie);
            await context.SaveChangesAsync();

            return movie.Id;
        }

        [HttpGet("putget/{id:int}")]
        public async Task<ActionResult<MoviePutGetDTO>> PutGet(int id)
        {
            var movieActionResult = await GetMovieById(id);

            if (movieActionResult.Result is NotFoundResult) 
            { 
                return NotFound(); 
            }

            var movieDTO = movieActionResult.Value;

            var selectedGenresIds = movieDTO.Genres.Select(g => g.Id).ToList();
            var genres = await context.Genres.ToListAsync();

            var movieTheatersIds = movieDTO.MovieTheaters.Select(m => m.Id).ToList();
            var movieTheaters = await context.MovieTheaters.ToListAsync();

            var genresDTOs = mapper.Map<List<GenreDTO>>(genres);
            var movieTheatersDTOs = mapper.Map<List<MovieTheaterDTO>>(movieTheaters);

            var response = new MoviePutGetDTO()
            {
                Movie = movieDTO,
                SelectedGenres = movieDTO.Genres,
                Genres = genresDTOs,
                SelectedMovieTheaters = movieTheatersDTOs,
                MovieTheaters = movieTheatersDTOs,
                Actors = movieDTO.Actors,
            };

            return response;
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> EditMovieById(int id, [FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = await context.Movies.Include(m => m.MoviesActors)
                .Include(m => m.MoviesGenres)
                .Include(m => m.MovieTheatersMovies)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            var movieDTO = mapper.Map(movieCreationDTO, movie);

            if (movieCreationDTO.Poster != null)
            {
                movie.Poster = await fileStorageService.EditFile(containerName, movieDTO.Poster, movieCreationDTO.Poster);
            }

            AnnotateActorsOrder(movieDTO);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteMoviesById(int id)
        {
            var movie = await context.Movies.FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            context.Remove(movie);
            await context.SaveChangesAsync();
            await fileStorageService.DeleteFile(movie.Poster, containerName);

            return NoContent();
        }

        private void AnnotateActorsOrder(Movie movie)
        {
            if (movie.MoviesActors != null)
            {
                for (int i = 0; i < movie.MoviesActors.Count; i++)
                {
                    movie.MoviesActors[i].Order = i;
                }
            }
        }
    }
}
