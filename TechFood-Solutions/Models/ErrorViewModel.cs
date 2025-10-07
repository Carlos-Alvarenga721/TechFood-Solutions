namespace TechFood_Solutions.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = "Ha ocurrido un error inesperado.";

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}