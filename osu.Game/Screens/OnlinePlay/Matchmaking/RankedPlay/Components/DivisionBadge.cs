// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.RankedPlay;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components
{
    public partial class DivisionBadge : CompositeDrawable, IHasTooltip
    {
        private readonly RankedPlayDivision division;
        private readonly RulesetInfo ruleset;
        private readonly int? rank;

        public DivisionBadge(RankedPlayDivision division, RulesetInfo ruleset, int? rank = null)
        {
            if (division.Tier == Tier.Lustrous && rank == null)
                throw new ArgumentException("Must specify a rank for Lustrous tier.");

            this.division = division;
            this.ruleset = ruleset;
            this.rank = rank;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Size = division.Tier switch
            {
                Tier.Bronze => new Vector2(120, 96),
                Tier.Silver => new Vector2(120, 99.75f),
                Tier.Gold => new Vector2(120, 100.5f),
                Tier.Platinum => new Vector2(120, 107.25f),
                Tier.Rhodium => new Vector2(120, 108),
                Tier.Radiant => new Vector2(120, 112.5f),
                Tier.Lustrous => new Vector2(120, 122.5f),
                _ => throw new ArgumentOutOfRangeException()
            };

            InternalChildren = new Drawable[]
            {
                new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = textures.Get($@"Online/RankedPlay/Tiers/Base/{division.Tier}"),
                    FillMode = FillMode.Fit,
                },
                new Sprite
                {
                    Size = division.Tier switch
                    {
                        Tier.Bronze => new Vector2(33),
                        Tier.Silver => new Vector2(34.5f),
                        _ => new Vector2(36),
                    },
                    Position = division.Tier switch
                    {
                        Tier.Bronze => new Vector2(43.5f, 24.75f),
                        Tier.Silver => new Vector2(42.75f, 27),
                        Tier.Gold => new Vector2(42, 23.25f),
                        Tier.Platinum => new Vector2(42, 28.5f),
                        Tier.Rhodium => new Vector2(42, 30),
                        Tier.Radiant => new Vector2(42, 33),
                        Tier.Lustrous => new Vector2(42, 43.5f),
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    Texture = textures.Get($@"Online/RankedPlay/Tiers/Rulesets/{ruleset.ShortName}"),
                },
                new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = textures.Get($@"Online/RankedPlay/Tiers/Stars/{division.Key}"),
                    FillMode = FillMode.Fit,
                },
                new CompactDivisionBadge(division, rank)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                },
            };
        }

        public LocalisableString TooltipText => division.DisplayName;
    }
}
