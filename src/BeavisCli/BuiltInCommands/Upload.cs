﻿using System.Threading.Tasks;

namespace BeavisCli.BuiltInCommands
{
    [Command("upload", "A tool for uploading files.")]
    public class Upload : ICommand
    {
        public async Task ExecuteAsync(CommandBuilder builder, CommandContext context)
        {
            await context.OnExecuteAsync(() =>
            {            
                return context.Exit();
            });
        }
    }
}
