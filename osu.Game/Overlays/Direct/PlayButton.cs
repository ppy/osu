// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Direct
{
    public class PlayButton : Container
    {
        private readonly Bindable<bool> playing;

        private Color4 hoverColour;
        private readonly SpriteIcon icon;
        private readonly LoadingAnimation loadingAnimation;
        private const float transition_duration = 500;

        private bool loading;
        public bool Loading
        {
            get { return loading; }
            set
            {
                loading = value;
                if (value)
                {
                    loadingAnimation.Show();
                    icon.FadeOut(transition_duration * 5, Easing.OutQuint);
                }
                else
                {
                    loadingAnimation.Hide();
                    icon.FadeIn(transition_duration, Easing.OutQuint);
                }
            }
        }

        public PlayButton(Bindable<bool> playing)
        {
            this.playing = playing;
            AddRange(new Drawable[]
            {
                icon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fit,
                    RelativeSizeAxes = Axes.Both,
                    Icon = FontAwesome.fa_play,
                },
                loadingAnimation = new LoadingAnimation(),
            });

            playing.ValueChanged += newValue => icon.Icon = newValue ? FontAwesome.fa_pause : FontAwesome.fa_play;

            playing.ValueChanged += newValue => icon.FadeColour(newValue || IsHovered ? hoverColour : Color4.White, 120, Easing.InOutQuint);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            hoverColour = colour.Yellow;
        }

        protected override bool OnClick(InputState state)
        {
            playing.Value = !playing.Value;
            return true;
        }

        protected override bool OnHover(InputState state)
        {
            icon.FadeColour(hoverColour, 120, Easing.InOutQuint);
            return base.OnHover(state);
        }

        protected override void OnHoverLost(InputState state)
        {
            if(!playing.Value)
                icon.FadeColour(Color4.White, 120, Easing.InOutQuint);
            base.OnHoverLost(state);
        }
    }
}
