using Nololiyt.Captcha.Interfaces;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nololiyt.NCaptchaExtensions.Redis
{
    public sealed class TicketFactory : ITicketFactory
    {
        private readonly RedisDatabaseDefiniton databaseDefiniton;
        private readonly string prefix;
        public TicketFactory(string prefix, TimeSpan? ticketsLifeTime,
            RedisDatabaseDefiniton databaseDefiniton)
        {
            this.TicketsLifeTime = ticketsLifeTime;
            this.databaseDefiniton = databaseDefiniton;
            this.prefix = prefix;
        }
        public TimeSpan? TicketsLifeTime { get; }

        public void Dispose()
        {
            this.databaseDefiniton.Dispose();
            GC.SuppressFinalize(this);
        }
        public async ValueTask<string> GenerateNewAsync(CancellationToken cancellationToken = default)
        {
            string ticket;
            do
            {
                ticket = $"{this.prefix}{Guid.NewGuid():N}";
            }
            while (!await this.databaseDefiniton.Database.StringSetAsync(
                ticket, string.Empty, this.TicketsLifeTime, When.NotExists).ConfigureAwait(false));
            return ticket;
        }
        public async ValueTask<bool> VerifyAsync(string ticket, CancellationToken cancellationToken = default)
        {
            return await this.databaseDefiniton.Database.KeyDeleteAsync(ticket).ConfigureAwait(false);
        }
    }
}
