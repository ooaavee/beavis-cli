﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BeavisCli.Internal
{
    /// <summary>
    /// Handler for unauthorized application execution attempts.
    /// </summary>
    internal class DefaultUnauthorizedHandler : IUnauthorizedHandler
    {
        public void HandleUnauthorizedApplicationExecution(WebCliContext context)
        {
            context.Response.WriteError("Unauthorized");
        }
    }
}