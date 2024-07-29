using System.ComponentModel.DataAnnotations;

namespace OrderService.DTO
{
    public class SendOrderNotificationDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string UserEmail { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public string Product { get; set; }
    }
}