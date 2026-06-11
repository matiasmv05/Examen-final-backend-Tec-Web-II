using Amazon.Core.CustomEntities;

namespace Amazon.api.Responses
{
    public class ApiResponse<T>
    {
        public Message[] Messages { get; set; }
        public T Data { get; set; }
        public Pagination Pagination { get; set; }
        public ApiResponse(T data)
        {
            Data = data;
        }
    }
}
