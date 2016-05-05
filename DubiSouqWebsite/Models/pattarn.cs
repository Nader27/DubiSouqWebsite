using DubiSouqWebsite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DubiSouqWebsite
{
    public  interface account
    {
        void editprofile(BaseViewModels model);
    }
    public class clintreg : account
    {
        public void editprofile(BaseViewModels model)
        {
            Entities db = new Entities();
            model.user.Active = true;
            model.user.Type_id = 1;
            db.users.Add(model.user);
            db.SaveChanges();
            model.address.User_ID = db.users.Max(m => m.ID);
            db.addresses.Add(model.address);
            db.SaveChanges();
        }
    }
    public class adminreg : account
    {
        public void editprofile(BaseViewModels model)
        {
            Entities db = new Entities();
            model.user.Active = true;
            db.users.Add(model.user);
            db.SaveChanges();
        }
    }
}