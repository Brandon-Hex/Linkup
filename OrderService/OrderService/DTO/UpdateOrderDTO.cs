using System.ComponentModel.DataAnnotations;

namespace OrderService.DTO
{
    public class UpdateOrderDTO
    {
        [Required]
        public string Product { get; set; }
    }
}
