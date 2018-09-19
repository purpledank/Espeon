﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Espeon.Attributes;
using Espeon.Helpers;

namespace Espeon.Core
{
    class Core
    {
        private static async Task Main()
        {
            var serviceCollection = new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    AlwaysDownloadUsers = true,
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 20
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    CaseSensitiveCommands = false
                }))
                .AddSingleton(new HttpClient())
                .AddSingleton(new Random());

            var services = AssemblyHelper.GetAllTypesWithAttribute<ServiceAttribute>();

            foreach (var service in services)
                serviceCollection.AddSingleton(service);

            var builtProvider = serviceCollection.BuildServiceProvider();

            var startup = new BotCore(builtProvider);
            await startup.RunBotAsync();
        }
    }
}