using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DubiSouqWebsite.Models
{
    public class OrderModel
    {
        public virtual order Order { get; set; }

        public virtual List<order_item> Order_Item { get; set; }
    }
}