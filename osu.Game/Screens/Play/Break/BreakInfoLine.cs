// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.Play.Break
{
    public partial class BreakInfoLine<T> : Container
        where T : struct
    {
        private const int spacing = 4;

        public Bindable<T> Current = new Bindable<T>();

        private readonly LocalisableString name;

        private OsuSpriteText valueText = null!;

        public BreakInfoLine(LocalisableString name)
        {
            this.name = name;

            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(spacing, 0),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = name,
                        Font = OsuFont.GetFont(size: 17),
                        Colour = colours.Yellow,
                    },
                    valueText = new OsuSpriteText
                    {
                        Text = @"-",
                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 17),
                        Colour = colours.YellowLight,
                    }
                }
            };

            Current.BindValueChanged(text => valueText.Text = Format(text.NewValue));
        }

        protected virtual LocalisableString Format(T count)
        {
            if (count is Enum countEnum)
                return countEnum.GetDescription();

            return count.ToString() ?? string.Empty;
        }
    }

    public partial class PercentageBreakInfoLine : BreakInfoLine<double>
    {
        public PercentageBreakInfoLine(LocalisableString name)
            : base(name)
        {
        }

        protected override LocalisableString Format(double count) => count.FormatAccuracy();
    }
}
