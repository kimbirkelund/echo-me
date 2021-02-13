using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace echo
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
            => Configuration = configuration;

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            var contents = new ConcurrentDictionary<string, Item>();

            app.Use(async (ctx, _) =>
                    {
                        var key = ctx.Request.GetEncodedPathAndQuery();

                        switch (ctx.Request.Method.ToLowerInvariant())
                        {
                            case "get":
                                if (contents.TryGetValue(key, out var item))
                                {
                                    var (data, contentType) = item;

                                    ctx.Response.StatusCode = 200;
                                    ctx.Response.ContentType = contentType;

                                    await ctx.Response.BodyWriter.WriteAsync(data);
                                }
                                else
                                    ctx.Response.StatusCode = 404;

                                break;

                            case "post":
                                ctx.Response.StatusCode = 201;
                                ctx.Response.ContentType = "text/plain";

                                var stream = new MemoryStream();
                                await ctx.Request.BodyReader.CopyToAsync(stream);
                                contents[key] = new Item(new ReadOnlyMemory<byte>(stream.ToArray()), ctx.Request.ContentType);

                                break;

                            default:
                                ctx.Response.StatusCode = 400;
                                break;
                        }
                    });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) { }
    }
}
