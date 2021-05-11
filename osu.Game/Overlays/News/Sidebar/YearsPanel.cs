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
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.News.Sidebar
{
    public class YearsPanel : CompositeDrawable
    {
        private readonly Bindable<APINewsSidebar> metadata = new Bindable<APINewsSidebar>();

        private FillFlowContainer<YearButton> flow;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, Bindable<APINewsSidebar> metadata)
        {
            this.metadata.BindTo(metadata);

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

            metadata.BindValueChanged(m =>
            {
                if (m.NewValue == null)
                {
                    Hide();
                    return;
                }

                flow.Children = m.NewValue.Years.Select(y => new YearButton(y)).ToArray();
                Show();
            }, true);
        }

        private class YearButton : OsuHoverContainer
        {
            protected override IEnumerable<Drawable> EffectTargets => new[] { text };

            private readonly OsuSpriteText text;

            public YearButton(int year)
            {
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
