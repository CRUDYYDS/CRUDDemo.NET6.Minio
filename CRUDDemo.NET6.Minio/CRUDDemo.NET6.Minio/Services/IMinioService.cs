using CRUDDemo.NET6.Minio.Models;
using System.IO;

namespace CRUDDemo.NET6.Minio.Services
{
    public interface IMinioService
    {
        Task<Result<FileModel>> UploadAsync(FormFileCollection file);
        Task<Result<FileModel>> UploadImageAsync(FormFileCollection file);
        Task<Result<FileModel>> UploadPdf(Stream file);
    }
}
