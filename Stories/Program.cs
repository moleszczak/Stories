using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Stories.Configuration;
using Stories.Services;

namespace Stories
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.TryAddScoped<IStoryApiClient, StoryApiClient>();
            builder.Services.TryAddScoped<IStoryDetailsApiClient, StoryDetailsApiClient>();
            builder.Services.Configure<StoriesApiConfiguration>(builder.Configuration.GetSection(StoriesApiConfiguration.SectionName));
            builder.Services.TryAddScoped<IStoriesApiConfiguration>(sb => sb.GetService<IOptions<StoriesApiConfiguration>>()!.Value);
            builder.Services.TryAddSingleton<WaitDurationProvider>((attempt) => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
            builder.Services.TryAddSingleton<CacheExpirationProvider>(() => TimeSpan.FromSeconds(120));
            builder.Services.AddMemoryCache();
            builder.Services.AddAutoMapper(typeof(MappingConfiguration));

            builder.Services.TryAddScoped(typeof(IApiClient<IEnumerable<int>>), typeof(ApiClient<IEnumerable<int>>));
            builder.Services.TryDecorate(typeof(IApiClient<IEnumerable<int>>), typeof(ApiClientWithCache<IEnumerable<int>>));

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}