using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TareasMVC.Entidades;
using TareasMVC.Models;
using TareasMVC.Servicios;

namespace TareasMVC.Controllers
{
    [Route("api/tareas")]
    public class TareasController: ControllerBase //para usar data en JSON usamos ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IMapper mapper;

        public TareasController(
            ApplicationDbContext context, 
            IServicioUsuarios servicioUsuarios,
            IMapper mapper) 
        {
            this.context = context;
            this.servicioUsuarios = servicioUsuarios;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<Tarea>> Post([FromBody] string titulo)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var existenTareas = await context.Tareas.AnyAsync(t=> t.UsuarioCreacionId == usuarioId);
            var ordenMayor = 0;
            if (existenTareas)
            {
                ordenMayor = await context.Tareas.Where(t=> t.UsuarioCreacionId==usuarioId).Select(t=> t.Orden).MaxAsync(); //el mayor
            }

            var tarea = new Tarea
            {
                Titulo = titulo,
                UsuarioCreacionId = usuarioId,
                FechaCreacion = DateTime.UtcNow,
                Orden = ordenMayor + 1
            };

            //añadimos la tarea a BD
            context.Add(tarea);
            await context.SaveChangesAsync();
            return tarea;
        }

        //Obtener el listado de tareas por usuario. El Get se invoca desde js
        public async Task<List<TareaDTO>> Get()
        {

            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tareas = await context.Tareas
                .Where(t=>t.UsuarioCreacionId ==usuarioId)
                .OrderBy(t=>t.Orden)

                //usando mapper para mapear de Tareas a TareaDTO desde el modelo AutomapperProfiles
                .ProjectTo<TareaDTO>(mapper.ConfigurationProvider)

                /* mapeo manual de Tareas a TareaDTO
                .Select(t=> new TareaDTO
                {
                   Id= t.Id,
                   Titulo= t.Titulo
                })
                */

                .ToListAsync();
            // con Ok retornamos codigo de respuesta 200 y tareas se serializa como un JSON en el cuerpo de la respuesta
            return tareas; 
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Tarea>> Get (int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId ();
            var tarea = await context.Tareas
                .Include(t=> t.Pasos.OrderBy(p=> p.Orden)) // join con tabla Pasos de la tarea de un usuario concreto
                .Include(t=> t.ArchivosAdjuntos.OrderBy(a=> a.Orden))
                .FirstOrDefaultAsync(t=> t.Id == id && t.UsuarioCreacionId ==usuarioId);

            if (tarea is null) 
            {
                return NotFound();
            }
            return tarea;
        }

        [HttpPut("id:int")]
        public async Task<IActionResult> EditarTarea(int id, [FromBody] TareaEditarDTO tareaEditarDTO)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tarea = await context.Tareas.FirstOrDefaultAsync(t=> t.Id == id && t.UsuarioCreacionId==usuarioId);

            if (tarea is null)
            {
                return NotFound();
            }
            tarea.Titulo = tareaEditarDTO.Titulo;
            tarea.Descripcion = tareaEditarDTO.Descripcion;

            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tarea = await context.Tareas.FirstOrDefaultAsync(t=> t.Id == id && t.UsuarioCreacionId==usuarioId);

            if (tarea is null)
            {
                return NotFound();
            }
            context.Remove(tarea);
            await context.SaveChangesAsync();
            return Ok();

        }

        [HttpPost("ordenar")]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tareas = await context.Tareas.Where(t=> t.UsuarioCreacionId==usuarioId).ToListAsync();
            var tareasId = tareas.Select(t => t.Id);  
            var idsTareasNoPertenecenAUsuario= ids.Except(tareasId).ToList();

            if (idsTareasNoPertenecenAUsuario.Any())
            {
                return Forbid();
            }
            var tareasDiccionario = tareas.ToDictionary(x => x.Id);

            for (int i = 0; i < ids.Length; i++)
            {
                var id = ids[i];
                var tarea = tareasDiccionario[id];
                tarea.Orden = i + 1;
            }

            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
