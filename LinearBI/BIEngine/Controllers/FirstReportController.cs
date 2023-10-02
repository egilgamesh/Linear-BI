using Microsoft.AspNetCore.Mvc;

namespace BI_Engine.Controllers;

[Route("api/[controller]")]
[ApiController]
// ReSharper disable once HollowTypeName
public class FirstReportController : ControllerBase
{
	[HttpGet("/API/GetFirstReport")]
	public async Task<IActionResult> GetFirstReport()
	{
		// Generate HTML content here
		var htmlContent = "<html><body><h1>Hello, World!</h1></body></html>";
		return Content(htmlContent, "text/html");
	}
}