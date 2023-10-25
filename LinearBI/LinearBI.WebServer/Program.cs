using System.Data;
using System.Net;
using System.Text;
using System.Text.Json;

namespace LinearBI.WebServer;

public class Program
{
	// ReSharper disable once ArrangeTypeMemberModifiers

	// ReSharper disable once TooManyDeclarations
	// ReSharper disable once MethodTooLong
	public static async Task Main(string[] args)
	{
		ReadConfigurationFile();
		var listener = new HttpListener();
		listener.Prefixes.Add(webServer!); // Set your desired URL and port
		listener.Start();
		Console.WriteLine("Server is running...");
		routeHandlers = new Dictionary<string, Func<HttpListenerContext, Task>>();

		// Scan the directory for HTML files and generate route handlers
		foreach (var file in Directory.GetFiles(reportsPath, "*.html"))
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

	private static void ReadConfigurationFile()
	{
		var configText = File.ReadAllText("config/Properties.json");
		var config = JsonSerializer.Deserialize<ConfigurationSetting>(configText);
		reportsPath = config!.ReportsPath;
		port = config.Port;
		serverAddress = config.ServerAddress;
		companyTitle = config.CompanyTitle;
		webServer = serverAddress + port + "/";
	}

	static string reportsPath = "";
	private static string port = "";
	private static string serverAddress = "";
	private static string companyTitle = "";
	private static Dictionary<string, Func<HttpListenerContext, Task>>? routeHandlers;
	private static string? webServer;

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
			await RenderContent(context, "notfound.html", 404).ConfigureAwait(false);
	}

	static async Task RenderContent(HttpListenerContext context, string fileName,
		int statusCode)
	{
		context.Response.StatusCode = statusCode;
		var navigationLinks = GenerateNavigationLinks(routeHandlers!.Keys);
		var template = await ReplaceLayoutPagePlaceHolderContent(fileName, navigationLinks).
			ConfigureAwait(false);
		var buffer = Encoding.UTF8.GetBytes(template);
		context.Response.ContentLength64 = buffer.Length;
		var output = context.Response.OutputStream;
		await output.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
		output.Close();
	}

	private static async Task<string> ReplaceLayoutPagePlaceHolderContent(string fileName,
		string navigationLinks)
	{
		var template = await File.ReadAllTextAsync("layout.html").ConfigureAwait(false);
		var content = await File.ReadAllTextAsync(fileName).ConfigureAwait(false);
		template = template.Replace("{{content}}", content);
		template = template.Replace("{{navigation}}", navigationLinks);
		template = template.Replace("{{CompanyTitle}}", companyTitle);
		template = template.Replace("{{year}}", DateTime.Now.Year.ToString());
		return template;
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