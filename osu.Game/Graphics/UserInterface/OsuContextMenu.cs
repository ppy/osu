// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuContextMenu<TItem> : OsuMenu<TItem>
        where TItem : OsuContextMenuItem
    {
        private const int fade_duration = 250;

        public OsuContextMenu()
        {
            CornerRadius = 5;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.1f),
                Radius = 4,
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            BackgroundColour = colours.ContextMenuGray;
        }

        protected override void AnimateOpen() => this.FadeIn(fade_duration, Easing.OutQuint);
        protected override void AnimateClose() => this.FadeOut(fade_duration, Easing.OutQuint);

        protected override MarginPadding ItemFlowContainerPadding => new MarginPadding { Vertical = DrawableOsuContextMenuItem.MARGIN_VERTICAL };

        protected override DrawableMenuItem CreateDrawableMenuItem(TItem item) => new DrawableOsuContextMenuItem(this, item);

        #region DrawableOsuContextMenuItem
        private class DrawableOsuContextMenuItem : DrawableMenuItem
        {
            private const int margin_horizontal = 17;
            private const int text_size = 17;
            private const int transition_length = 80;
            public const int MARGIN_VERTICAL = 4;

            private SampleChannel sampleClick;
            private SampleChannel sampleHover;

            private OsuSpriteText text;
            private OsuSpriteText textBold;

            public DrawableOsuContextMenuItem(Menu<TItem> menu, TItem item)
                : base(item)
            {
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
                switch (Item.Type)
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
                textBold.FadeIn(transition_length, Easing.OutQuint);
                text.FadeOut(transition_length, Easing.OutQuint);
                return base.OnHover(state);
            }

            protected override void OnHoverLost(InputState state)
            {
                textBold.FadeOut(transition_length, Easing.OutQuint);
                text.FadeIn(transition_length, Easing.OutQuint);
                base.OnHoverLost(state);
            }

            protected override bool OnClick(InputState state)
            {
                sampleClick.Play();
                return base.OnClick(state);
            }

            protected override Drawable CreateContent() => new Container
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
                        Text = Item.Text,
                        Margin = new MarginPadding { Horizontal = margin_horizontal, Vertical = MARGIN_VERTICAL },
                    },
                    textBold = new OsuSpriteText
                    {
                        AlwaysPresent = true,
                        Alpha = 0,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        TextSize = text_size,
                        Text = Item.Text,
                        Font = @"Exo2.0-Bold",
                        Margin = new MarginPadding { Horizontal = margin_horizontal, Vertical = MARGIN_VERTICAL },
                    }
                }
            };
        }
        #endregion
    }
}