using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.Models
{
  public  interface IPersonBase
    {
        DateTime LastActiveTime { get; set; }

        bool IsDeleted { get; set; }
    }
}
