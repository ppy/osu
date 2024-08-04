// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Holds all data about traps for our tournament.
    /// </summary>
    [Serializable]
    public class InstructionInfo
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public TeamColour Team;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public Steps Step;

        public int BeatmapID;

        private SpriteIcon icon = new SpriteIcon
        {
            Icon = FontAwesome.Regular.GrinWink,
            Colour = new OsuColour().Yellow,
        };

        private LocalisableString name = string.Empty;
        private LocalisableString description = string.Empty;

        private LocalisableString teamPrompt = string.Empty;

        /// <summary>
        /// A constructor to set up an instance of <see cref="InstructionInfo"/>.
        /// </summary>
        /// <param name="team">The current team.</param>
        /// <param name="step">The current step.</param>
        public InstructionInfo(TeamColour team = TeamColour.Neutral, Steps step = Steps.Default)
        {
            Step = step;

            teamPrompt = team == TeamColour.Red ? @"红队" : (team == TeamColour.Blue ? @"蓝队" : string.Empty);

            switch (Step)
            {
                case Steps.Protect:
                    name = @$"标记保图·{teamPrompt}";
                    description = @"被保护的图无法被禁与设置陷阱。";
                    icon.Icon = FontAwesome.Solid.Lock;
                    icon.Colour = new OsuColour().Cyan;
                    break;

                case Steps.Ban:
                    name = @$"标记禁图·{teamPrompt}";
                    description = @"被禁止的图无法被选与设置陷阱。";
                    icon.Icon = FontAwesome.Solid.Ban;
                    icon.Colour = Color4.Orange;
                    break;

                case Steps.Trap:
                    name = @$"设置陷阱·{teamPrompt}";
                    description = @"请队长*私信*告知裁判，禁止外泄。";
                    icon.Icon = FontAwesome.Solid.ExclamationCircle;
                    icon.Colour = new OsuColour().Pink1;
                    break;

                case Steps.Pick:
                    name = @$"标记选图·{teamPrompt}";
                    description = @"选择该轮要游玩的图。";
                    icon.Icon = FontAwesome.Solid.Check;
                    icon.Colour = new OsuColour().Green;
                    break;

                case Steps.Win:
                    name = @$"胜方染色·{teamPrompt}";
                    description = @"此图所在格将染成获胜队颜色。";
                    icon.Icon = FontAwesome.Solid.Trophy;
                    icon.Colour = team == TeamColour.Red ? new OsuColour().Pink : (team == TeamColour.Blue ? new OsuColour().Sky : new OsuColour().Yellow);
                    break;

                case Steps.EX:
                    name = @"即将进入 EX 模式";
                    description = @"当前棋盘不足以任一方取胜，需要重新染色。";
                    icon.Icon = FontAwesome.Solid.Bolt;
                    icon.Colour = Color4.Orange;
                    break;

                case Steps.FinalWin:
                    name = @$"{teamPrompt}获胜！";
                    description = @$"恭喜{teamPrompt}获得最终胜利！";
                    icon.Icon = FontAwesome.Solid.Medal;
                    icon.Colour = team == TeamColour.Red ? new OsuColour().Pink : (team == TeamColour.Blue ? new OsuColour().Sky : new OsuColour().Yellow);
                    break;

                default:
                    name = @"欢迎来到 OFFC";
                    description = @"祝游玩愉快！";
                    break;
            };
        }

        public LocalisableString Name => name;
        public LocalisableString Description => description;
        public IconUsage Icon => icon.Icon;
        public ColourInfo IconColor => icon.Colour;

        /// <summary>
        /// Get the original trap type based on a string.
        /// The string should exactly match the name of the trap.
        /// </summary>
        /// <param name="typeString">A <see cref="LocalisableString"/>, the string to handle</param>
        /// <returns>A <see cref="TrapType"/>, representing the trap type</returns>
        public Steps GetReversedType(LocalisableString typeString)
        {
            switch (typeString.ToString())
            {
                case @"标记保图":
                    return Steps.Protect;

                case @"标记禁图":
                    return Steps.Ban;

                case @"设置陷阱":
                    return Steps.Trap;

                case @"标记选图":
                    return Steps.Pick;

                case @"胜方染色":
                    return Steps.Win;

                case @"即将进入 EX 模式":
                    return Steps.EX;

                default:
                    return Steps.Default;
            }
        }
    }

    /// <summary>
    /// Lists out all available trap types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Steps
    {
        /// <summary>
        /// Mark maps unable to be set a trap on and banned.
        /// </summary>
        Protect,
        /// <summary>
        /// Mark maps unable to be chosen and set a trap on.
        /// </summary>
        Ban,
        /// <summary>
        /// Add traps.
        /// </summary>
        Trap,
        /// <summary>
        /// Pick maps.
        /// </summary>
        Pick,
        /// <summary>
        /// Mark colours.
        /// </summary>
        Win,
        /// <summary>
        /// The EX stage.
        /// </summary>
        EX,
        /// <summary>
        /// The winner is decided.
        /// </summary>
        FinalWin,
        /// <summary>
        /// Placeholder for default conditions.
        /// </summary>
        Default,
    }
}
