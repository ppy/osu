// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Overlays;

namespace osu.Game.Screens.SelectV2.Wedge
{
    public abstract partial class DifficultyNameContent : CompositeDrawable
    {
        protected OsuSpriteText DifficultyName = null!;
        private OsuSpriteText mappedByLabel = null!;
        protected OsuHoverContainer MapperLink = null!;
        protected OsuSpriteText MapperName = null!;

        protected DifficultyNameContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    DifficultyName = new TruncatingSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                    },
                    mappedByLabel = new OsuSpriteText
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        // TODO: better null display? beatmap carousel panels also just show this text currently.
                        Text = " mapped by ",
                        Font = OsuFont.GetFont(size: 14),
                    },
                    // This is not a `LinkFlowContainer` as there are single-frame layout issues when Update()
                    // is being used for layout, see https://github.com/ppy/osu-framework/issues/3369.
                    MapperLink = new MapperLinkContainer
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        AutoSizeAxes = Axes.Both,
                        Child = MapperName = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 14),
                        }
                    },
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            // truncate difficulty name when width exceeds bounds, prioritizing mapper name display
            DifficultyName.MaxWidth = Math.Max(DrawWidth - mappedByLabel.DrawWidth
                                                         - MapperName.DrawWidth, 0);
        }

        private partial class MapperLinkContainer : OsuHoverContainer
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider? overlayColourProvider, OsuColour colours)
            {
                TooltipText = ContextMenuStrings.ViewProfile;
                IdleColour = overlayColourProvider?.Light2 ?? colours.Blue;
            }
        }
    }
}
