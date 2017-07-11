// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuContextMenuItem : ContextMenuItem
    {
        private const int transition_length = 80;
        private const int margin_horizontal = 17;
        public const int MARGIN_VERTICAL = 4;
        private const int text_size = 17;

        private OsuSpriteText text;
        private OsuSpriteText textBold;

        private SampleChannel sampleClick;
        private SampleChannel sampleHover;

        private readonly MenuItemType type;

        protected override Container CreateTextContainer(string title) => new Container
        {
            AutoSizeAxes = Axes.Both,
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            Children = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    TextSize = text_size,
                    Text = title,
                    Margin = new MarginPadding { Horizontal = margin_horizontal, Vertical = MARGIN_VERTICAL },
                },
                textBold = new OsuSpriteText
                {
                    AlwaysPresent = true,
                    Alpha = 0,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    TextSize = text_size,
                    Text = title,
                    Font = @"Exo2.0-Bold",
                    Margin = new MarginPadding { Horizontal = margin_horizontal, Vertical = MARGIN_VERTICAL },
                }
            }
        };

        public OsuContextMenuItem(string title, MenuItemType type = MenuItemType.Standard) : base(title)
        {
            this.type = type;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            sampleHover = audio.Sample.Get(@"UI/generic-hover");
            sampleClick = audio.Sample.Get(@"UI/generic-click");

            BackgroundColour = Color4.Transparent;
            BackgroundColourHover = OsuColour.FromHex(@"172023");

            updateTextColour();
        }

        private void updateTextColour()
        {
            switch (type)
            {
                case MenuItemType.Standard:
                    textBold.Colour = text.Colour = Color4.White;
                    break;
                case MenuItemType.Destructive:
                    textBold.Colour = text.Colour = Color4.Red;
                    break;
                case MenuItemType.Highlighted:
                    textBold.Colour = text.Colour = OsuColour.FromHex(@"ffcc22");
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
            sampleClick.Play();
            return base.OnClick(state);
        }
    }
}