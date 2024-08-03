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
    public class TrapInfo
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public TeamColour Team;

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        public TrapType Mode;

        public int BeatmapID;

        public bool IsTriggered = false;

        private SpriteIcon icon = new SpriteIcon
        {
            Icon = FontAwesome.Solid.QuestionCircle,
            Colour = Color4.White,
        };

        private LocalisableString name = string.Empty;
        private LocalisableString description = string.Empty;

        /// <summary>
        /// A constructor to set up an instance of <see cref="TrapInfo"/>.
        /// </summary>
        /// <param name="colour">The team which set the trap (if exists).</param>
        /// <param name="type">The trap type (see <see cref="TrapType"/> for available values).</param>
        /// <param name="mapID">The beatmap ID which the trap is set on.</param>
        public TrapInfo(TeamColour colour = TeamColour.Neutral, TrapType type = TrapType.Unknown, int mapID = 0)
        {
            Team = colour;
            Mode = type;
            BeatmapID = mapID;
            IsTriggered = false;

            switch (type)
            {
                case TrapType.Follow:
                    name = @"时空之门";
                    description = @"陷阱设置方获得额外两次选图机会。";
                    icon.Icon = FontAwesome.Solid.Clock;
                    icon.Colour = new OsuColour().Purple1;
                    break;

                case TrapType.Swap:
                    name = @"大陆漂移";
                    description = @"游玩结束后，此格子将与另一格子完全交换。";
                    icon.Icon = FontAwesome.Solid.ExchangeAlt;
                    icon.Colour = Color4.Orange;
                    break;

                case TrapType.Solo:
                    name = @"决一死战";
                    description = @"两队各派1人游玩，FM图必须选择Mod。";
                    icon.Icon = FontAwesome.Solid.UserCircle;
                    icon.Colour = new OsuColour().Pink1;
                    break;

                case TrapType.Unused:
                    name = @"陷阱无效";
                    description = @"布置方触发了陷阱，将不会生效。";
                    icon.Icon = FontAwesome.Solid.Check;
                    icon.Colour = Color4.White;
                    break;

                default:
                    name = @"Unknown Trap";
                    description = @"We don't know this one.";
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
        public TrapType GetReversedType(LocalisableString typeString)
        {
            switch (typeString.ToString())
            {
                case @"决一死战":
                    return TrapType.Solo;

                case @"大陆漂移":
                    return TrapType.Swap;

                case @"时空之门":
                    return TrapType.Follow;

                default:
                    return TrapType.Unknown;
            }
        }
    }

    /// <summary>
    /// Lists out all available trap types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TrapType
    {
        /// <summary>
        /// Gets extra chances to choose beatmaps.
        /// </summary>
        Follow,
        /// <summary>
        /// Swap the specified map block with another after the current gameplay comes to an end.
        /// </summary>
        Swap,
        /// <summary>
        /// Perform an 1v1 play, with each player having at least one mod on (including the one bundled with the map).
        /// </summary>
        Solo,
        /// <summary>
        /// The trap has no effect.
        /// </summary>
        Unused,
        /// <summary>
        /// Placeholder for unimplemented or empty traps.
        /// </summary>
        Unknown,
    }
}
