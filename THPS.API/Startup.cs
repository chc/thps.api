using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QScript;
using THPS.API.Authentication;
using THPS.API.DbContext;
using THPS.API.Repository;
using THPS.API.Utils;

using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using QScript.JsonConverters;

namespace THPS.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "THPS API", Version = "v1" });

                c.AddSecurityDefinition("ApiKeyAuth", new OpenApiSecurityScheme
                {
                    Name = "APIKey",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "ApiKeyAuth",
                    In = ParameterLocation.Header,
                    Description = "APIKey header."
                });


                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "ApiKeyAuth"
                                },
                                Scheme = "ApiKeyAuth",
                                Name = "APIKey",
                                In = ParameterLocation.Header,

                            },
                            new List<string>()
                        }
                    });
            });

            services.AddMvc().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                options.SerializerSettings.Converters.Add(new QTokenConverter());
                options.SerializerSettings.Converters.Add(new SymbolEntryConverter());
            }
            ).SetCompatibilityVersion(CompatibilityVersion.Latest);


            services.AddScoped<ITHPSAPIDBContext, THPSAPIDBContext>(c => new THPSAPIDBContext(Configuration.GetConnectionString("QDatabase")));
            services.AddScoped<IScriptKeyRepository, ScriptKeyRepository>();
            services.AddScoped<IChecksumResolver, ChecksumResolver>();
            services.AddSingleton<APIKeyProvider>(c => new APIKeyProvider(Configuration.GetValue<string>("APIKeyPrivateKey")));

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin"));
                }));
                options.AddPolicy("SaveAccess", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "SaveAccess"));
                }));

                options.AddPolicy("APIKeyManage", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin"));
                }));

                options.AddPolicy("SceneAccess", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "SceneAccess"));
                }));
            });

            services.AddAuthentication("ApiKeyAuth").AddScheme<ApiKeyAuthOpts, ApiKeyAuthHandler>("ApiKeyAuth", "ApiKeyAuth", opts => { });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var builder = new ConfigurationBuilder()
                        .SetBasePath(env.ContentRootPath)
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables();
            this.Configuration = builder.Build();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });


            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
