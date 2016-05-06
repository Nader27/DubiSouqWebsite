using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DubiSouqWebsite.Models
{
    public static partial class ShoppingCart
    {
        public static void AddToCart(int id,int quantity)
        {
            Entities db = new Entities();
            user USER = HttpContext.Current.Session["user"] as user;
            List<cart_item> ct = GetCartItems();
            foreach (cart_item item in ct)
            {
                var cartItem = db.cart_item.SingleOrDefault(c => c.Product_ID == id);
                if (cartItem != null)
                {
                    cartItem.Quantity+=quantity;
                    db.SaveChanges();
                    return;
                }
            }
            cart_item cart = new cart_item();
            cart.Product_ID = id;
            cart.Quantity = quantity;
            cart.user_ID = USER.ID;
            db.cart_item.Add(cart);
            db.SaveChanges();
            return;
        }

        public static void RemoveFromCart(int id , int quantity)
        {
            Entities db = new Entities();
            user USER = HttpContext.Current.Session["user"] as user;
            List<cart_item> ct = GetCartItems();
            foreach (cart_item item in ct)
            {
                var cartItem = db.cart_item.SingleOrDefault(c => c.Product_ID == id);
                if (cartItem != null)
                {
                    cartItem.Quantity -= quantity;
                    if(cartItem.Quantity == 0)
                        db.cart_item.Remove(cartItem);
                    db.SaveChanges();
                }
            }
        }

        public static void EmptyCart()
        {
            Entities db = new Entities();
            user USER = HttpContext.Current.Session["user"] as user;
            List<cart_item> ct = GetCartItems();
            foreach (cart_item cartItem in ct)
            {
                db.cart_item.Remove(cartItem);
                db.SaveChanges();
            }
        }

        public static List<cart_item> GetCartItems()
        {
            Entities db = new Entities();
            user USER = HttpContext.Current.Session["user"] as user;
            if (USER != null)
            {
                return db.cart_item.Where(u => u.user_ID == USER.ID).ToList();
            }
            return new List<cart_item>(); ;
        }

        public static int GetCount()
        {
            int quan = 0;
            foreach (cart_item item in GetCartItems())
            {
                quan += item.Quantity;
            }
            return quan;
        }

        public static decimal GetTotal()
        {
            Entities db = new Entities();
            List<cart_item> ct = GetCartItems();
            double total = 0;
            foreach (cart_item cartItem in ct)
            {
                total += ((cartItem.product.Price / 100) * (100 - cartItem.product.Sale) * cartItem.Quantity) ?? 0;
            }

            return Convert.ToDecimal(total);
        }

        public static int CreateOrder(int id)
        {
            Entities db = new Entities();
            user USER = HttpContext.Current.Session["user"] as user;
            var cartItems = GetCartItems();
            order ORDER = new order();
            ORDER.User_ID = USER.ID;
            ORDER.Status = 1;
            ORDER.Payment_Method = id;
            ORDER.Time = DateTime.Now;
            ORDER.Total = Convert.ToDouble(GetTotal());
            if (ORDER.Total < 1000)
                ORDER.Total += 50;
            db.orders.Add(ORDER);
            db.SaveChanges();
            foreach (cart_item item in cartItems)
            {
                order_item ORDERITEM = new order_item();
                ORDERITEM.Product_ID = item.Product_ID;
                ORDERITEM.Quantity = item.Quantity;
                ORDERITEM.Order_ID = db.orders.Max(u => u.ID);
                db.order_item.Add(ORDERITEM);
                db.SaveChanges();
                product prod = db.products.Single(u => u.ID == item.Product_ID);
                prod.Quantity -= item.Quantity;
                db.Entry(prod).CurrentValues.SetValues(prod);
                db.SaveChanges();
            }
            EmptyCart();
            return db.orders.Max(u => u.ID);
        }

        public static List<whish_list> Getwishlist()
        {
            Entities db = new Entities();
            user USER = HttpContext.Current.Session["user"] as user;
            if (USER != null)
            {
                return db.whish_list.Where(u => u.User_ID == USER.ID).ToList();
            }
            return new List<whish_list>();
        }

        public static void AddToWish(int id)
        {
            Entities db = new Entities();
            user USER = HttpContext.Current.Session["user"] as user;
            List<whish_list> wl = Getwishlist();
            foreach (whish_list item in wl)
            {
                var WhishItem = db.whish_list.SingleOrDefault(c => c.Product_ID == id);
                if (WhishItem != null)
                    return;
            }
            whish_list wish = new whish_list();
            wish.Product_ID = id;
            wish.User_ID = USER.ID;
            db.whish_list.Add(wish);
            db.SaveChanges();
        }

        public static void RemoveFromWishlist(int id)
        {
            Entities db = new Entities();
            user USER = HttpContext.Current.Session["user"] as user;
            List<whish_list> wl = Getwishlist();
            foreach (whish_list item in wl)
            {
                var WhishItem = db.whish_list.SingleOrDefault(c => c.Product_ID == id);
                if (WhishItem != null)
                {
                    db.whish_list.Remove(WhishItem);
                    db.SaveChanges();
                }
            }
        }

        public static int GetwhishCount()
        {
            return Getwishlist().Count;
        }
    }

    public class ShoppingCartViewModel
    {
        public decimal CartTotal { get; set; }
        public int CartCount { get; set; }
        public int Id { get; set; }
        public int itemcount { get; set; }
        public int wishcount { get; set; }
    }
}