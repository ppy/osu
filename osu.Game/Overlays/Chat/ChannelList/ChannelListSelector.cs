// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Chat.ChannelList
{
    public class ChannelListSelector : OsuClickableContainer
    {
        public readonly BindableBool SelectorActive = new BindableBool();

        private Box hoverBox = null!;
        private Box selectBox = null!;
        private OsuSpriteText text = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Height = 30;
            RelativeSizeAxes = Axes.X;

            Children = new Drawable[]
            {
                hoverBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3,
                    Alpha = 0f,
                },
                selectBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                    Alpha = 0f,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = 18, Right = 10 },
                    Child = text = new OsuSpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Text = "Add more channels",
                        Font = OsuFont.Torus.With(size: 17, weight: FontWeight.SemiBold),
                        Colour = colourProvider.Light3,
                        Margin = new MarginPadding { Bottom = 2 },
                        RelativeSizeAxes = Axes.X,
                        Truncate = true,
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectorActive.BindValueChanged(selector =>
            {
                if (selector.NewValue)
                {
                    text.FadeColour(colourProvider.Content1, 300, Easing.OutQuint);
                    selectBox.FadeIn(300, Easing.OutQuint);
                }
                else
                {
                    text.FadeColour(colourProvider.Light3, 200, Easing.OutQuint);
                    selectBox.FadeOut(200, Easing.OutQuint);
                }
            }, true);

            Action = () => SelectorActive.Value = true;
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverBox.FadeIn(300, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverBox.FadeOut(200, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
