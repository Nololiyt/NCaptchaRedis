using Nololiyt.Captcha.CaptchaFactories;
using Nololiyt.Captcha.Interfaces;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nololiyt.NCaptchaExtensions.Redis
{
    public sealed class StringAnswerSaver : ICaptchaAnswerSaver<string>
    {
        private readonly ConnectionMultiplexer connectionMultiplexer;
        private readonly IDatabase database;
        private readonly string prefix;
        public StringAnswerSaver(string prefix, TimeSpan? answersLifeTime, string redisConnectionString, int db = -1)
        {
            this.AnswersLifeTime = answersLifeTime;
            this.connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            this.database = this.connectionMultiplexer.GetDatabase(db);
            this.prefix = prefix;
        }
        public TimeSpan? AnswersLifeTime { get; }
        public void Dispose()
        {
            this.connectionMultiplexer.Dispose();
            GC.SuppressFinalize(this);
        }
        public async ValueTask<string> SaveAsync(string answer,
            CancellationToken cancellationToken = default)
        {
            string id;
            do
            {
                id = $"{this.prefix}{Guid.NewGuid():N}";
            }
            while (!await this.database.StringSetAsync(
                id, answer, this.AnswersLifeTime, When.NotExists).ConfigureAwait(false));
            return id;
        }

        public async ValueTask<string?> TryGetAsync(string id, CancellationToken cancellationToken = default)
        {
            return await this.database.StringGetSetAsync(id, RedisValue.Null).ConfigureAwait(false);
        }
    }
}
