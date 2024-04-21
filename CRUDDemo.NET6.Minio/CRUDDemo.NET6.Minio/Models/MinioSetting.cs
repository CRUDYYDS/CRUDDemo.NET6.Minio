namespace CRUDDemo.NET6.Minio.Models
{
    public class MinioSetting
    {
        public const string SectionName = "MinioSettings";
        public string Endpoint { get; set; }= string.Empty;
        public string Region { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public bool Secure { get; set; }= false;
        public string BucketName {  get; set; } = string.Empty;
        public string FileURL {  get; set; } = string.Empty;
    }
}
