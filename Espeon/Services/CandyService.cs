﻿using Discord;
using Espeon.Commands;
using System;
using System.Threading.Tasks;
using Espeon.Databases.UserStore;

namespace Espeon.Services
{
    public class CandyService : BaseService
    {
        [Inject] private Random _random;

        private Random Random => _random ?? (_random = new Random());

        public CandyService(IServiceProvider services) : base(services)
        {
        }

        public Task UpdateCandiesAsync(EspeonContext context, ulong id, int amount)
            => UpdateCandiesAsync(context, context.UserStore, id, amount);

        public async Task UpdateCandiesAsync(EspeonContext context, UserStore store, ulong id, int amount)
        {
            var bot = context.Client.CurrentUser;

            if (amount < 0 && id != bot.Id)
            {
                var espeon = await store.GetOrCreateUserAsync(bot);

                espeon.CandyAmount += Math.Abs(amount);
                store.Update(espeon);
            }

            var user = await store.GetOrCreateUserAsync(context.User);
            user.CandyAmount += amount;

            if (user.CandyAmount > user.HighestCandies)
                user.HighestCandies = user.CandyAmount;

            store.Update(user);

            await store.SaveChangesAsync();
        }

        public async Task TransferCandiesAsync(EspeonContext context, IUser sender, IUser receiver, int amount)
        {
            var foundSender = await context.UserStore.GetOrCreateUserAsync(sender);
            var foundReceiver = await context.UserStore.GetOrCreateUserAsync(receiver);

            foundSender.CandyAmount -= amount;
            foundReceiver.CandyAmount += amount;

            if (foundReceiver.CandyAmount > foundReceiver.HighestCandies)
                foundReceiver.HighestCandies = foundReceiver.CandyAmount;

            context.UserStore.Update(foundReceiver);
            context.UserStore.Update(foundSender);

            await context.UserStore.SaveChangesAsync();
        }

        public async Task<int> GetCandiesAsync(EspeonContext context, IUser user)
        {
            var foundUser = await context.UserStore.GetOrCreateUserAsync(user);
            return foundUser.CandyAmount;
        }

        public async Task<(bool IsSuccess, int Amount, TimeSpan Cooldown)> TryClaimCandiesAsync(EspeonContext context, IUser toClaim)
        {
            var user = await context.UserStore.GetOrCreateUserAsync(toClaim);
            var difference = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(user.LastClaimedCandies);

            if (difference < TimeSpan.FromHours(8))
            {
                return (false, 0, TimeSpan.FromHours(8) - difference);
            }

            var amount = Random.Next(1, 21);
            user.CandyAmount += amount;

            if (user.CandyAmount > user.HighestCandies)
                user.HighestCandies = user.CandyAmount;

            user.LastClaimedCandies = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            context.UserStore.Update(user);

            await context.UserStore.SaveChangesAsync();

            return (true, amount, TimeSpan.FromHours(8));
        }
    }
}
