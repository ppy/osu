// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osuTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarMusicButton : ToolbarOverlayToggleButton
    {
        private Circle volumeBar;

        protected override Anchor TooltipAnchor => Anchor.TopRight;

        public ToolbarMusicButton()
        {
            Hotkey = GlobalAction.ToggleNowPlaying;
            AutoSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader(true)]
        private void load(NowPlayingOverlay music)
        {
            StateContainer = music;

            Flow.Padding = new MarginPadding { Horizontal = Toolbar.HEIGHT / 4 };
            Flow.Add(new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Width = 3f,
                Height = IconContainer.Height,
                Margin = new MarginPadding { Horizontal = 2.5f },
                Masking = true,
                Children = new[]
                {
                    new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White.Opacity(0.25f),
                    },
                    volumeBar = new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0f,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Colour = Color4.White,
                    }
                }
            });
        }

        [Resolved]
        private AudioManager audio { get; set; }

        [Resolved(canBeNull: true)]
        private VolumeOverlay volume { get; set; }

        private IBindable<double> globalVolume;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            globalVolume = audio.Volume.GetBoundCopy();
            globalVolume.BindValueChanged(v => volumeBar.Height = (float)v.NewValue, true);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            volume?.Adjust(GlobalAction.IncreaseVolume, e.ScrollDelta.Y, e.IsPrecise);
            return true;
        }
    }
}
