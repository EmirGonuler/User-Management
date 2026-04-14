namespace UserManagement.Web.Models
{
    /// <summary>
    /// ViewModel used by the shared Error view.
    /// Provided by the default ASP.NET Core MVC template.
    /// </summary>
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
