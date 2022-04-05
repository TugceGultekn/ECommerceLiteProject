using ECommerceLiteEntity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteBLL.Repostory
{
   public  class Repositories
    {

    }


    public class CategoryRepo : RepositoryBase <Category , int> { }
    public class ProductRepo : RepositoryBase<Product, int> { }
    public class OrderRepo : RepositoryBase<Order, int> { }
    public class OrderDetailRepo : RepositoryBase<OrderDetails, int> { }
    public class CustomerRepo : RepositoryBase<Customer, int> { }
    public class AdminRepo : RepositoryBase<Admin, int> { }
    public class PassiveUserRepo : RepositoryBase<PassiveUser, int> { }
    public class ProductPictureRepo : RepositoryBase<ProductPictureRepo, int> { }
}
