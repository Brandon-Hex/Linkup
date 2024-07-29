using System.ComponentModel.DataAnnotations;

namespace OrderService.DTO
{
    public class CreateOrderDTO
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public string Product { get; set; }
    }
}
