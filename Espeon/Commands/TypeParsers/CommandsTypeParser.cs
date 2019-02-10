﻿using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Commands.TypeParsers
{
    public class CommandsTypeParser : TypeParser<IReadOnlyCollection<Command>>
    {
        public override async Task<TypeParserResult<IReadOnlyCollection<Command>>> ParseAsync(string value, ICommandContext context, IServiceProvider provider)
        {
            var service = provider.GetService<CommandService>();
            var commands = service.GetAllCommands();

            var found = commands.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase) 
                || x.FullAliases.Any(y => y.Contains(value, StringComparison.InvariantCultureIgnoreCase))).ToArray();

            var canExecute = new List<Command>();

            foreach(var command in found)
            {
                var result = await command.RunChecksAsync(context, provider);

                if(result.IsSuccessful)
                {
                    canExecute.Add(command);
                }
            }

            if (canExecute.Count == 0)
                return new TypeParserResult<IReadOnlyCollection<Command>>($"Failed to find any commands matching {value}");

            return new TypeParserResult<IReadOnlyCollection<Command>>(found);
        }
    }
}
