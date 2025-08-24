namespace TechFood_Solutions.Models
{
    public class Restaurant
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string LogoUrl { get; set; }
        public string Descripcion { get; set; }

        public ICollection<MenuItem> MenuItems { get; set; }
    }
}
