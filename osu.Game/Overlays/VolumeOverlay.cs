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
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Volume;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class VolumeOverlay : VisibilityContainer
    {
        private const float offset = 10;

        private VolumeMeter volumeMeterMaster;
        private VolumeMeter volumeMeterEffect;
        private VolumeMeter volumeMeterMusic;
        private MuteButton muteButton;

        private readonly BindableDouble muteAdjustment = new BindableDouble();

        private readonly Bindable<bool> isMuted = new Bindable<bool>();
        public Bindable<bool> IsMuted => isMuted;

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
                        volumeMeterEffect = new VolumeMeter("EFFECTS", 125, colours.BlueDarker)
                        {
                            Margin = new MarginPadding { Top = 100 + MuteButton.HEIGHT } //to counter the mute button and re-center the volume meters
                        },
                        volumeMeterMaster = new VolumeMeter("MASTER", 150, colours.PinkDarker),
                        volumeMeterMusic = new VolumeMeter("MUSIC", 125, colours.BlueDarker),
                        muteButton = new MuteButton
                        {
                            Margin = new MarginPadding { Top = 100 },
                            Current = { BindTarget = isMuted }
                        }
                    }
                },
            });

            volumeMeterMaster.Bindable.BindTo(audio.Volume);
            volumeMeterEffect.Bindable.BindTo(audio.VolumeSample);
            volumeMeterMusic.Bindable.BindTo(audio.VolumeTrack);

            isMuted.BindValueChanged(muted =>
            {
                if (muted.NewValue)
                    audio.AddAdjustment(AdjustableProperty.Volume, muteAdjustment);
                else
                    audio.RemoveAdjustment(AdjustableProperty.Volume, muteAdjustment);
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            volumeMeterMaster.Bindable.ValueChanged += _ => Show();
            volumeMeterEffect.Bindable.ValueChanged += _ => Show();
            volumeMeterMusic.Bindable.ValueChanged += _ => Show();
            muteButton.Current.ValueChanged += _ => Show();
        }

        public bool Adjust(GlobalAction action, float amount = 1, bool isPrecise = false)
        {
            if (!IsLoaded) return false;

            switch (action)
            {
                case GlobalAction.DecreaseVolume:
                    if (State.Value == Visibility.Hidden)
                        Show();
                    else if (volumeMeterMusic.IsHovered)
                        volumeMeterMusic.Decrease(amount, isPrecise);
                    else if (volumeMeterEffect.IsHovered)
                        volumeMeterEffect.Decrease(amount, isPrecise);
                    else
                        volumeMeterMaster.Decrease(amount, isPrecise);
                    return true;

                case GlobalAction.IncreaseVolume:
                    if (State.Value == Visibility.Hidden)
                        Show();
                    else if (volumeMeterMusic.IsHovered)
                        volumeMeterMusic.Increase(amount, isPrecise);
                    else if (volumeMeterEffect.IsHovered)
                        volumeMeterEffect.Increase(amount, isPrecise);
                    else
                        volumeMeterMaster.Increase(amount, isPrecise);
                    return true;

                case GlobalAction.ToggleMute:
                    Show();
                    muteButton.Current.Value = !muteButton.Current.Value;
                    return true;
            }

            return false;
        }

        private ScheduledDelegate popOutDelegate;

        public override void Show()
        {
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
