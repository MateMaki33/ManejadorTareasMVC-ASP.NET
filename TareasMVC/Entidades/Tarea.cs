using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TareasMVC.Entidades
{
    public class Tarea
    {
        public int Id { get; set; }

        [StringLength(250)]
        [Required] 
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public int Orden { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacionId { get; set; } 
        public IdentityUser UsuarioCreacion { get; set; } //Propiedad de navegacion FK de Usuarios
        public  List<ArchivoAdjunto> ArchivosAdjuntos { get; set; } // Propiedad de navegacion FK de Archivos
        public List<Paso> Pasos { get; set; } // Propiedad de navegacion FK de Paso. Esto hace una relacion de 1 a muchos(1 tarea x pasos)
       

    }
}
