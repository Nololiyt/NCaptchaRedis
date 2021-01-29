using Nololiyt.Captcha.CaptchaFactories;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nololiyt.NCaptchaExtensions.Redis
{
    public sealed class StringAnswerSaver : ICaptchaAnswerSaver<string>
    {
        private readonly RedisDatabaseDefiniton databaseDefiniton;
        private readonly string prefix;

        public StringAnswerSaver(string prefix, TimeSpan? answersLifeTime,
            RedisDatabaseDefiniton databaseDefiniton)
        {
            this.AnswersLifeTime = answersLifeTime;
            this.databaseDefiniton = databaseDefiniton;
            this.prefix = prefix;
        }
        public TimeSpan? AnswersLifeTime { get; }
        public void Dispose()
        {
            this.databaseDefiniton.Dispose();
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
            while (!await this.databaseDefiniton.Database.StringSetAsync(
                id, answer, this.AnswersLifeTime, When.NotExists).ConfigureAwait(false));
            return id;
        }

        public async ValueTask<string?> TryGetAsync(string id, CancellationToken cancellationToken = default)
        {
            var result = await this.databaseDefiniton.Database.StringGetAsync(id).ConfigureAwait(false);
            _ = this.databaseDefiniton.Database.KeyDeleteAsync(id);
            return result;
        }
    }
}
