// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class ContextMenuItem : MenuItem
    {
        private const int transition_length = 200;
        private const int margin_left = 15;
        public const int MARGIN_VERTICAL = 5;

        private readonly OsuSpriteText text;
        private readonly OsuSpriteText textBold;

        private SampleChannel sampleClick;
        private SampleChannel sampleHover;

        private readonly ContextMenuType type;

        public ContextMenuItem(string title, ContextMenuType type = ContextMenuType.Standard)
        {
            this.type = type;

            Children = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    TextSize = 17,
                    Text = title,
                    Margin = new MarginPadding{ Left = margin_left, Vertical = MARGIN_VERTICAL },
                },
                textBold = new OsuSpriteText
                {
                    Alpha = 0,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    TextSize = 17,
                    Text = title,
                    Font = @"Exo2.0-Bold",
                    Margin = new MarginPadding{ Left = margin_left, Vertical = MARGIN_VERTICAL },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            sampleHover = audio.Sample.Get(@"Menu/menuclick");
            sampleClick = audio.Sample.Get(@"Menu/menuback");

            BackgroundColour = Color4.Transparent;
            BackgroundColourHover = OsuColour.FromHex(@"172023");

            switch (type)
            {
                case ContextMenuType.Highlighted:
                    text.Colour = textBold.Colour = colours.Yellow;
                    break;
                case ContextMenuType.Destructive:
                    text.Colour = textBold.Colour = Color4.Red;
                    break;
            }
        }

        protected override bool OnHover(InputState state)
        {
            sampleHover.Play();
            textBold.FadeIn(transition_length, EasingTypes.OutQuint);
            text.FadeOut(transition_length, EasingTypes.OutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            textBold.FadeOut(transition_length, EasingTypes.OutQuint);
            text.FadeIn(transition_length, EasingTypes.OutQuint);
            base.OnHoverLost(state);
        }

        protected override bool OnClick(InputState state)
        {
            sampleClick?.Play();
            return base.OnClick(state);
        }
    }
}
