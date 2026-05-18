using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HotelLux.Stay.API.Extensions;

public static class VersionedSwaggerExtensions
{
    public static IServiceCollection AddVersionedSwagger(
        this IServiceCollection services,
        string apiTitle,
        string apiDescription)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(
            sp => new ConfigureSwaggerOptions(
                sp.GetRequiredService<IApiVersionDescriptionProvider>(),
                apiTitle,
                apiDescription));

        services.AddSwaggerGen();
        return services;
    }

    private sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        private readonly string _title;
        private readonly string _description;

        public ConfigureSwaggerOptions(
            IApiVersionDescriptionProvider provider,
            string title,
            string description)
        {
            _provider = provider;
            _title = title;
            _description = description;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = _title,
                    Version = description.ApiVersion.ToString(),
                    Description = _description
                });
            }

            options.DocInclusionPredicate((documentName, apiDescription) =>
                string.Equals(documentName, apiDescription.GroupName, StringComparison.OrdinalIgnoreCase));

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "JWT del ecosistema HotelLux (login en Auth)."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }
    }
}
