using Minio;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using CRUDDemo.NET6.Minio.Models;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Minio.AspNetCore;

namespace CRUDDemo.NET6.Minio.Services
{
    public class MinioService: IMinioService
    {
        public IWebHostEnvironment _webHostingEnvironment { get; set; }
        public MinioClient _client { get; set; }
        public MinioSetting _minioSetting { get; set; }
        public MinioService(IWebHostEnvironment webHostingEnvironment, MinioClient client, MinioSetting minioSetting)
        {
            _webHostingEnvironment = webHostingEnvironment;
            _client = client;
            _minioSetting = minioSetting;
        }

        public static Dictionary<string, string> contentTypDict = new Dictionary<string, string> {
               {"bmp","image/bmp" },
               {"jpg","image/jpeg"},
               {"jpeg","image/jpeg"},
               {"jpe","image/jpeg"},
               {"png","image/png"},
               {"gif","image/gif"},
               {"ico","image/x-ico"},
               {"tif","image/tiff"},
               {"tiff","image/tiff"},
               {"fax","image/fax"},
               {"wbmp","image//vnd.wap.wbmp"},
               {"rp","image/vnd.rn-realpix"} };

        public async Task<Result<FileModel>> UploadImageAsync(FormFileCollection file)
        {
            Result<FileModel> res = new Result<FileModel>(false, "上传失败");

            //获得文件扩展名
            string fileNameEx = System.IO.Path.GetExtension(file[0].FileName).Replace(".", "");

            //是否是图片，现在只能是图片上传 文件类型 或扩展名不一致则返回
            if (contentTypDict.Values.FirstOrDefault(c => c == file[0].ContentType.ToLower()) == null || contentTypDict.Keys.FirstOrDefault(c => c == fileNameEx) == null)
            {
                res.Message = "图片格式不正确";
                return res;
            }
            else
                return await UploadAsync(file);
        }
        public async Task<Result<FileModel>> UploadAsync(FormFileCollection file)
        {
            Result<FileModel> res = new Result<FileModel>(false, "上传失败");
            try
            {
                //存储桶名
                string bucketName = _minioSetting.BucketName;

                FileModel fileModel = new FileModel();
                await CreateBucket(bucketName);
                var newFileName = CreateNewFileName(bucketName, file[0].FileName);
                PutObjectArgs putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    //.WithFileName(newFileName)
                    .WithStreamData(file[0].OpenReadStream())
                    .WithObject(newFileName)
                    .WithObjectSize(file[0].Length)
                    .WithContentType(file[0].ContentType);
                await _client.PutObjectAsync(putObjectArgs);
                fileModel.Url = $"{_minioSetting.FileURL}{newFileName}";
                if (contentTypDict.Values.Contains(file[0].ContentType.ToLower()))
                {
                    string path = $"{_webHostingEnvironment.ContentRootPath}/wwwroot/imgTemp/";
                    if (!string.IsNullOrEmpty(path) && !Directory.Exists(Path.GetDirectoryName(path)))
                        Directory.CreateDirectory(Path.GetDirectoryName(path));

                    var bImageName = $"{newFileName}";
                    var savepath = $"{path}{newFileName}";//保存绝对路径
                    #region 保存原图到本地
                    using (FileStream fs = System.IO.File.Create(path + newFileName))
                    {
                        file[0].CopyTo(fs);
                        fs.Flush();
                    }
                    #endregion

                    //#region 保存缩略图到本地
                    //var bUrlRes = TencentCloudImageHelper.GetThumbnailImage(240, newFileName, path);
                    //#endregion

                    //上传压缩图

                    using (var sw = new FileStream(savepath, FileMode.Open))
                    {
                        //bucketName, bImageName, sw, sw.Length,"image/jpeg"

                        await _client.PutObjectAsync(new PutObjectArgs()
                            .WithBucket(bucketName)
                            //.WithFileName(bImageName)
                            .WithStreamData(sw)
                            .WithObject(bImageName)
                            .WithObjectSize(sw.Length)
                            .WithContentType("image/jpeg")); 
                        fileModel.Url = $"{_minioSetting.FileURL}{bImageName}";
                    }

                    if (Directory.Exists(Path.GetDirectoryName(path)))
                        Directory.Delete(Path.GetDirectoryName(path), true);
                }
                res.Success = true;
                res.Message = "上传成功";
                res.Data = fileModel;
                return res;
            }
            catch (Exception ex)
            {
                return res;
            }
        }

        public async Task<Result<FileModel>> UploadPdf(Stream file)
        {
            throw new NotImplementedException();
        }

        private async Task CreateBucket(string bucketName)
        {
            BucketExistsArgs bktExistArgs = new BucketExistsArgs().WithBucket(bucketName);
            var found = await _client.BucketExistsAsync(bktExistArgs);
            if (!found)
            {
                MakeBucketArgs makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
                await _client.MakeBucketAsync(makeBucketArgs);
                //设置只读策略
                var pObj = new
                {
                    Version = "2012-10-17",
                    Statement = new[]
                    {
                       new
                       {
                           Effect = "Allow",
                           Principal = new
                           {
                               AWS = new [] {"*"}
                           },
                           Action = new [] {"s3:GetBucketLocation", "s3:ListBucket"},
                           Resource = new []
                           {
                               $"arn:aws:s3:::{bucketName}"
                           }
                       },
                       new
                       {
                           Effect = "Allow",
                           Principal = new
                           {
                               AWS = new [] {"*"}
                           },
                           Action = new [] {"s3:GetObject"},
                           Resource = new []
                           {
                               $"arn:aws:s3:::{bucketName}/*"
                           }
                       }
                   }
                };
                var po = JsonSerializer.Serialize(pObj);
                SetPolicyArgs policyArgs = new SetPolicyArgs().WithBucket(bucketName).WithPolicy(po);
                await _client.SetPolicyAsync(policyArgs);
            }
        }

        private string CreateNewFileName(string bucketName, string oldFileName)
        {
            var dt = Guid.NewGuid().ToString().Replace("-", "").Substring(10) + DateTimeOffset.Now.ToUnixTimeSeconds();
            var extensions = Path.GetExtension(oldFileName);
            var newFileName = $"{bucketName}-{dt}{extensions}";
            return newFileName;
        }
    }
}
