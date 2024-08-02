// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets;
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

        private SpriteIcon icon = new SpriteIcon
        {
            Icon = FontAwesome.Solid.QuestionCircle,
            Colour = Color4.White,
        };

        private LocalisableString name;
        private LocalisableString description;

        public TrapInfo(TeamColour colour, TrapType type, int mapID)
        {
            Team = colour;
            Mode = type;
            BeatmapID = mapID;

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
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TrapType
    {
        Follow,
        Swap,
        Solo,
        Unknown,
    }
}
