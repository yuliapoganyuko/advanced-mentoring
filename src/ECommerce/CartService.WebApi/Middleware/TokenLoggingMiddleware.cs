using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class TokenLoggingMiddleware
{
	private readonly RequestDelegate next;
	private readonly ILogger<TokenLoggingMiddleware> logger;

	public TokenLoggingMiddleware(RequestDelegate next, ILogger<TokenLoggingMiddleware> logger)
	{
		this.next = next;
		this.logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var user = context.User;
		if (user?.Identity?.IsAuthenticated == true)
		{
			var sub = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var roles = string.Join(',', user.FindAll(ClaimTypes.Role).Select(c => c.Value));
			var iat = user.FindFirst("iat")?.Value;
			var exp = user.FindFirst("exp")?.Value;

			logger.LogInformation("Identity token details: sub={Sub}, roles={Roles}, iat={Iat}, exp={Exp}",
				sub, roles, iat, exp);
		}

		await next(context);
	}
}