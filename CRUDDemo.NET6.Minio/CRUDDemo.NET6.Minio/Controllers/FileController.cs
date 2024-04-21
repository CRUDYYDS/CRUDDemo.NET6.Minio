using CRUDDemo.NET6.Minio.Models;
using CRUDDemo.NET6.Minio.Services;
using Microsoft.AspNetCore.Mvc;

namespace CRUDDemo.NET6.Minio.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FileController: ControllerBase
    {
        public IMinioService _minioService { get; set; }
        public FileController(IMinioService minioService)
        {
            this._minioService = minioService;
        }

        [Route("UploadImg")]
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<Result<FileModel>> UploadImg(FormFileCollection file)
        {
            return await _minioService.UploadImageAsync(file);
        }
    }
}
