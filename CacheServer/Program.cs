using CacheServer.Context;
using CacheServer.Models;
using CacheServer.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using System.Text;
using System.Text.Json;

internal class Program
{
    public static async Task Main()
    {
        IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
        var jsonOpts = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        using var db = new AppDbContext();

        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5000/");
        listener.Start();

        Console.WriteLine("Listener started...");

        while (true)
        {
            var ctx = await listener.GetContextAsync();
            _ = Task.Run(() => HandleRequest(ctx));
        }

        async Task HandleRequest(HttpListenerContext ctx)
        {
            try
            {
                var request = ctx.Request;
                var parts = request.Url!.AbsolutePath.Trim('/').Split('/');

                if (parts[0] != "products")
                {
                    SendResponse(ctx, 404, "Not found");
                    return;
                }

                int? id = parts.Length > 1 && int.TryParse(parts[1], out var i) ? i : null;

                using var context = new AppDbContext();
                IRepository repo = new CachedRepository(new Repository(context), cache);

                switch (request.HttpMethod)
                {
                    case "GET":
                        if (id == null)
                        {
                            SendResponse(ctx, 200, await repo.GetAllAsync());
                            return;
                        }
                        var product = await repo.GetByIdAsync(id.Value);
                        if (product == null) SendResponse(ctx, 404, "Product not found");
                        else SendResponse(ctx, 200, product);
                        break;

                    case "POST":
                        var newP = ReadProduct(request);
                        if (await repo.AddAsync(newP)) SendResponse(ctx, 201, "Product added");
                        else SendResponse(ctx, 400, "Product could not be added");
                        break;

                    case "PUT":
                        var updP = ReadProduct(request);
                        if (await repo.UpdateAsync(updP)) SendResponse(ctx, 200, "Product updated");
                        else SendResponse(ctx, 404, "Product not found");
                        break;

                    case "DELETE":
                        if (id == null) SendResponse(ctx, 400, "Id needed");
                        else if (await repo.DeleteAsync(id.Value)) SendResponse(ctx, 200, "Product deleted");
                        else SendResponse(ctx, 404, "Product not found");
                        break;

                    default:
                        SendResponse(ctx, 405, "Method not allowed");
                        break;
                }
            }
            catch (Exception ex)
            {
                SendResponse(ctx, 500, ex.Message);
            }
        }

        Product ReadProduct(HttpListenerRequest request)
        {
            using var sr = new StreamReader(request.InputStream, request.ContentEncoding);
            return JsonSerializer.Deserialize<Product>(sr.ReadToEnd(), jsonOpts)!;
        }

        void SendResponse(HttpListenerContext ctx, int status, object? data)
        {
            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data, jsonOpts));
            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/json";
            ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
            ctx.Response.Close();
        }
    }
}