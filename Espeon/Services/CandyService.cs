﻿using System;
using System.Threading.Tasks;
using Espeon.Attributes;
using Espeon.Core.Entities.User;

namespace Espeon.Services
{
    [Service]
    public class CandyService
    {
        private readonly DatabaseService _database;

        public CandyService(DatabaseService database)
        {
            _database = database;
        }

        public async Task UpdateCandiesAsync(ulong id, bool isClaim, int amount)
        {
            var bot = await _database.GetBotUserAsync();
            if (id == bot.Id) return;
            if (amount < 0)
            {
                bot.RareCandies -= amount;
                _database.UpdateObject("users", bot);
            }
            var user = await _database.GetObjectAsync<UserObject>("users", id);
            user.RareCandies += amount;
            if (isClaim)
                user.LastClaimed = DateTime.UtcNow;
            _database.UpdateObject("users", user);
        }

        public async Task<bool> CanClaimAsync(ulong id)
        {
            var user = await _database.GetObjectAsync<UserObject>("users", id);
            return DateTime.UtcNow - user.LastClaimed > TimeSpan.FromHours(8);
        }

        public async Task<int> GetCandiesAsync(ulong id)
            => (await _database.GetObjectAsync<UserObject>("users", id)).RareCandies;
    }
}
