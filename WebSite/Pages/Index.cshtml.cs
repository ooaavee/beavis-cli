﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebSite.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(IEnumerable<Kantaluokka> kkkk)
        {

        }

        public void OnGet()
        {

        }
    }
}
