// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class ContextMenuItem : ClickableContainer
    {
        private const int height = 25;
        private const int width = 170;
        private const int transition_length = 200;

        private Color4 backgroundHoveredColour => OsuColour.FromHex(@"172023");
        private Color4 backgroundColour => OsuColour.FromHex(@"223034");

        protected Color4 TextColour { set { text.Colour = textBold.Colour = value; } }

        private readonly OsuSpriteText text;
        private readonly OsuSpriteText textBold;
        private readonly Box background;

        private SampleChannel sampleClick;
        private SampleChannel sampleHover;

        public ContextMenuItem(string title)
        {
            Width = width;
            Height = height;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = backgroundColour,
                },
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    TextSize = 17,
                    Text = title,
                    Margin = new MarginPadding{ Left = 20 },
                },
                textBold = new OsuSpriteText
                {
                    Alpha = 0,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    TextSize = 17,
                    Text = title,
                    Font = @"Exo2.0-Bold",
                    Margin = new MarginPadding{ Left = 20 },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Sample.Get(@"Menu/menuclick");
            sampleClick = audio.Sample.Get(@"Menu/menuback");
        }

        protected override bool OnHover(InputState state)
        {
            sampleHover.Play();
            background.Colour = backgroundHoveredColour;
            textBold.FadeIn(transition_length, EasingTypes.OutQuint);
            text.FadeOut(transition_length, EasingTypes.OutQuint);

            return true;
        }

        protected override void OnHoverLost(InputState state)
        {
            background.Colour = backgroundColour;
            textBold.FadeOut(transition_length, EasingTypes.OutQuint);
            text.FadeIn(transition_length, EasingTypes.OutQuint);
        }

        protected override bool OnClick(InputState state)
        {
            sampleClick?.Play();
            Action?.Invoke();
            return true;
        }
    }
}
