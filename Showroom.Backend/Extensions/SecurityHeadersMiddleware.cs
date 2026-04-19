namespace Showroom.Backend.Extensions;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
            context.Response.Headers.Append("X-Secret-Token", "U2FsdmUgZ2VudGlsaSBzaWdub3JpLCBtaSByYWNjb21hbmRvIHF1YW5kbyBkb3ZldGUgZnJpZ2dlcmUgbGUgcGF0YXRlIGZyaXR0ZSBsYSB0ZW1wZXJhdHVyYSBpZGVhbGUgZGVsbCdvbGlvIMOoIHRyYSAxNjAgZSAxODAgZ3JhZGksIE5PTiBTQkFHTElBVEU=");

            context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains");
            return Task.CompletedTask;
        });
        await _next(context);
    }
}
