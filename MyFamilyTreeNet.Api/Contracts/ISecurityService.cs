    public interface ISecurityService
    {
        string SanitizeInput(string input);
        bool IsValidInput(string input);
        bool ContainsHtml(string input);
        string EscapeHtml(string input);
        bool ValidateCSRFToken(HttpContext context);
    }