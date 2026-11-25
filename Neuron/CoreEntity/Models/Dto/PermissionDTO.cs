namespace Interview.Models
{
    public class PermissionDTO:BaseDTO
    {

        public PermissionDTO(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public string Name { get; set; }
        public string Code { get; set; }
    }
}
