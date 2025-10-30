// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Volume;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    [Cached]
    public partial class VolumeOverlay : VisibilityContainer
    {
        public Bindable<bool> IsMuted { get; } = new Bindable<bool>();
        public Bindable<bool> IsEffectMuted { get; } = new Bindable<bool>();
        public Bindable<bool> IsMusicMuted { get; } = new Bindable<bool>();

        private const float offset = 10;

        private VolumeMeterWithMute volumeMeterMaster = null!;
        private VolumeMeterWithMute volumeMeterEffect = null!;
        private VolumeMeterWithMute volumeMeterMusic = null!;

        private SelectionCycleFillFlowContainer<VolumeMeterWithMute> volumeMeters = null!;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuColour colours)
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 300,
                    Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.75f), Color4.Black.Opacity(0))
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Spacing = new Vector2(0, offset),
                    Margin = new MarginPadding { Left = offset },
                    Children = new Drawable[]
                    {
                        volumeMeters = new SelectionCycleFillFlowContainer<VolumeMeterWithMute>
                        {
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Spacing = new Vector2(0, offset),
                            Children = new[]
                            {
                                volumeMeterEffect = new VolumeMeterWithMute("EFFECTS", 125, colours.BlueDarker, MuteMode.Effects) { IsMuted = { BindTarget = IsEffectMuted }, },
                                volumeMeterMaster = new VolumeMeterWithMute("MASTER", 150, colours.PinkDarker, MuteMode.Master) { IsMuted = { BindTarget = IsMuted }, },
                                volumeMeterMusic = new VolumeMeterWithMute("MUSIC", 125, colours.BlueDarker, MuteMode.Music) { IsMuted = { BindTarget = IsMusicMuted }, },
                            }
                        },
                    },
                },
            });

            volumeMeterMaster.Bindable.BindTo(audio.Volume);
            volumeMeterEffect.Bindable.BindTo(audio.VolumeSample);
            volumeMeterMusic.Bindable.BindTo(audio.VolumeTrack);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            foreach (var volumeMeter in volumeMeters)
                volumeMeter.Bindable.ValueChanged += _ => Show();
        }

        public bool Adjust(GlobalAction action, float amount = 1, bool isPrecise = false)
        {
            if (!IsLoaded) return false;

            switch (action)
            {
                case GlobalAction.DecreaseVolume:
                    if (State.Value == Visibility.Hidden)
                        Show();
                    else
                        volumeMeters.Selected?.Decrease(amount, isPrecise);
                    return true;

                case GlobalAction.IncreaseVolume:
                    if (State.Value == Visibility.Hidden)
                        Show();
                    else
                        volumeMeters.Selected?.Increase(amount, isPrecise);
                    return true;

                case GlobalAction.NextVolumeMeter:
                    if (State.Value != Visibility.Visible)
                        return false;

                    volumeMeters.SelectNext();
                    Show();
                    return true;

                case GlobalAction.PreviousVolumeMeter:
                    if (State.Value != Visibility.Visible)
                        return false;

                    volumeMeters.SelectPrevious();
                    Show();
                    return true;

                case GlobalAction.ToggleMute:
                    Show();
                    volumeMeterMaster.ToggleMute();
                    return true;

                case GlobalAction.ToggleEffectsMute:
                    Show();
                    volumeMeterEffect.ToggleMute();
                    return true;

                case GlobalAction.ToggleMusicMute:
                    Show();
                    volumeMeterMusic.ToggleMute();
                    return true;
            }

            return false;
        }

        public void FocusMasterVolume()
        {
            volumeMeters.Select(volumeMeterMaster);
        }

        public override void Show()
        {
            // Focus on the master meter as a default if previously hidden
            if (State.Value == Visibility.Hidden)
                FocusMasterVolume();

            if (State.Value == Visibility.Visible)
                schedulePopOut();

            base.Show();
        }

        protected override void PopIn()
        {
            ClearTransforms();
            this.FadeIn(100);

            schedulePopOut();
        }

        protected override void PopOut()
        {
            this.FadeOut(100);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            // keep the scheduled event correctly timed as long as we have movement.
            schedulePopOut();
            return base.OnMouseMove(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            schedulePopOut();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            schedulePopOut();
            base.OnHoverLost(e);
        }

        private ScheduledDelegate? popOutDelegate;

        private void schedulePopOut()
        {
            popOutDelegate?.Cancel();
            this.Delay(1000).Schedule(() =>
            {
                if (!IsHovered)
                    Hide();
            }, out popOutDelegate);
        }
    }
}
