using ECommerceLiteEntity.IdentityModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.Models
{
    [Table("Orders")]
    public class Order : Base<int>
    {
        
        [Required]
        [Display(Name ="Sipariş Numarası")]
        [StringLength(maximumLength:11, MinimumLength =11,ErrorMessage ="Sipariş numarası 11 haneli olmalıdır.")]

        public string OrderNumber { get; set; }

        // kim bu siparişi verdi
        public string CustomerTCNumber { get; set; }
        [ForeignKey("CustomerTCNumber")]
        public virtual  Customer Customer { get; set; }

        // bu siparişin içinde ne var ? ne aldı?

        public virtual List<OrderDetails>  OrderDetails { get; set; }
    }
}
