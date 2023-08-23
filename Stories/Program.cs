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
            builder.Services.TryAddScoped<ISleepDurationProvderFactory, SleepDurationProvderFactory>();
            builder.Services.Configure<StoriesApiConfiguration>(builder.Configuration.GetSection(StoriesApiConfiguration.SectionName));
            builder.Services.TryAddScoped<IStoriesApiConfiguration>(sb => sb.GetService<IOptions<StoriesApiConfiguration>>()!.Value);

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