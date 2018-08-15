﻿using System;
using System.Threading.Tasks;

namespace BeavisCli.Services
{
    public class UnauthorizedHandler : IUnauthorizedHandler
    {
        public virtual Task OnUnauthorizedAsync(CommandContext context)
        {
            context.WriteError("Unauthorized");
            return Task.CompletedTask;
        }
    }
}