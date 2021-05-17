// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.News.Sidebar
{
    public class YearsPanel : CompositeDrawable
    {
        private readonly Bindable<APINewsSidebar> metadata = new Bindable<APINewsSidebar>();

        private FillFlowContainer yearsFlow;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, Bindable<APINewsSidebar> metadata)
        {
            this.metadata.BindTo(metadata);

            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
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
                    Child = yearsFlow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0, 5)
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            metadata.BindValueChanged(m =>
            {
                yearsFlow.Clear();

                if (m.NewValue == null)
                {
                    Hide();
                    return;
                }

                var currentYear = m.NewValue.CurrentYear;

                foreach (var y in m.NewValue.Years)
                    yearsFlow.Add(new YearButton(y, y == currentYear));

                Show();
            }, true);
        }

        private class YearButton : OsuHoverContainer
        {
            private readonly bool isCurrent;

            public YearButton(int year, bool isCurrent)
            {
                this.isCurrent = isCurrent;

                RelativeSizeAxes = Axes.X;
                Width = 0.25f;
                Height = 15;

                Child = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.GetFont(size: 12, weight: isCurrent ? FontWeight.SemiBold : FontWeight.Medium),
                    Text = year.ToString()
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                IdleColour = isCurrent ? Color4.White : colourProvider.Light2;
                HoverColour = isCurrent ? Color4.White : colourProvider.Light1;
                Action = () => { }; // Avoid button being disabled since there's no proper action assigned.
            }
        }
    }
}
