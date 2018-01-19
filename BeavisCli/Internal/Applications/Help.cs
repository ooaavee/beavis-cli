﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace BeavisCli.Internal.Applications
{
    internal class Help : WebCliApplication
    {
        private readonly BeavisCliSandbox _sandbox;
        private readonly WebCliOptions _options;

        public Help(BeavisCliSandbox sandbox, IOptions<WebCliOptions> options) : base("help", "Displays help")
        {
            _sandbox = sandbox;
            _options = options.Value;
        }

        public override async Task ExecuteAsync(WebCliContext context)
        {
            await OnExecuteAsync(() =>
            {
                var group1 = new List<WebCliApplication>();
                var group2 = new List<WebCliApplication>();

                foreach (WebCliApplication app in _sandbox.GetApplications(context.HttpContext))
                {
                    bool isOneOfDefaultApps = app.GetType().Assembly.Equals(GetType().Assembly);

                    // if default apps are not visible for help
                    if (isOneOfDefaultApps)
                    {
                        if (!_options.AreDefaultApplicationsBrowsable)
                        {
                            continue;
                        }
                    }

                    // apps can decide if they are visible for help
                    if (!app.IsBrowsable(context))
                    {
                        continue;
                    }

                    // ignore 'help'
                    if (app.GetType() == GetType())
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty(app.Name))
                    {
                        continue;
                    }

                    if (isOneOfDefaultApps)
                    {
                        // these are apps from this assembly
                        group1.Add(app);
                    }
                    else
                    {
                        // these are apps from other assemblies
                        group2.Add(app);
                    }
                }


                var lines = new List<Tuple<string, string>>();
                foreach (WebCliApplication app in group1.Concat(group2))
                {
                    lines.Add(new Tuple<string, string>(app.Name, app.Description));
                }

                context.Response.WriteInformation("List of supported applications:");

                foreach (string text in TerminalUtil.MakeBeautifulLines(lines))
                {
                    context.Response.WriteInformation(text);                    
                }

                context.Response.WriteEmptyLine();

                return Exit(context);
            }, context);

        }
    }
}
