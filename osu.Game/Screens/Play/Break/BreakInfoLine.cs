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

namespace osu.Game.Screens.Play.Break
{
    public partial class BreakInfoLine<T> : Container
        where T : struct
    {
        private const int margin = 2;

        public Bindable<T> Current = new Bindable<T>();

        private readonly OsuSpriteText text;
        private readonly OsuSpriteText valueText;

        public BreakInfoLine(LocalisableString name)
        {
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreRight,
                    Text = name,
                    Font = OsuFont.GetFont(size: 17),
                    Margin = new MarginPadding { Right = margin }
                },
                valueText = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,
                    Text = @"-",
                    Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 17),
                    Margin = new MarginPadding { Left = margin }
                }
            };

            Current.ValueChanged += text => valueText.Text = Format(text.NewValue);
        }

        protected virtual LocalisableString Format(T count)
        {
            if (count is Enum countEnum)
                return countEnum.GetDescription();

            return count.ToString() ?? string.Empty;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            text.Colour = colours.Yellow;
            valueText.Colour = colours.YellowLight;
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
