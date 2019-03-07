﻿using Discord.WebSocket;
using Espeon.Commands;
using Espeon.Commands.Interactive.Criteria;
using Espeon.Services;

namespace Espeon.Commands.Interactive.Paginator
{
    public class DefaultPaginator : PaginatorBase
    {
        public override EspeonContext Context { get; }
        public override InteractiveService Interactive { get; }
        public override MessageService MessageService { get; }
        public override PaginatorOptions Options { get; }
        public override ICriterion<SocketReaction> Criterion { get; }

        public DefaultPaginator(EspeonContext context, InteractiveService interactive,
            MessageService messageService, PaginatorOptions options, ICriterion<SocketReaction> criterion)
        {
            Context = context;
            Interactive = interactive;
            MessageService = messageService;
            Options = options;
            Criterion = criterion;
        }
    }
}
