using Newtonsoft.Json;

namespace Interview.Models
{
    public abstract class BaseDTO
    {
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public DateTime CreationDate { get; set; }
        [JsonIgnore]
        public DateTime LastModifyDate { get; set; }
    }
}