// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json.Linq;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace osu.Desktop.LegacyIpc
{
    /// <summary>
    /// Provides IPC to legacy osu! clients.
    /// </summary>
    public class LegacyTcpIpcProvider : TcpIpcProvider
    {
        private static readonly Logger logger = Logger.GetLogger("legacy-ipc");

        /// <summary>
        /// Invoked when a message is received from a legacy client.
        /// </summary>
        public new Func<object, object> MessageReceived;

        public LegacyTcpIpcProvider()
            : base(45357)
        {
            base.MessageReceived += msg =>
            {
                try
                {
                    logger.Add($"Processing legacy IPC message...");
                    logger.Add($"\t{msg.Value}", LogLevel.Debug);

                    var legacyData = ((JObject)msg.Value).ToObject<LegacyIpcMessage.Data>();
                    object value = parseObject((JObject)legacyData!.MessageData, legacyData.MessageType);

                    object result = MessageReceived?.Invoke(value);
                    return result != null ? new LegacyIpcMessage { Value = result } : null;
                }
                catch (Exception ex)
                {
                    logger.Add($"Processing IPC message failed: {msg.Value}", exception: ex);
                    return null;
                }
            };
        }

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
