using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using MoviesAPI.Entities;
using MoviesAPI.Helpers;

namespace MoviesAPI.Controllers
{
    [Route("api/actors")]
    [ApiController]
    public class ActorsController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly string containerName = "actors";

        public ActorsController(ApplicationDbContext context, IMapper mapper, IFileStorageService fileStorageService)
        {
            this.context = context;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }
        
        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> GetAllActors([FromQuery] PaginationDTO paginationDTO)
        {
            var queryable = context.Actors.AsQueryable();
            await HttpContext.InsertPaginationToHeader(queryable);
            var actors = await queryable.OrderBy(actor => actor.Name).Paginate(paginationDTO).ToListAsync();
            var actorsDTOs = mapper.Map<List<ActorDTO>>(actors);

            return actorsDTOs;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ActorDTO>> GetActorById(int id)
        {
            var actor = await context.Actors.FirstOrDefaultAsync(actor => actor.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            var actorDTO = mapper.Map<ActorDTO>(actor);

            return actorDTO;
        }

        [HttpPost]
        public async Task<ActionResult> CreateActor([FromForm] ActorCreationDTO actorCreationDTO)
        {
            var actor = mapper.Map<Actor>(actorCreationDTO);

            if (actorCreationDTO.Picture != null)
            {
                actor.Picture = await fileStorageService.SaveFile(containerName, actorCreationDTO.Picture);
            }

            context.Actors.Add(actor);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> EditActorById(int id, [FromForm] ActorEditingDTO actorEditingDTO)
        {
            var actor = await context.Actors.FirstOrDefaultAsync(actor => actor.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            actor = mapper.Map(actorEditingDTO, actor);

            if (actorEditingDTO.Picture != null)
            {
                actor.Picture = await fileStorageService.EditFile(containerName, actor.Picture, actorEditingDTO.Picture);
            }

            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteActorById(int id)
        {
            var actor = context.Actors.FirstOrDefault(actor => actor.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            context.Remove(actor);
            await context.SaveChangesAsync();
            await fileStorageService.DeleteFile(actor.Picture, containerName);

            return NoContent();
        }

    }
}
