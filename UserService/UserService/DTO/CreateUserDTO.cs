using System.ComponentModel.DataAnnotations;

namespace UserService.DTO
{
    public class CreateUserDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public DateOnly DateOfBirth { get; set; }
    }
}
