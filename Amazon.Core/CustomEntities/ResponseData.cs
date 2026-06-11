using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Amazon.Core.CustomEntities
{
    public class ResponseData
    {
        public PagedList<object> Pagination { get; set; }
        public Message[] Messages { get; set; }
        [JsonIgnore]
        public HttpStatusCode StatusCode { get; set; }
    }
}
