using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using osu.Game.Screens;
using osu.Game.Graphics;
using osu.Framework.Input;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Graphics.Transformations;


namespace osu.Game.Overlays.Pause
{
    public class PauseOverlay : OverlayContainer
    {
        private bool paused = false;

        public event Action OnPause;
        public event Action OnPlay;
        public event Action OnRetry;
        public event Action OnQuit;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.6f,
                },
                new PauseButton
                {
                    Text = @"Resume",
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Position = new Vector2(0, -200),
                    Action = Play
                },
                new PauseButton
                {
                    Text = @"Retry",
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre
                },
                new PauseButton
                {
                    Text = @"Quit",
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Position = new Vector2(0, 200)
                }
            };
        }

        protected override void PopIn()
        {
            this.FadeTo(1, 100, EasingTypes.In);
        }

        protected override void PopOut()
        {
            this.FadeTo(0, 100, EasingTypes.In);
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Escape:
                    paused = !paused;
                    (paused ? (Action)Pause : Play)?.Invoke();
                    return true;
            }
            return base.OnKeyDown(state, args);
        }

        private void Pause()
        {
            paused = true;
            Show();
            OnPause?.Invoke();
        }

        private void Play()
        {
            paused = false;
            Hide();
            OnPlay?.Invoke();
        }

        private void Retry()
        {
            OnRetry?.Invoke();
        }

        private void Quit()
        {
            OnQuit?.Invoke();
        }

        public PauseOverlay()
        {
            RelativeSizeAxes = Axes.Both;
            AutoSizeAxes = Axes.Both;
        }
    }
}