// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;

#nullable enable

namespace osu.Desktop.LegacyIpc
{
    /// <summary>
    /// Provides IPC to legacy osu! clients.
    /// </summary>
    public class LegacyTcpIpcProvider : TcpIpcProvider
    {
        private static readonly Logger logger = Logger.GetLogger("legacy-ipc");

        public LegacyTcpIpcProvider()
            : base(45357)
        {
            MessageReceived += msg =>
            {
                try
                {
                    logger.Add("Processing legacy IPC message...");
                    logger.Add($"    {msg.Value}", LogLevel.Debug);

                    // See explanation in LegacyIpcMessage for why this is done this way.
                    var legacyData = ((JObject)msg.Value).ToObject<LegacyIpcMessage.Data>();
                    object value = parseObject((JObject)legacyData!.MessageData, legacyData.MessageType);

                    return new LegacyIpcMessage
                    {
                        Value = onLegacyIpcMessageReceived(value)
                    };
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
                    return value.ToObject<LegacyIpcDifficultyCalculationRequest>()
                           ?? throw new InvalidOperationException($"Failed to parse request {value}");

                case nameof(LegacyIpcDifficultyCalculationResponse):
                    return value.ToObject<LegacyIpcDifficultyCalculationResponse>()
                           ?? throw new InvalidOperationException($"Failed to parse request {value}");

                default:
                    throw new ArgumentException($"Unsupported object type {type}");
            }
        }

        private object onLegacyIpcMessageReceived(object message)
        {
            switch (message)
            {
                case LegacyIpcDifficultyCalculationRequest req:
                    try
                    {
                        var ruleset = getLegacyRulesetFromID(req.RulesetId);

                        Mod[] mods = ruleset.ConvertFromLegacyMods((LegacyMods)req.Mods).ToArray();
                        WorkingBeatmap beatmap = new FlatFileWorkingBeatmap(req.BeatmapFile, _ => ruleset);

                        return new LegacyIpcDifficultyCalculationResponse
                        {
                            StarRating = ruleset.CreateDifficultyCalculator(beatmap).Calculate(mods).StarRating
                        };
                    }
                    catch
                    {
                        return new LegacyIpcDifficultyCalculationResponse();
                    }

                default:
                    throw new ArgumentException($"Unsupported message type {message}");
            }
        }

        private static Ruleset getLegacyRulesetFromID(int rulesetId)
        {
            switch (rulesetId)
            {
                case 0:
                    return new OsuRuleset();

                case 1:
                    return new TaikoRuleset();

                case 2:
                    return new CatchRuleset();

                case 3:
                    return new ManiaRuleset();

                default:
                    throw new ArgumentException("Invalid ruleset id");
            }
        }
    }
}
