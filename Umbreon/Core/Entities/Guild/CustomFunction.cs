﻿namespace Umbreon.Core.Entities.Guild
{
    public class CustomFunction
    {
        public string FunctionName { get; set; }
        public string FunctionCallback { get; set; }
        public ulong GuildId { get; set; }
        public bool IsPrivate { get; set; }
        public string Summary { get; set; }
    }
}