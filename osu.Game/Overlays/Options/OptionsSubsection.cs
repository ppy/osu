// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Options
{
    public abstract class OptionsSubsection : FillFlowContainer
    {
        private Container<Drawable> content;
        protected override Container<Drawable> Content => content;

        protected abstract string Header { get; }

        public OptionsSubsection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Direction = FillDirection.Down;
            AddInternal(new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = Header.ToUpper(),
                    Margin = new MarginPadding { Bottom = 10 },
                    Font = @"Exo2.0-Black",
                },
                content = new FillFlowContainer
                {
                    Direction = FillDirection.Down,
                    Spacing = new Vector2(0, 5),
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
            });
        }
    }
}

