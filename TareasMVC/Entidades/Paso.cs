namespace TareasMVC.Entidades
{
    public class Paso
    {
        public Guid Id { get; set; }
        public int TareaId { get; set; }
        public string Descripcion { get; set;}
        public bool Realizado { get; set; }
        public int Orden { get; set; }
        public Tarea Tarea { get; set; } // Propiedad de navegación que permite asociar el paso con su tarea relacionada como PK
    }
}
