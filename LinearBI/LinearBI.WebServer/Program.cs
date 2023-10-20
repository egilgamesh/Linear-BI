using System.Net;
using System.Text;

namespace LinearBI.WebServer;

public class Program
{
	public static async Task Main(string[] args)
	{
		var listener = new HttpListener();
		listener.Prefixes.Add("http://localhost:8080/"); // Set your desired URL and port
		listener.Start();
		Console.WriteLine("Server is running...");
		while (true)
		{
			var context = await listener.GetContextAsync().ConfigureAwait(false);
			await ProcessRequestAsync(context).ConfigureAwait(false);
		}
	}

	//static async Task ProcessRequestAsync(HttpListenerContext context)
	//{
	//	var dynamicMessage = "Hello, Dynamic World!";
	//	var responseString = $"<html><body><h1>{dynamicMessage}</h1></body></html>";
	//	var buffer = Encoding.UTF8.GetBytes(responseString);
	//	context.Response.ContentLength64 = buffer.Length;
	//	var output = context.Response.OutputStream;
	//	await output.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
	//	output.Close();
	//}

	static async Task ProcessRequestAsync(HttpListenerContext context)
	{
		var requestUrl = context.Request.RawUrl;
		switch (requestUrl)
		{
		case "/home":
			await RenderHomePage(context).ConfigureAwait(false);
			break;
		case "/about":
			await RenderAboutPage(context).ConfigureAwait(false);
			break;
		default:
			await RenderNotFoundPage(context).ConfigureAwait(false);
			break;
		}
	}

	static async Task RenderHomePage(HttpListenerContext context)
	{
		var responseString = "<b>Welcome to the Home Page!</b>";
		await SendResponse(context, responseString).ConfigureAwait(false);
	}

	static async Task RenderAboutPage(HttpListenerContext context)
	{
		var responseString = "Learn about us on the About Page!";
		await SendResponse(context, responseString).ConfigureAwait(false);
	}

	static async Task RenderNotFoundPage(HttpListenerContext context)
	{
		context.Response.StatusCode = 404;
		var responseString = "404 - Not Found";
		await SendResponse(context, responseString).ConfigureAwait(false);
	}

	static async Task SendResponse(HttpListenerContext context, string responseString)
	{
		var buffer = Encoding.UTF8.GetBytes(responseString);
		context.Response.ContentLength64 = buffer.Length;
		var output = context.Response.OutputStream;
		await output.WriteAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
		output.Close();
	}
}