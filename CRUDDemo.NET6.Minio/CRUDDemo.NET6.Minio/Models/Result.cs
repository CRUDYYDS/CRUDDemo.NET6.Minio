namespace CRUDDemo.NET6.Minio.Models
{
    public class Result<T>
    {
        public Result(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }
        public bool Success { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }
}
