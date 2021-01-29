using StackExchange.Redis;
using System;

namespace Nololiyt.NCaptchaExtensions.Redis
{
    public sealed class RedisDatabaseDefiniton : IDisposable
    {
        internal IDatabase Database { get; }
        private readonly ConnectionMultiplexer? connectionMultiplexer;
        public RedisDatabaseDefiniton(
            ConnectionMultiplexer connectionMultiplexer,
            int db = -1,
            bool disposeMultiplexerWhenDisposing = false)
        {
            this.Database = connectionMultiplexer.GetDatabase(db);
            if (disposeMultiplexerWhenDisposing)
                this.connectionMultiplexer = connectionMultiplexer;
        }
        public RedisDatabaseDefiniton(
            string redisConnectionString,
            int db = -1)
        {
            this.connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            this.Database = this.connectionMultiplexer.GetDatabase(db);
        }
        public RedisDatabaseDefiniton(IDatabase database)
        {
            this.Database = database;
        }

        public void Dispose()
        {
            this.connectionMultiplexer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
