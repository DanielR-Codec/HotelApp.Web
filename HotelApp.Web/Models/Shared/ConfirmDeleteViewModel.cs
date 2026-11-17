namespace HotelApp.Web.Models.Shared
{
    /// <summary>Data of the shared confirmation page used by every module before deactivating / deleting.</summary>
    public class ConfirmDeleteViewModel
    {
        public int Id { get; set; }

        /// <summary>For example "el hotel", "la habitación".</summary>
        public string EntityName { get; set; } = string.Empty;

        /// <summary>"Desactivar" (soft delete) or "Eliminar" (hard delete).</summary>
        public string ActionVerb { get; set; } = "Desactivar";

        public string? Warning { get; set; }

        /// <summary>Label → value pairs that describe the record.</summary>
        public List<KeyValuePair<string, string>> Fields { get; set; } = new();
    }
}
