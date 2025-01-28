// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Input.Bindings;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Overlays.Toolbar
{
    public partial class ToolbarMusicButton : ToolbarOverlayToggleButton
    {
        private Box volumeBar;

        protected override Anchor TooltipAnchor => Anchor.TopRight;

        public ToolbarMusicButton()
        {
            Hotkey = GlobalAction.ToggleNowPlaying;
            ButtonContent.AutoSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader(true)]
        private void load(NowPlayingOverlay music)
        {
            StateContainer = music;

            Flow.Padding = new MarginPadding { Horizontal = Toolbar.HEIGHT / 4 };
            Flow.Add(volumeDisplay = new CircularContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Width = 3f,
                Height = IconContainer.Height,
                Margin = new MarginPadding { Horizontal = 2.5f },
                Masking = true,
                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.White.Opacity(0.25f),
                    },
                    volumeBar = new Box
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
        private Container volumeDisplay;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            globalVolume = audio.Volume.GetBoundCopy();
            globalVolume.BindValueChanged(v => volumeBar.ResizeHeightTo((float)v.NewValue, 200, Easing.OutQuint), true);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!IsHovered)
                return false;

            switch (e.Key)
            {
                case Key.Up:
                    focusForAdjustment();
                    volume?.Adjust(GlobalAction.IncreaseVolume);
                    return true;

                case Key.Down:
                    focusForAdjustment();
                    volume?.Adjust(GlobalAction.DecreaseVolume);
                    return true;
            }

            return base.OnKeyDown(e);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            focusForAdjustment();
            volume?.Adjust(GlobalAction.IncreaseVolume, e.ScrollDelta.Y, e.IsPrecise);
            return true;
        }

        private void focusForAdjustment()
        {
            volume?.FocusMasterVolume();
            expandVolumeBarTemporarily();
        }

        private TransformSequence<Container> expandTransform;
        private ScheduledDelegate contractTransform;

        private void expandVolumeBarTemporarily()
        {
            // avoid starting a new transform if one is already active.
            if (expandTransform == null)
            {
                expandTransform = volumeDisplay.ResizeWidthTo(6, 500, Easing.OutQuint);
                expandTransform.Finally(_ => expandTransform = null);
            }

            contractTransform?.Cancel();
            contractTransform = Scheduler.AddDelayed(() =>
            {
                volumeDisplay.ResizeWidthTo(3f, 500, Easing.OutQuint);
            }, 1000);
        }
    }
}
