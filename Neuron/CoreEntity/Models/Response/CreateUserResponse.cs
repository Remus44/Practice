using Interview.Models;

namespace Interview.Models
{
    public class CreateUserResponse
    {
        public CreateUserResponse(UserDTO userDTO)
        {
            UserDTO= userDTO;
        }

        UserDTO UserDTO{get;set;}
    }
}
