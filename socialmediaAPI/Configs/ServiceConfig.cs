using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using socialmediaAPI.Repositories.Interface;
using socialmediaAPI.Repositories.Repos;
using socialmediaAPI.Services.Authentication;
using socialmediaAPI.Services.CloudinaryService;
using socialmediaAPI.Services.SMTP;
using socialmediaAPI.Services.Validators;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace socialmediaAPI.Configs
{
    public static class ServiceConfig
    {
        public static IServiceCollection AddServicesConfig(this IServiceCollection services, IConfiguration config)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddSignalR();
            services.ConfigDbContext(config);
            services.ConfigRepositories(config);
            services.ConfigAuthentication(config);
            services.ConfigDI(config);

            return services;
        }
        public static IServiceCollection ConfigDbContext(this IServiceCollection services, IConfiguration config)
        {
            DatabaseConfigs databaseConfigs = new DatabaseConfigs();
            config.GetSection("DatabaseConfigs").Bind(databaseConfigs);

            databaseConfigs.SetupDatabase();
            services.AddSingleton(databaseConfigs);
            BsonSerializer.RegisterSerializer(new ObjectSerializer());

            return services;
        }
        public static IServiceCollection ConfigRepositories(this IServiceCollection services, IConfiguration config)
        {
            services.AddTransient<IConversationRepository, ConversationRepository>();
            services.AddTransient<IMessageRepository, MessageRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IPostRepository, PostRepository>();
            services.AddTransient<ICommentRepository, CommentRepository>();
            return services;
        }
        public static IServiceCollection ConfigAuthentication(this IServiceCollection services, IConfiguration config)
        {
            TokenConfigs tokenConfiguration = new TokenConfigs();
            config.Bind("TokenConfiguration", tokenConfiguration);
            services.AddSingleton(tokenConfiguration);
            services.AddSingleton<TokenGenerator>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                        tokenConfiguration.AccessTokenSecret)),
                    ValidIssuer = tokenConfiguration.Issuer,
                    ValidAudience = tokenConfiguration.Audience,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(6)
                };
            });
            services.AddAuthorization();
            return services;
        }
        public static IServiceCollection ConfigDI(this IServiceCollection services, IConfiguration config)
        {
            //Cloudinary
            CloudinaryConfigs cloudinarySettings = new CloudinaryConfigs();
            config.Bind("CloudinarySettings", cloudinarySettings);
            services.AddSingleton(cloudinarySettings);
            services.AddSingleton<CloudinaryHandler>();
            //SMTP
            SMTPConfigs smtpConfigs = new SMTPConfigs();
            config.Bind("SMTPConfiguration", smtpConfigs);
            services.AddSingleton(smtpConfigs);
            services.AddSingleton<EmailUtil>();
            //Automapper
            services.AddAutoMapper(typeof(AutomapperConfigs));
            //Validator
            services.AddTransient<UserValidator>();
            //JSON serialization
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.IncludeFields = true;
            });
            //CORS allow all hosts to call to
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("http://localhost:3000")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                });
            });
            return services;
        }
    }
}
