﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SharpLink;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Umbreon.Attributes;

namespace Umbreon.Services
{
    [Service]
    public class MusicService
    {
        private readonly DiscordSocketClient _client;
        private readonly LogService _log;
        private LavalinkManager _lavalinkManager;

        private
            ConcurrentDictionary<ulong, (LavalinkPlayer player, bool isPaused, ulong channelId,
                ConcurrentQueue<LavalinkTrack> queue)> _lavaCache =
                new ConcurrentDictionary<ulong, (LavalinkPlayer, bool, ulong, ConcurrentQueue<LavalinkTrack>)>();

        public MusicService(DiscordSocketClient client, LogService log)
        {
            _client = client;
            _log = log;
        }

        public async Task Initialise()
        {
            _lavalinkManager = new LavalinkManager(_client, new LavalinkManagerConfig
            {
                RESTHost = "localhost",
                RESTPort = 2333,
                WebSocketHost = "localhost",
                WebSocketPort = 80,
                Authorization = "casinobestpass",
                TotalShards = 1,
                LogSeverity = LogSeverity.Verbose
            });
            _lavalinkManager.Log += _log.LogEvent;
            await _lavalinkManager.StartAsync();
            _lavalinkManager.TrackEnd += TrackFinishedAsync;
        }

        public (LavalinkPlayer, bool, ulong, ConcurrentQueue<LavalinkTrack>) GetGuild(ICommandContext context)
        {
            return _lavaCache.TryGetValue(context.Guild.Id, out var found) ? found : (null, false, 0, null);
        }

        public async Task JoinAsync(ICommandContext context)
        {
            if (_lavaCache.ContainsKey(context.Guild.Id))
                await _lavalinkManager.LeaveAsync(context.Guild.Id);
            var player = await _lavalinkManager.JoinAsync((context.User as IGuildUser).VoiceChannel);
            if (!_lavaCache.TryAdd(context.Guild.Id,
                (player, false, context.Channel.Id, new ConcurrentQueue<LavalinkTrack>())))
            {
                _lavaCache[context.Guild.Id] = (player, false, context.Channel.Id, _lavaCache[context.Guild.Id].queue);
            }
        }

        public async Task<bool> PlayTrackAsync(ICommandContext context, LavalinkTrack track)
        {
            if (!_lavaCache.ContainsKey(context.Guild.Id))
                await JoinAsync(context);
            var currentGuild = _lavaCache[context.Guild.Id];
            var player = currentGuild.player;
            _lavaCache[context.Guild.Id].queue.Enqueue(track);
            if (!player.Playing && !currentGuild.isPaused)
                await player.PlayAsync(track);
            return _lavaCache[context.Guild.Id].queue.Count > 1;
        }

        public Task<LavalinkTrack> GetTrackAsync(string toSearch)
            => _lavalinkManager.GetTrackAsync($"ytsearch:{toSearch}");

        private async Task TrackFinishedAsync(LavalinkPlayer player, LavalinkTrack __, string reason)
        {
            if (reason == "REPLACED") return;
            var guildId = player.VoiceChannel.GuildId;
            _lavaCache[guildId].queue.TryDequeue(out _);
            if (_lavaCache[guildId].queue.TryPeek(out var track))
            {
                await player.PlayAsync(track);
                var channel = _client.GetChannel(_lavaCache[guildId].channelId) as SocketTextChannel;
                var embed = new EmbedBuilder
                {
                    Title = $"Now playing {track.Title}",
                    Color = Color.Red
                };
                await channel.SendMessageAsync(string.Empty, embed: embed.Build()); // TODO once overhauled fix this
            }
            else
            {
                await player.StopAsync();
            }
        }

        public async Task LeaveAsync(ICommandContext context)
        {
            if (!_lavaCache.ContainsKey(context.Guild.Id)) return;
            await _lavalinkManager.LeaveAsync(context.Guild.Id);
            _lavaCache.TryRemove(context.Guild.Id, out _);
        }

        public async Task SetVolumeAsync(ICommandContext context, uint volume)
        {
            if (_lavaCache.TryGetValue(context.Guild.Id, out var currentGuild))
                await currentGuild.player.SetVolumeAsync(volume);
        }

        public async Task PauseAsync(ICommandContext context)
        {
            if (!_lavaCache.TryGetValue(context.Guild.Id, out var currentGuild)) return;
            if (!currentGuild.isPaused)
            {
                await currentGuild.player.PauseAsync();
                _lavaCache[context.Guild.Id] = (currentGuild.player, true, currentGuild.channelId, currentGuild.queue);
            }
        }

        public async Task ResumeAsync(ICommandContext context)
        {
            if (!_lavaCache.TryGetValue(context.Guild.Id, out var currentGuild)) return;
            if (currentGuild.isPaused)
            {
                await currentGuild.player.ResumeAsync();
                _lavaCache[context.Guild.Id] = (currentGuild.player, false, currentGuild.channelId, currentGuild.queue);
            }
        }

        public async Task SkipSongAsync(ICommandContext context)
        {
            if (!_lavaCache.TryGetValue(context.Guild.Id, out var currentGuild)) return;
            currentGuild.queue.TryDequeue(out _);
            if (currentGuild.queue.TryPeek(out var track))
            {
                await currentGuild.player.PlayAsync(track);
                var channel = _client.GetChannel(currentGuild.channelId) as SocketTextChannel;
                var embed = new EmbedBuilder
                {
                    Title = $"Now playing {track.Title}",
                    Color = Color.Red
                };
                await channel.SendMessageAsync(string.Empty, embed: embed.Build()); // TODO once overhauled fix this
                _lavaCache[context.Guild.Id] = currentGuild;
            }
            else
            {
                await currentGuild.player.StopAsync();
            }
        }
    }
}