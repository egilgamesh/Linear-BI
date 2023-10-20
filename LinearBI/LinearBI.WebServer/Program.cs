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
		listener.Prefixes.Add("http://localhost:8080/"); // Set your desired URL and port
		listener.Start();
		Console.WriteLine("Server is running...");
		var routeHandlers = new Dictionary<string, Func<HttpListenerContext, Task>>
		{
			{ "/home", (context) => RenderContent(context, "home.html", 200) },
			{ "/about", (context) => RenderContent(context, "about.html", 200) },
			{ "/notfound", (context) => RenderContent(context, "notfound.html", 404) },
			// Add more routes and content functions here
		};
		while (true)
		{
			var context = await listener.GetContextAsync().ConfigureAwait(false);
			await ProcessRequestAsync(context, routeHandlers).ConfigureAwait(false);
		}
	}

	static async Task ProcessRequestAsync(HttpListenerContext context,
		IReadOnlyDictionary<string, Func<HttpListenerContext, Task>> routeHandlers)
	{
		var requestUrl = context.Request.RawUrl;
		if (routeHandlers.ContainsKey(requestUrl))
		{
			var handler = routeHandlers[requestUrl];
			await handler(context).ConfigureAwait(false);
		}
		else
		{
			await RenderContent(context, "notfound.html", 404).ConfigureAwait(false);
		}
	}

	static async Task RenderContent(HttpListenerContext context, string fileName, int statusCode)
	{
		context.Response.StatusCode = statusCode;
		var template = await File.ReadAllTextAsync("layout.html").ConfigureAwait(false);
		var content = await File.ReadAllTextAsync(fileName).ConfigureAwait(false);
		template = template.Replace("{{content}}", content);
		var buffer = Encoding.UTF8.GetBytes(template);
		context.Response.ContentLength64 = buffer.Length;
		var output = context.Response.OutputStream;
		await output.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
		output.Close();
	}

	private static string GetContent(string reportName)
	{
		return "<b>Welcome to the Home Page!</b>";
	}
}