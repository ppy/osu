// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A Checkbox styled to be placed in line with an <see cref="OsuTabControl{T}"/>
    /// </summary>
    public partial class OsuTabControlCheckbox : Checkbox
    {
        private readonly Box box;
        private readonly SpriteText text;

        private Color4? accentColour;

        public Color4 AccentColour
        {
            get => accentColour.GetValueOrDefault();
            set
            {
                accentColour = value;

                updateFade();
            }
        }

        public LocalisableString Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        private const float transition_length = 500;
        private Sample sampleChecked;
        private Sample sampleUnchecked;
        private readonly SpriteIcon icon;

        public OsuTabControlCheckbox()
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Top = 5, Bottom = 5, },
                    Spacing = new Vector2(5f, 0f),
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText { Font = OsuFont.GetFont(size: 14) },
                        icon = new SpriteIcon
                        {
                            Size = new Vector2(14),
                            Icon = FontAwesome.Regular.Circle,
                            Shadow = true,
                        },
                    },
                },
                box = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 1,
                    Alpha = 0,
                    Colour = Color4.White,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, AudioManager audio)
        {
            if (accentColour == null)
                AccentColour = colours.Blue;

            sampleChecked = audio.Samples.Get(@"UI/check-on");
            sampleUnchecked = audio.Samples.Get(@"UI/check-off");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(selected =>
            {
                icon.Icon = selected.NewValue ? FontAwesome.Regular.CheckCircle : FontAwesome.Regular.Circle;
                text.Font = text.Font.With(weight: selected.NewValue ? FontWeight.Bold : FontWeight.Medium);

                updateFade();
            }, true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateFade();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!Current.Value)
                updateFade();

            base.OnHoverLost(e);
        }

        protected override void OnUserChange(bool value)
        {
            base.OnUserChange(value);

            if (value)
                sampleChecked?.Play();
            else
                sampleUnchecked?.Play();
        }

        private void updateFade()
        {
            box.FadeTo(Current.Value || IsHovered ? 1 : 0, transition_length, Easing.OutQuint);
            text.FadeColour(Current.Value || IsHovered ? Color4.White : AccentColour, transition_length, Easing.OutQuint);
        }
    }
}
