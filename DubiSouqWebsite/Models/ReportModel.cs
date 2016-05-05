using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DubiSouqWebsite.Models
{
    public static class ReportModel
    {

        //command 11 (Add user)         id : UserID ,     text : UserEmail
        //command 12 (Edit user)        id : UserID ,     text : UserEmail
        //command 13 (Delete user)      id : UserID ,     text : UserEmail
        //command 21 (Add product)      id : ProductID ,  text : ProductName
        //command 22 (Edit product)     id : ProductID ,  text : ProductName
        //command 23 (Delete product)   id : ProductID ,  text : ProductName
        //command 3  (Make Sale)       id : ProductID ,  text : SaleValue
        //command 4  (Edit OrderStatus) id : OrderID ,    text : OrderStatus
        public static void CreateAdminReport(int userid, int command, int id, string text = "")
        {
            Entities db = new Entities();
            user user = db.users.Find(userid);
            report report = new report();
            report.Product_ID = null;
            string Message =  user.Name.ToString() + " Role : " + user.user_type.Name +" Has ";
            switch (command)
            {
                case 11:
                    Message += "Added New User To The System  User ID = " + id.ToString() + " Email : " + text;
                    break;
                case 12:
                    Message += "Edited User Information in The System  User ID = " + id.ToString() + " Email : " + text;
                    break;
                case 13:
                    Message += "Deleted User from The System  Product ID = " + id.ToString() + " Email : " + text;
                    break;
                case 21:
                    Message += "Added New Product To The Shop  Product ID = " + id.ToString() + " Name : " + text;
                    report.Product_ID = id;
                    break;
                case 22:
                    Message += "Edited Product Information in The Shop  Product ID = " + id.ToString() + " Name : " + text;
                    report.Product_ID = id;
                    break;
                case 23:
                    Message += "Deleted Product from The Shop  Product ID = " + id.ToString() + " Name : " + text;
                    break;
                case 3:
                    Message += "Made a Sale on Product in The Shop  Product ID = " + id.ToString() + " Sale = " + text + "%";
                    report.Product_ID = id;
                    break;
                case 4:
                    Message += "Changed Order Status in The System  Order ID = " + id.ToString() + " Status : " + text;
                    break;
                default:
                    break;
            }
            report.User_ID = user.ID;
            report.Description = Message;
            report.Type_ID = 2;
            report.Time = DateTime.Now;
            db.reports.Add(report);
            db.SaveChanges();
        }

        //command 5 (Register)      id : UserID ,    text : UserEmail
        //command 61 (Add Offer)    id : OfferID ,   text : OfferName
        //command 62 (Edit Offer)   id : OfferID ,   text : OfferName
        //command 63 (Delete Offer) id : OfferID ,   text : OfferName
        //command 71 (Make Order)   id : ProductID , text : ProductName
        //command 72 (Cancel Order) id : ProductID , text : ProductName
        public static void CreateUserReport(int userid, int command, int id, string text = "")
        {
            Entities db = new Entities();
            user user = db.users.Find(userid);
            report report = new report();
            report.Product_ID = null;
            string Message = " User " + user.Name.ToString() + "  Has ";
            switch (command)
            {
                case 5:
                    Message += "Registered To The System  User ID = " + id.ToString() + " Email : " + text;
                    break;
                case 61:
                    Message += "Added New Offer To The Shop Offer ID = " + id.ToString() + " Name : " + text;
                    report.Product_ID = id;
                    break;
                case 62:
                    Message += "Edited Offer Information in The Shop  Offer ID = " + id.ToString() + " Name : " + text;
                    report.Product_ID = id;
                    break;
                case 63:
                    Message += "Deleted Offer from The System  Shop ID = " + id.ToString() + " Name : " + text;
                    report.Product_ID = id;
                    break;
                case 71:
                    Message += "Made Order To The System  Order ID = " + id.ToString();
                    break;
                case 72:
                    Message += "Canceled Order from The System  Order ID = " + id.ToString();
                    break;
            }
            report.User_ID = user.ID;
            report.Description = Message;
            report.Type_ID = 2;
            report.Time = DateTime.Now;
            db.reports.Add(report);
            db.SaveChanges();
        }
        

    }
}