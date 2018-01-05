using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace echo
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var contents = new ConcurrentDictionary<string, string>();

            app.Use(async (ctx, next) =>
                    {
                        switch (ctx.Request.Method?.ToLowerInvariant())
                        {
                            case "get":
                                ctx.Response.StatusCode = 200;
                                ctx.Response.ContentType = "text/plain";

                                using (var writer = new StreamWriter(ctx.Response.Body))
                                {
                                    await writer.WriteAsync(contents.GetValueOrDefault(ctx.Request.GetEncodedPathAndQuery()))
                                                .ConfigureAwait(false);
                                }

                                break;

                            case "post":
                                ctx.Response.StatusCode = 201;

                                using (var reader = new StreamReader(ctx.Request.Body))
                                {
                                    contents[ctx.Request.GetEncodedPathAndQuery()] = await reader.ReadToEndAsync()
                                                                                                 .ConfigureAwait(false);
                                }

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
