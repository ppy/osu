// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;
using System.Linq;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Framework.Bindables;
using System.Collections.Specialized;

namespace osu.Game.Overlays.News.Sidebar
{
    public class YearsPanel : CompositeDrawable
    {
        public int[] Years
        {
            set
            {
                years.Clear();
                years.AddRange(value);
            }
        }

        private readonly BindableList<int> years = new BindableList<int>();

        private FillFlowContainer<YearButton> flow;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Width = 160;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 6;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(5),
                    Child = flow = new FillFlowContainer<YearButton>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(5)
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            years.BindCollectionChanged((u, v) =>
            {
                switch (v.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        flow.Children = years.Select(y => new YearButton(y)).ToArray();
                        break;
                }
            }, true);
        }

        private class YearButton : OsuHoverContainer
        {
            protected override IEnumerable<Drawable> EffectTargets => new[] { text };

            private readonly int year;
            private readonly OsuSpriteText text;

            public YearButton(int year)
            {
                this.year = year;

                Size = new Vector2(33.75f, 15);
                Child = text = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(size: 12),
                    Text = year.ToString()
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = colourProvider.Light2;
                HoverColour = colourProvider.Light1;
                Action = () => { }; // TODO
            }
        }
    }
}
