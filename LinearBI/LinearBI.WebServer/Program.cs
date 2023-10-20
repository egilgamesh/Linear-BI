using System.Net;
using System.Text;

namespace LinearBI.WebServer;

public class Program
{
	// ReSharper disable once ArrangeTypeMemberModifiers

	// ReSharper disable once TooManyDeclarations
	public static async Task Main(string[] args)
	{
		var listener = new HttpListener();
		listener.Prefixes.Add(WebServer); // Set your desired URL and port
		listener.Start();
		Console.WriteLine("Server is running...");
		//routeHandlers = new Dictionary<string, Func<HttpListenerContext, Task>>
		//{
		//	{ "/home", (context) => RenderContent(context, ReportsPath + "home.html", 200) },
		//	{ "/about", (context) => RenderContent(context, ReportsPath + "about.html", 200) },
		//	{ "/notfound", (context) => RenderContent(context, "notfound.html", 404) },
		//	// Add more routes and content functions here
		//};
		routeHandlers = new Dictionary<string, Func<HttpListenerContext, Task>>();

		// Scan the directory for HTML files and generate route handlers
		foreach (var file in Directory.GetFiles(ReportsPath, "*.html"))
		{
			var route = "/" + Path.GetFileNameWithoutExtension(file);
			routeHandlers[route] = (context) => RenderContent(context, file, 200);
		}
		while (true)
		{
			var context = await listener.GetContextAsync().ConfigureAwait(false);
			await ProcessRequestAsync(context, routeHandlers).ConfigureAwait(false);
		}
	}

	const string ReportsPath = "Reports/";
	private const string Port = ":8080/";
	private const string ServerAddress = "http://localhost";
	const string WebServer = ServerAddress + Port;
	private static Dictionary<string, Func<HttpListenerContext, Task>>? routeHandlers;

	static async Task ProcessRequestAsync(HttpListenerContext context,
		IReadOnlyDictionary<string, Func<HttpListenerContext, Task>> reportsRouteHandlers)
	{
		var requestUrl = context.Request.RawUrl;
		if (reportsRouteHandlers.ContainsKey(requestUrl!))
		{
			var handler = reportsRouteHandlers[requestUrl!];
			await handler(context).ConfigureAwait(false);
		}
		else
		{
			await RenderContent(context, "notfound.html", 404).ConfigureAwait(false);
		}
	}

	static async Task RenderContent(HttpListenerContext context, string fileName,
		int statusCode)
	{
		context.Response.StatusCode = statusCode;
		var navigationLinks = GenerateNavigationLinks(routeHandlers.Keys);
		var template = await File.ReadAllTextAsync("layout.html").ConfigureAwait(false);
		var content = await File.ReadAllTextAsync(fileName).ConfigureAwait(false);
		template = template.Replace("{{content}}", content);
		template = template.Replace("{{navigation}}", navigationLinks);
		var buffer = Encoding.UTF8.GetBytes(template);
		context.Response.ContentLength64 = buffer.Length;
		var output = context.Response.OutputStream;
		await output.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
		output.Close();
	}

	private static string GenerateNavigationLinks(IEnumerable<string> routes)
	{
		var navigationLinks = new StringBuilder();
		foreach (var route in routes)
			navigationLinks.Append($"<li><a href=\"{route}\">{route[1..]}</a></li>");
		return navigationLinks.ToString();
	}

	private static string GetContent(string reportName)
	{
		return "<b>Welcome to the Home Page!</b>";
	}
}