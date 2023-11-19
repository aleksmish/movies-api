using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;

namespace MoviesAPI.Controllers
{
    [ApiController]
    [Route("api/movie-theaters")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "IsAdmin")]
    public class MovieTheatersController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public MovieTheatersController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<MovieTheaterDTO>>> GetAllMovieTheaters([FromQuery] PaginationDTO paginationDTO)
        {
            var queryable = context.MovieTheaters.AsQueryable();

            await HttpContext.InsertPaginationToHeader(queryable);

            var movieTheaters = await queryable.OrderBy(movieTheater => movieTheater.Name).Paginate(paginationDTO).ToListAsync();
            var movieTheaterDTOs = mapper.Map<List<MovieTheaterDTO>>(movieTheaters);

            return movieTheaterDTOs;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<MovieTheaterDTO>> GetMovieTheaterById(int id)
        {
            var movieTheater = await context.MovieTheaters.FirstOrDefaultAsync(movieTheater => movieTheater.Id == id);

            if (movieTheater == null)
            {
                return NotFound();
            }

            var movieTheaterDTO = mapper.Map<MovieTheaterDTO>(movieTheater);

            return movieTheaterDTO;
        }

        [HttpPost]
        public async Task<ActionResult> CreateMovieTheater([FromBody] MovieTheaterCreationDTO movieTheaterCreationDTO)
        {
            var movieTheater = mapper.Map<MovieTheater>(movieTheaterCreationDTO);

            await context.MovieTheaters.AddAsync(movieTheater);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> EditMovieTheater(int id, [FromBody] MovieTheaterEditingDTO movieTheaterEditingDTO)
        {
            var movieTheater = await context.MovieTheaters.FirstOrDefaultAsync(movieTheater => movieTheater.Id == id);

            if (movieTheater == null)
            {
                return NotFound();
            }

            mapper.Map(movieTheaterEditingDTO, movieTheater);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteMovieTheater(int id)
        {
            var movieTheater = await context.MovieTheaters.FirstOrDefaultAsync(movieTheater => movieTheater.Id == id);

            if (movieTheater == null)
            {
                return NotFound();
            }

            context.MovieTheaters.Remove(movieTheater);
            await context.SaveChangesAsync();

            return NoContent();
        }
    }
}
