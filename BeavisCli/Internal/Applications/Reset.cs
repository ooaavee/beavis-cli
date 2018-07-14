﻿using BeavisCli.JavaScriptStatements;
using System.Threading.Tasks;

namespace BeavisCli.Internal.Applications
{
    [WebCliApplicationDefinition(Name = "reset", Description = "Resets the terminal.")]
    internal class Reset : WebCliApplication
    {
        public override async Task ExecuteAsync(WebCliContext context)
        {
            await OnExecuteAsync(() =>
            {
                context.Response.AddJavaScript(new ClearTerminal());
                context.Response.AddJavaScript(new ClearTerminalHistory());
                return Exit(context);
            }, context);

        }
    }
}
