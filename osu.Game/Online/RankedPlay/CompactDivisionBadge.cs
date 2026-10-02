// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Online.RankedPlay
{
    public partial class CompactDivisionBadge : CompositeDrawable, IHasTooltip
    {
        private readonly RankedPlayDivision division;
        private readonly int? rank;

        public CompactDivisionBadge(RankedPlayDivision division, int? rank = null)
        {
            if (division.Tier == Tier.Lustrous && rank == null)
                throw new ArgumentException("Must specify a rank for Lustrous tier.");

            this.division = division;
            this.rank = rank;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Size = new Vector2(36, 19.5f);

            InternalChild = new Sprite
            {
                RelativeSizeAxes = Axes.Both,
                Texture = textures.Get(@$"Online/RankedPlay/Tiers/Compact/{division.Tier}"),
            };

            if (division.Tier != Tier.Lustrous)
            {
                AddInternal(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(4),
                    ChildrenEnumerable = Enumerable.Range(1, (int)division.Division)
                                                   .Select(_ => new Container
                                                   {
                                                       Size = new Vector2(3, 10),
                                                       Masking = true,
                                                       CornerRadius = 1.5f,
                                                       Child = new Box
                                                       {
                                                           RelativeSizeAxes = Axes.Both,
                                                           Colour = Color4.Black.Opacity(0.75f),
                                                       },
                                                   }),
                });
            }
            else if (rank != null)
            {
                AddInternal(new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Torus.With(size: 18, weight: FontWeight.Bold),
                    Colour = Colour4.Black.Opacity(0.75f),
                    Text = rank.ToLocalisableString(@"N0"),
                });
            }
        }

        public LocalisableString TooltipText => division.DisplayName;
    }
}
