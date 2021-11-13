// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Beatmaps.Drawables.Cards.Statistics
{
    /// <summary>
    /// A single statistic shown on a beatmap card.
    /// </summary>
    public abstract class BeatmapCardStatistic : CompositeDrawable, IHasTooltip, IHasCustomTooltip
    {
        protected IconUsage Icon
        {
            get => spriteIcon.Icon;
            set => spriteIcon.Icon = value;
        }

        protected LocalisableString Text
        {
            get => spriteText.Text;
            set => spriteText.Text = value;
        }

        public LocalisableString TooltipText { get; protected set; }

        private readonly SpriteIcon spriteIcon;
        private readonly OsuSpriteText spriteText;

        protected BeatmapCardStatistic()
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(5, 0),
                Children = new Drawable[]
                {
                    spriteIcon = new SpriteIcon
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Size = new Vector2(10),
                        Margin = new MarginPadding { Top = 1 }
                    },
                    spriteText = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Font = OsuFont.Default.With(size: 14)
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            spriteIcon.Colour = colourProvider.Content2;
        }

        #region Tooltip implementation

        public virtual ITooltip GetCustomTooltip() => null;
        public virtual object TooltipContent => null;

        #endregion
    }
}
