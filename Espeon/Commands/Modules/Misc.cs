﻿using Casino.Common.Discord.Net;
using Discord;
using Discord.Net;
using Espeon.Services;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq;
using Newtonsoft.Json.Linq;
using Qmmands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Espeon.Commands
{
    /*
     * Cat Fact
     * Joke
     * Chuck
     * Gif
     * Clear Responses
     * Admins
     * Mods
     * Help
     * Emote
     */
    [Name("Misc")]
    [Description("Commands that don't have their own home")]
    public class Misc : EspeonBase
    {
        public Config Config { get; set; }
        public EmotesService Emotes { get; set; }
        public IHttpClientFactory ClientFactory { get; set; }

        private readonly Emoji _delete = new Emoji("🚮");

        [Command("Ping")]
        [Name("Ping")]
        [Description("View the bots gateway latency and REST ping")]
        public async Task PingAsync()
        {
            var user = Context.Invoker;
            var latency = Context.Client.Latency;

            var resp = Responses.GetResponse(Context.Command.Module.Name, 
                Context.Command.Name, user.ResponsePack, 0, latency);

            var response = ResponseBuilder.Message(Context, resp);

            var sw = new Stopwatch();
            sw.Start();

            var message = await SendMessageAsync(response);

            sw.Stop();

            resp = Responses.GetResponse(Context.Command.Module.Name, 
                Context.Command.Name, user.ResponsePack, 1, latency, sw.ElapsedMilliseconds);

            response = ResponseBuilder.Message(Context, resp);

            await message.ModifyAsync(x => x.Embed = response);
        }


        [Command("Catfact")]
        [RunMode(RunMode.Parallel)]
        [Name("Get Catfact")]
        [Cooldown(1, 1, CooldownMeasure.Minutes, CooldownBucket.API)]
        [Description("Gets a random cat fact")]
        public async Task GetCatfactAsync()
        {
            var client = ClientFactory.CreateClient();

            using var response = await client.GetAsync("https://catfact.ninja/fact");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(content);

                await SendOkAsync(0, obj["fact"]);
            }
            else
            {
                await SendNotOkAsync(1);
            }
        }

        [Command("Joke")]
        [RunMode(RunMode.Parallel)]
        [Name("Get Joke")]
        [Cooldown(1, 1, CooldownMeasure.Minutes, CooldownBucket.API)]
        [Description("Gets a random joke")]
        public async Task GetJokeAsync()
        {
            var client = ClientFactory.CreateClient();

            using var response = await client.GetAsync("https://icanhazdadjoke.com/");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(content);

                await SendOkAsync(0, obj["joke"]);
            }
            else
            {
                await SendNotOkAsync(1);
            }
        }

        [Command("Gif")]
        [RunMode(RunMode.Parallel)]
        [Name("Get Gif")]
        [Cooldown(1, 1, CooldownMeasure.Minutes, CooldownBucket.API)]
        [Description("Gets a gif based on the specified search")]
        public async Task GetGifAsync([Remainder] string search)
        {
            var client = ClientFactory.CreateClient();

            using var response = await client
                .GetAsync($"https://api.giphy.com/v1/gifs/random?api_key={Config.GiphyAPIKey}&rating=r&tag={search}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(content);

                if (!obj["data"].Any())
                {
                    await SendNotOkAsync(0);
                }
                else
                {
                    var gifUrl = obj["data"]["image_original_url"];

                    using var stream = await client.GetStreamAsync($"{gifUrl}");

                    try
                    {
                        await SendFileAsync(stream, $"{search}.gif");
                    }
                    catch (HttpException ex) when (ex.DiscordCode == 40003)
                    {
                        await SendNotOkAsync(1);
                    }
                }
            }
            else
            {
                await SendNotOkAsync(2);
            }
        }


        [Command("Clear")]
        [RunMode(RunMode.Parallel)]
        [Name("Clear Messages")]
        [Description("Clears the bots responses")]
        public Task ClearMessagesAsync(int amount = 2)
            => Message.DeleteMessagesAsync(Context, amount);

        [Command("help")]
        [Name("Help Modules")]
        [Description("View generic help on the bot")]
        public async Task HelpAsync()
        {
            var modules = Services.GetService<CommandService>().GetAllModules();

            var canExecute = new List<Module>();

            foreach (var module in modules)
            {
                var result = await module.RunChecksAsync(Context, Services);

                if (!result.IsSuccessful)
                    continue;

                var results = await module.Commands.Select(x => x.RunChecksAsync(Context, Services)).AllAsync();

                if (results.Count(x => x.IsSuccessful) > 0)
                    canExecute.Add(module);
            }

            var prefix = GetPrefix(Context.PrefixUsed);

            var builder = GetBuilder()
                .AddField("Modules", string.Join(", ", canExecute
                    .Select(x => $"`{Format.Sanitize(ulong.TryParse(x.Name, out _) ? Context.Guild.Name : x.Name)}`")))
                .WithFooter($"To view help with a specific module invoke {prefix}help Module")
                .WithDescription($"Hello, my name is Espeon{Emotes.Collection["Espeon"]}! " +
                                 $"You can invoke my commands either by mentioning me or using the `{Format.Sanitize(prefix)}` prefix!");

            var message = await SendMessageAsync(builder.Build());

            var deleteCallback = new DeleteCallback(Context, message, _delete,
                new ReactionFromSourceUser(Context.User.Id));

            await TryAddCallbackAsync(deleteCallback);
        }

        [Command("help")]
        [Name("Help Module")]
        [Priority(0)]
        [Description("View help on the specified module")]
        public async Task HelpAsync([Remainder] Module module)
        {
            var prefix = GetPrefix(Context.PrefixUsed);

            var canExecute = new List<Command>();

            foreach (var command in module.Commands)
            {
                var result = await command.RunChecksAsync(Context, Services);

                if (result.IsSuccessful)
                    canExecute.Add(command);
            }

            var builder = GetBuilder()
                .AddField(ulong.TryParse(module.Name, out _) ? Context.Guild.Name : module.Name ?? "\u200b",
                    module.Description ?? "\u200b")
                .AddField("Commands", string.Join(", ",
                    canExecute.Select(x => $"`{Format.Sanitize(x.FullAliases.First().ToLower())}`")))
                .WithFooter($"To view help with a specific command invoke {prefix}help Command Name");

            var message = await SendMessageAsync(builder.Build());

            var deleteCallback = new DeleteCallback(Context, message, _delete,
                new ReactionFromSourceUser(Context.User.Id));

            await TryAddCallbackAsync(deleteCallback);
        }

        [Command("help")]
        [Name("Help Commands")]
        [Priority(1)]
        [Description("View help on the specified command(s)")]
        public async Task HelpAsync([Remainder] IReadOnlyCollection<Command> commands)
        {
            var batch = commands.Batch(3).Select(x => x.ToArray()).ToArray();
            var toSend = new List<EmbedBuilder>();

            var prefix = GetPrefix(Context.PrefixUsed);

            string GetParameters(Command command)
            {
                var sb = new StringBuilder();

                foreach (var param in command.Parameters)
                {
                    sb.Append(Utilities.ExampleUsage.TryGetValue(param.Type, out var usage)
                        ? $" `{usage}`"
                        : $" `{param.Name}`");
                }

                return sb.ToString();
            }

            if (batch.Length == 1)
            {
                if (batch[0].Length == 1)
                {
                    var cmd = batch[0][0];

                    

                    var builder = GetBuilder()
                        .AddField(cmd.Name,
                            $"{prefix}{cmd.FullAliases.First().ToLower()}{GetParameters(cmd)}")
                        .AddField("Summary", cmd.Description ?? "\u200b")
                        .AddField("Aliases", string.Join(", ", cmd.FullAliases.Select(x => x.ToLower())) ?? "\u200b");

                    var message = await SendMessageAsync(builder.Build());

                    var deleteCallback = new DeleteCallback(Context, message, _delete,
                        new ReactionFromSourceUser(Context.User.Id));

                    await TryAddCallbackAsync(deleteCallback);

                    return;
                }
            }

            foreach (var col in batch)
            {
                var builder = GetBuilder()
                    .WithFooter($"Type {prefix}help Command Name to view help for that specific command");

                foreach (var command in col)
                {
                    //TODO add summaries/examples
                    builder.AddField(command.Name,
                            $"{prefix}{command.FullAliases.First().ToLower()}{GetParameters(command)}");
                }

                toSend.Add(builder);
            }

            if (toSend.Count == 1)
            {
                var message = await SendMessageAsync(toSend[0].Build());

                var deleteCallback = new DeleteCallback(Context, message, _delete,
                    new ReactionFromSourceUser(Context.User.Id));

                await TryAddCallbackAsync(deleteCallback);

                return;
            }

            var i = 0;
            var options = PaginatorOptions.Default(
                toSend.ToDictionary(x => i++,
                    x => (string.Empty, x.WithFooter($"Page {i}/{toSend.Count}").Build())));

            var paginator = new DefaultPaginator(Context, Interactive, Message,
                options, new ReactionFromSourceUser(Context.User.Id));

            await SendPaginatedMessageAsync(paginator);
        }

        private EmbedBuilder GetBuilder()
        {
            return new EmbedBuilder
            {
                Color = Utilities.EspeonColor,
                Title = "Espeon's Help",
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.User.GetAvatarOrDefaultUrl(),
                    Name = Context.User.GetDisplayName()
                },
                ThumbnailUrl = Context.Guild.CurrentUser.GetAvatarOrDefaultUrl(),
                Timestamp = DateTimeOffset.UtcNow
            };
        }

        private string GetPrefix(string prefix)
        {
            return prefix == Context.Guild.CurrentUser.Mention
                ? $"@{Context.Guild.CurrentUser.GetDisplayName()}"
                : prefix;
        }

        [Command("Mods")]
        [Name("List Mods")]
        [Description("List the guilds bot moderators")]
        public async Task ListModsAsync()
        {
            var currentGuild = Context.CurrentGuild;

            var modTasks = currentGuild.Moderators.Select(async x => Context.Guild.GetUser(x) as IGuildUser
                ?? await Context.Client.Rest.GetGuildUserAsync(Context.Guild.Id, x)).Where(x => !(x is null));

            var mods = await modTasks.AllAsync();

            if (mods.Length == 0)
            {
                await SendOkAsync(0);
            }
            else
            {
                await SendOkAsync(1,
                    string.Join(", ", mods.Select(x => $"{Format.Sanitize(x.GetDisplayName())}")));
            }
        }

        [Command("Admins")]
        [Name("List Admins")]
        [Description("List the guilds bot admins")]
        public async Task ListAdminsAsync()
        {
            var currentGuild = Context.CurrentGuild;

            var adminTasks = currentGuild.Admins.Select(async x => Context.Guild.GetUser(x) as IGuildUser
                ?? await Context.Client.Rest.GetGuildUserAsync(Context.Guild.Id, x)).Where(x => !(x is null));

            var admins = await adminTasks.AllAsync();

            if (admins.Length == 0)
            {
                //should never happen but sure
                await SendOkAsync(0);
            }
            else
            {
                await SendOkAsync(1,
                    string.Join(", ", admins.Select(x => $"{Format.Sanitize(x.GetDisplayName())}")));
            }
        }

        [Command("Emote")]
        [Name("Steal Emote")]
        [RequirePermissions(PermissionTarget.Bot, GuildPermission.ManageEmojis)]
        [RequirePermissions(PermissionTarget.User, GuildPermission.ManageEmojis)]
        [Description("Adds the specified emote(s) to your guild")]
        public async Task StealEmoteAsync(params Emote[] emotes)
        {
            var animatedCount = Context.Guild.Emotes.Count(x => x.Animated);
            var normalCount = Context.Guild.Emotes.Count(x => !x.Animated);

            var failed = new List<Emote>();
            var client = ClientFactory.CreateClient();
            Stream stream;

            var added = 0;

            foreach (var emote in emotes)
            {
                if (emote.Animated && animatedCount >= 50)
                {
                    failed.Add(emote);
                    continue;
                }

                if (!emote.Animated && normalCount >= 50)
                {
                    failed.Add(emote);
                    continue;
                }

                stream = await client.GetStreamAsync(emote.Url);
                await Context.Guild.CreateEmoteAsync(emote.Name, new Image(stream));

                animatedCount = Context.Guild.Emotes.Count(x => x.Animated);
                normalCount = Context.Guild.Emotes.Count(x => !x.Animated);

                added++;

                stream.Dispose();
            }

            if (failed.Count < emotes.Length)
                await SendOkAsync(0, added);

            if (failed.Count > 0)
                await SendNotOkAsync(1, string.Join(", ", failed.Select(x => x.Name)));
        }
    }
}
