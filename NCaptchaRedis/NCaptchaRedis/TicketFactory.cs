using Nololiyt.Captcha.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nololiyt.NCaptchaExtensions.Redis
{
    public sealed class TicketFactory : ITicketFactory
    {
        private readonly ConnectionMultiplexer connectionMultiplexer;
        private readonly IDatabase database;
        private readonly string prefix;
        public TicketFactory(string prefix, TimeSpan? ticketsLifeTime, string redisConnectionString, int db = -1)
        {
            this.TicketsLifeTime = ticketsLifeTime;
            this.connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            this.database = this.connectionMultiplexer.GetDatabase(db);
            this.prefix = prefix;
        }
        public TimeSpan? TicketsLifeTime { get; }

        public void Dispose()
        {
            this.connectionMultiplexer.Dispose();
            GC.SuppressFinalize(this);
        }
        public async ValueTask<string> GenerateNewAsync(CancellationToken cancellationToken = default)
        {
            string ticket;
            do
            {
                ticket = $"{this.prefix}{Guid.NewGuid():N}";
            }
            while (!await this.database.StringSetAsync(
                ticket, ticket, this.TicketsLifeTime, When.NotExists).ConfigureAwait(false));
            return ticket;
        }
        public async ValueTask<bool> VerifyAsync(string ticket, CancellationToken cancellationToken = default)
        {
            var result = await this.database.StringGetAsync(ticket).ConfigureAwait(false);
            if (result.IsNull)
                return false;
            return (string)result == ticket;
        }
    }
}
