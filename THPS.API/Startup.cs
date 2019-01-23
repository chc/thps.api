using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QScript;
using Swashbuckle.AspNetCore.Swagger;
using THPS.API.Authentication;
using THPS.API.DbContext;
using THPS.API.Repository;
using THPS.API.Utils;

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
            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling =
                                           Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "THPS API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme { In = "header", Description = "Please enter your API Key", Name = "APIKey", Type = "apiKey" });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>> { { "Bearer", Enumerable.Empty<string>() } });
                c.CustomSchemaIds(x => x.FullName);

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                //c.IncludeXmlComments(xmlPath);
            });
            services.AddScoped<ITHPSAPIDBContext, THPSAPIDBContext>(c => new THPSAPIDBContext(Configuration.GetConnectionString("QDatabase")));
            services.AddScoped<IScriptKeyRepository, ScriptKeyRepository>();
            services.AddScoped<IChecksumResolver, ChecksumResolver>();
            services.AddSingleton<APIKeyProvider>(c => new APIKeyProvider("MIIEowIBAAKCAQEAmI89Ywgoq9u5+64/+H+mbvJXzkQIuZt8mvgPjX+ULht1bfePk3HXDX/iQgInS1E4t+MLtSPIzCm+OLQIw5ug9r5ovxJacnyWhyeFldne2/uUCfQhfw9ILSffxdwc2AiFwKLUIdrxWLRp0ogKv32uorg2fIeuzC0GV+i/f9LrFRMlL3W9SrQNlJrbgLwTzmDRnaLrECMkhddiW7vAGVMyzVw3pwoP4ybxSeJwr+zK13HiGiar2fMP8mKp+xS+tI9SLi6EfiR3nUZzFCORDyJaIR0Vm3DOC5G+eroar5ejdPwhqkXo3mwJ18pv5mEh2nyU2bBU3HARaXCZLp9uZmwgeQIBJQKCAQEAjDCZRj7ZQ/pX4FrurP+tsg8gQQA/XnM0O2B8/pDpB8Y0jpB2GMl5ggbP6aEdMHQmSBy+pnPo8vXtEYLXnv22gd9nLB6z+oBS+LyIl4nvYlzNOZQ6dMHvTBbNoQ90c31z/SABM93yiN0i+NAJ4GWnZRf6t59GrciCXp6GdXW1bU5xee93g6eJludyySbjFBbqCxZC5k2oYHZxGukPGT3PuFDoDroHHMbqKiyN13kOeJz3hJ/3Dt/XnIErSX+tEz5aodG4HxubNT0qyMgutuU+OIuAoAmnUKDnvafMXLw3kYaQSqmxY7fVnYEvvrPV0aP3dVokGnz/Wzh+mj0a60yArQKBgQDN4Ig6Z3ecvb+a8UPyfghapg2bmzIoKHENuTW8Ry61AtuZp5tbDZom9V0taer3h/qSpVas7eOqTAVQc1pGsGM6NzCNY/UtWo2HO35at3SG/pUk02lXOqqFuqzlAuVDh3yP618TqWj2jW59rodDg7xoJd/xpPrdqI61nSWe0ARkUQKBgQC9s6UmeZy1dmbpSygqMvQ8CaK7XDMQbOVwI1s3J512GILswjraRUrpnxiKzHaO20T2zeWuTNCCixwT1dy0I+/EZQMnNJE/ebofK1LlY3gee5XGsHVt6hoAU4tPwD/nOItABD050AZQeU9cYMrfELrqtBt4KbDwsG+0Jdz2DtC3qQKBgQChXPUm23ltA2yp37HL5j3mLx9sH7GwxcBks8JVTIxkXC+UG5Vw4SXLggrCuszrhkDvequ3+Llb9mUHtFuDg1SxFoAAHemt7QckzcPmPMMdssfsbll7uFwjoCalqFLUED8JBJagtTaXuvW8dAknFDm5acszBMSf5Po7UafdVu6vfQKBgQCkEP2JDztsghPQr7QI0h9WhN/EosRhO5YAHpQUBpYFRaGqK3ErejLzkIPtqen/AtPcXwvVBCn0XKKpXwQRazA7Ju34ZNClmbW6F6Gjy6YoM4h0fO/wW8N152O6mG6edheRT8Y/1oIAaOqwwmpEYX8QLRDWoJkHg9Y68FBmGqasrQKBgFi0IqcebioOKV1HCloIM2bY3zWO8/JcqLsLJdVEvDcoYdJUZX3dblig3KWNfy0T5dvpjjdYqbXpbl0E6piUMDJ81gvUPySMyUp5uBaAPqk2vftE9NQ3AZK3lXIVnnSZRcou51rZ3SZRSdBlqccWRA9GSkjzLJVEWcoyg/B6loTj"));

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SaveAccess", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin") || (c.Type == "role" && c.Value == "SaveAccess"));
                }));

                options.AddPolicy("APIKeyManage", policy => policy.RequireAssertion(context =>
                {
                    return context.User.HasClaim(c => (c.Type == "role" && c.Value == "Admin"));
                }));
            });

            services.AddAuthentication("ApiKeyAuth").AddScheme<ApiKeyAuthOpts, ApiKeyAuthHandler>("ApiKeyAuth", "ApiKeyAuth", opts => { });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "OpenSpy API V1");
            });

            var builder = new ConfigurationBuilder()
                        .SetBasePath(env.ContentRootPath)
                        .AddJsonFile("appsettings.json")
                        .AddEnvironmentVariables();
            this.Configuration = builder.Build();


            app.UseMvc();
        }
    }
}
