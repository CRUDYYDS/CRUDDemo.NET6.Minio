namespace CRUDDemo.NET6.Minio
{
    using CRUDDemo.NET6.Minio.Models;
    using CRUDDemo.NET6.Minio.Services;
    using global::Minio.AspNetCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Minio;
    using Minio.Controllers;
    using System.Configuration;
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSingleton<IMinioService, MinioService>();
            builder.Services.AddSwaggerGen();

            var minioSetting = new MinioSetting();
            builder.Configuration.Bind("MinioSettings", minioSetting);
            
            builder.Services.AddMinio(options => {
                options.Endpoint = minioSetting.Endpoint;
                options.Region = minioSetting.Region;
                options.AccessKey = minioSetting.AccessKey;
                options.SecretKey = minioSetting.SecretKey;
            });
            builder.Services.AddSingleton<MinioSetting>(minioSetting);
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
