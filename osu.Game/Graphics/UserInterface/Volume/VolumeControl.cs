// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
using OpenTK;
using osu.Framework.Audio;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Game.Input.Bindings;

namespace osu.Game.Graphics.UserInterface.Volume
{
    public class VolumeControl : OverlayContainer
    {
        private readonly VolumeMeter volumeMeterMaster;
        private readonly IconButton muteIcon;

        protected override bool BlockPassThroughMouse => false;

        public VolumeControl()
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.BottomRight;
            Origin = Anchor.BottomRight;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding { Left = 10, Right = 10, Top = 30, Bottom = 30 },
                    Spacing = new Vector2(15, 0),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Size = new Vector2(IconButton.BUTTON_SIZE),
                            Child = muteIcon = new IconButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Icon = FontAwesome.fa_volume_up,
                                Action = () => Adjust(GlobalAction.ToggleMute),
                            }
                        },
                        volumeMeterMaster = new VolumeMeter("Master"),
                        volumeMeterEffect = new VolumeMeter("Effects"),
                        volumeMeterMusic = new VolumeMeter("Music")
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            volumeMeterMaster.Bindable.ValueChanged += _ => settingChanged();
            volumeMeterEffect.Bindable.ValueChanged += _ => settingChanged();
            volumeMeterMusic.Bindable.ValueChanged += _ => settingChanged();
            muted.ValueChanged += _ => settingChanged();
        }

        public bool Adjust(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.DecreaseVolume:
                    if (State == Visibility.Hidden)
                        Show();
                    else
                        volumeMeterMaster.Decrease();
                    return true;
                case GlobalAction.IncreaseVolume:
                    if (State == Visibility.Hidden)
                        Show();
                    else
                        volumeMeterMaster.Increase();
                    return true;
                case GlobalAction.ToggleMute:
                    Show();
                    muted.Toggle();
                    return true;
            }

            return false;
        }

        private void settingChanged()
        {
            Show();
            schedulePopOut();
        }

        private readonly BindableDouble muteAdjustment = new BindableDouble();

        private readonly BindableBool muted = new BindableBool();

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            volumeMeterMaster.Bindable.BindTo(audio.Volume);
            volumeMeterEffect.Bindable.BindTo(audio.VolumeSample);
            volumeMeterMusic.Bindable.BindTo(audio.VolumeTrack);

            muted.ValueChanged += mute =>
            {
                if (mute)
                {
                    audio.AddAdjustment(AdjustableProperty.Volume, muteAdjustment);
                    muteIcon.Icon = FontAwesome.fa_volume_off;
                }
                else
                {
                    audio.RemoveAdjustment(AdjustableProperty.Volume, muteAdjustment);
                    muteIcon.Icon = FontAwesome.fa_volume_up;
                }
            };
        }

        private ScheduledDelegate popOutDelegate;

        private readonly VolumeMeter volumeMeterEffect;
        private readonly VolumeMeter volumeMeterMusic;

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

        private void schedulePopOut()
        {
            popOutDelegate?.Cancel();
            this.Delay(1000).Schedule(Hide, out popOutDelegate);
        }
    }
}
