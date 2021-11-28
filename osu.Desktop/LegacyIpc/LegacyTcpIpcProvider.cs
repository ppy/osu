// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using osu.Framework.Platform;

namespace osu.Desktop.LegacyIpc
{
    public class LegacyTcpIpcProvider : TcpIpcProvider
    {
        public new Func<object, object> MessageReceived;

        public LegacyTcpIpcProvider()
            : base(45357)
        {
            base.MessageReceived += msg =>
            {
                try
                {
                    var legacyData = ((JObject)msg.Value).ToObject<LegacyIpcMessage.Data>();
                    object value = parseObject((JObject)legacyData.MessageData, legacyData.MessageType);

                    object result = MessageReceived?.Invoke(value);
                    return result != null ? new LegacyIpcMessage { Value = result } : null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                return null;
            };
        }

        public Task SendMessageAsync(object message) => base.SendMessageAsync(new LegacyIpcMessage { Value = message });

        public async Task<T> SendMessageWithResponseAsync<T>(object message)
        {
            var result = await base.SendMessageWithResponseAsync(new LegacyIpcMessage { Value = message }).ConfigureAwait(false);

            var legacyData = ((JObject)result.Value).ToObject<LegacyIpcMessage.Data>();
            return (T)parseObject((JObject)legacyData.MessageData, legacyData.MessageType);
        }

        public new Task SendMessageAsync(IpcMessage message) => throw new InvalidOperationException("Use typed overloads.");

        public new Task<IpcMessage> SendMessageWithResponseAsync(IpcMessage message) => throw new InvalidOperationException("Use typed overloads.");

        private object parseObject(JObject value, string type)
        {
            switch (type)
            {
                case nameof(LegacyIpcDifficultyCalculationRequest):
                    return value.ToObject<LegacyIpcDifficultyCalculationRequest>();

                case nameof(LegacyIpcDifficultyCalculationResponse):
                    return value.ToObject<LegacyIpcDifficultyCalculationResponse>();

                default:
                    throw new ArgumentException($"Unknown type: {type}");
            }
        }
    }
}
