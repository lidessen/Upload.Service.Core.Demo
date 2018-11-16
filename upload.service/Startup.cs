using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace upload.service
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.Configure<Settings>(Configuration.GetSection("Settings"));
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder =>
                builder.AllowAnyHeader()
                       .AllowAnyOrigin()
                       .AllowAnyMethod()
            );
            var path = Configuration.GetSection("Settings").GetValue<string>("UploadPath");
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Configuration.GetSection("Settings").GetValue<string>("UploadPath")),
                RequestPath = new PathString(Configuration.GetSection("Settings").GetValue<string>("RequestPath"))
            });

            app.UseMvc();
        }
    }

    public class Settings
    {
        public string UploadPath { get; set; }
        public string RequestPath { get; set; }
    }
}
