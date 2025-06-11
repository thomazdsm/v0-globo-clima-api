using GloboClima.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Amazon.CognitoIdentityProvider;
using GloboClima.Application.Interfaces;
using Microsoft.OpenApi.Models;
using System.Reflection;
using GloboClima.Domain.Interfaces;
using GloboClima.Infra.Data.Repositories;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;

namespace GloboClima.Infra.IoC
{
    /// <summary>
    /// Configuração de injeção de dependências da aplicação
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            // AWS Services Configuration
            services.AddAWSServices(configuration);

            // DynamoDB Configuration
            services.AddDynamoDBServices(configuration);

            // JWT Authentication Configuration
            services.AddJWTConfiguration(configuration);
            services.AddAuthorizationPolicies();

            // Swagger Docs Configuration
            services.AddSwaggerConfiguration(configuration);

            services.AddClients();

            // Register Application Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IWeatherService, WeatherService>();
            services.AddScoped<IFavoriteCityService, FavoriteCityService>();

            // Register Repositories
            services.AddScoped<IFavoriteCityRepository, FavoriteCityRepository>();

            // Add Logging
            services.AddLogging();

            return services;
        }

        public static IServiceCollection AddAWSServices(this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Configuração das credenciais AWS
            var awsOptions = configuration.GetAWSOptions();
            services.AddDefaultAWSOptions(awsOptions);

            // Adiciona o serviço do Cognito
            services.AddAWSService<IAmazonCognitoIdentityProvider>();

            return services;
        }
        
        public static IServiceCollection AddDynamoDBServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configuração das credenciais AWS (reutiliza a configuração já existente)
            var awsOptions = configuration.GetAWSOptions();

            // Registra o cliente do DynamoDB
            services.AddAWSService<IAmazonDynamoDB>(awsOptions);

            // Registra o contexto do DynamoDB
            services.AddScoped<IDynamoDBContext>(provider =>
            {
                var dynamoClient = provider.GetRequiredService<IAmazonDynamoDB>();
                var dynamoConfig = new DynamoDBContextConfig
                {
                    // Configurações opcionais
                    ConsistentRead = false, // Para melhor performance, use eventual consistency
                    SkipVersionCheck = true // Pula verificação de versão para melhor performance
                };

                return new DynamoDBContext(dynamoClient, dynamoConfig);
            });

            return services;
        }

        public static IServiceCollection AddJWTConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtConfig = configuration.GetSection("Jwt");
            var awsConfig = configuration.GetSection("AWS:Cognito");

            services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    // Configurações básicas do AWS Cognito
                    options.Authority = jwtConfig["Issuer"]; // https://cognito-idp.{region}.amazonaws.com/{userPoolId}
                    options.Audience = jwtConfig["Audience"]; // Seu Client ID do Cognito

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtConfig["Issuer"],

                        ValidateAudience = false, // Cognito usa 'aud' claim diferente, então desabilitamos
                                                  // ValidAudience = jwtConfig["Audience"], // Comentado pois causa problemas com Cognito

                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        // Configurações específicas para AWS Cognito
                        NameClaimType = "email", // ou "email" dependendo do que você quer usar
                        RoleClaimType = "cognito:groups",

                        // Claims que devem ser mapeados
                        ClockSkew = TimeSpan.Zero, // Remove tolerância de tempo

                        // Para debug - remova em produção
                        RequireExpirationTime = true,
                        RequireSignedTokens = true
                    };

                    // Remove o resolver customizado - deixa o .NET buscar automaticamente
                    // IssuerSigningKeyResolver = null

                    options.RequireHttpsMetadata = true; // Altere para false apenas em desenvolvimento local
                    options.SaveToken = true;

                    // Para debug - adiciona logs detalhados
                    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Console.WriteLine($"Authentication failed: {context.Exception}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            Console.WriteLine("Token validated successfully");
                            // Debug: mostra todos os claims
                            var claims = context.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                            Console.WriteLine($"Claims: {string.Join(", ", claims)}");
                            return Task.CompletedTask;
                        },
                        OnChallenge = context =>
                        {
                            Console.WriteLine($"OnChallenge: {context.Error}, {context.ErrorDescription}");
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }

        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAuthenticatedUser", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });

                // Adicione outras políticas conforme necessário
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireClaim("cognito:groups", "admin");
                });
            });

            return services;
        }

        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "GloboClima API",
                    Version = "v1",
                    Description = "API para autenticação e gerenciamento climático",
                    Contact = new OpenApiContact
                    {
                        Name = "Thomaz Machado",
                        Email = "thomazdsmachado@.com"
                    }
                });

                // JWT Bearer Authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header usando Bearer scheme. \n\n" +
                                  "Digite 'Bearer' [espaço] e então seu token.\n\n" +
                                  "Exemplo: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });

                // XML Comments para documentação detalhada
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Tags customizadas
                c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
            });

            return services;
        }

        public static IServiceCollection AddClients(this IServiceCollection services)
        {
            services.AddHttpClient<ICountryService, CountryService>();
            services.AddHttpClient<IWeatherService, WeatherService>();

            return services;
        }
    }
}