﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BeavisCli
{
    public interface IJob
    {
        Task ExecuteAsync(HttpContext context, WebCliResponse response);
    }
}