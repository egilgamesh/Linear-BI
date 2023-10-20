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
		listener.Prefixes.Add($"http://localhost:8080/"); // Set your desired URL and port
		listener.Start();
		Console.WriteLine("Server is running...");
		var routeHandlers = new Dictionary<string, Func<HttpListenerContext, Task>>
		{
			{ "/home", (context) => RenderContent(context, GetContent("Home"), 200) },
			{
				"/about",
				(context) => RenderContent(context, "Learn about us on the About Page!", 200)
			},
			{ "/notfound", (context) => RenderContent(context, "404 - Not Found", 404) },
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
			await RenderContent(context, "404 - Not Found", 404).ConfigureAwait(false);
		}
	}

	static async Task RenderContent(HttpListenerContext context, string content, int statusCode)
	{
		context.Response.StatusCode = statusCode;
		var buffer = Encoding.UTF8.GetBytes(content);
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