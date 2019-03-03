﻿using Discord;
using Discord.WebSocket;
using Espeon.Databases.CommandStore;
using Espeon.Databases.Entities;
using Espeon.Databases.GuildStore;
using Espeon.Databases.UserStore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Espeon.Services
{
    public class StarboardService : BaseService
    {
        [Inject] private readonly DiscordSocketClient _client;
        [Inject] private readonly IServiceProvider _services;

        private readonly static Emoji Star = new Emoji("⭐");

        public override Task InitialiseAsync(UserStore userStore, GuildStore guildStore, CommandStore commandStore, IServiceProvider services)
        {
            _client.ReactionAdded += ReactionAddedAsync;

            return Task.CompletedTask;
        }

        private async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (!(channel is SocketTextChannel textChannel))
                return;

            if (!reaction.Emote.Equals(Star))
                return;

            using var guildStore = _services.GetService<GuildStore>();
            var guild = await guildStore.GetOrCreateGuildAsync(textChannel.Guild, x => x.StarredMessages);

            if (!(textChannel.Guild.GetTextChannel(guild.StarboardChannelId) is SocketTextChannel starChannel))
                return;

            var message = await msg.GetOrDownloadAsync();

            var foundMessage = guild.StarredMessages
                .FirstOrDefault(x => x.Id == message.Id || x.StarboardMessageId == message.Id);

            var count = message.Reactions[Star].ReactionCount;
            var m = $"{Star} **{count}** - {(message.Author as IGuildUser).GetDisplayName()} in <#{message.Channel.Id}>";

            if (foundMessage is null)
            {
                var users = await message.GetReactionUsersAsync(Star, count).FlattenAsync();

                var builder = new EmbedBuilder
                {
                    Author = new EmbedAuthorBuilder
                    {
                        Name = (message.Author as IGuildUser).GetDisplayName(),
                        IconUrl = message.Author.GetAvatarOrDefaultUrl()
                    },
                    Description = message.Content
                };

                if (message.Embeds.FirstOrDefault() is IEmbed embed)
                {
                    if (embed.Type == EmbedType.Image || embed.Type == EmbedType.Gifv)
                        builder.WithImageUrl(embed.Url);                        
                }

                if(message.Attachments.FirstOrDefault() is IAttachment attachment)
                {
                    var extensions = new[] { "png", "jpeg", "jpg", "gif", "webp" };

                    if (extensions.Any(x => attachment.Url.EndsWith(x)))
                        builder.WithImageUrl(attachment.Url);
                }

                var newStar = await starChannel.SendMessageAsync(m, embed: builder.Build());

                guild.StarredMessages.Add(new StarredMessage
                {
                    AuthorId = message.Author.Id,
                    ChannelId = message.Channel.Id,
                    Id = message.Id,
                    StarboardMessageId = newStar.Id,
                    ReactionUsers = users.Select(x => x.Id).ToList(),
                    ImageUrl = builder.ImageUrl
                });

                await guildStore.SaveChangesAsync();
            }
            else
            {
                if (foundMessage.ReactionUsers.Contains(reaction.UserId))
                    return;

                foundMessage.ReactionUsers.Add(reaction.UserId);

                var fetchedMessage = await starChannel.GetMessageAsync(foundMessage.StarboardMessageId) as IUserMessage;

                await fetchedMessage.ModifyAsync(x => x.Content = m);

                await guildStore.SaveChangesAsync();
            }
        }
    }
}
