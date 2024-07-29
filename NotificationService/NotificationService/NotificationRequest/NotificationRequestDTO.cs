using System.ComponentModel.DataAnnotations;

namespace NotificationService.NotificationRequest
{
    public class NotificationRequestDTO
    {
        [Required]
        public string UserId { get; set; }
    }
}
