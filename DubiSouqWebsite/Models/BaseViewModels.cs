using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DubiSouqWebsite.Models
{
    public class BaseViewModels
    {

        public virtual user user { get; set; }

        public virtual address address { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

    }
}