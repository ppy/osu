using osu.Framework.Allocation;
using osu.Framework.Graphics;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Screens.Symcol.Pieces;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio;
using osu.Game.Overlays.Settings;
using osu.Framework.Configuration;
using osu.Framework.Timing;
using osu.Framework.Audio.Track;
using osu.Game.Database;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Framework.Screens;
using osu.Game.Screens.Menu;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Beatmaps.ControlPoints;
using System;
using osu.Game.Configuration;

namespace osu.Game.Screens.Symcol.Screens
{
    public class SymcolOffsetTicker : OsuScreen
    {
        private SampleChannel tick;
        private double savedTime = 0;
        private double savedTime2 = 0;
        private tickBar bar;
        private SettingsSlider<double> offset;

        protected override void OnResuming(Screen last)
        {
            Beatmap.Value.Track?.Stop();
            base.OnResuming(last);
        }
        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            Background.FadeColour(Color4.DarkGray, 500);
            Beatmap.Value.Track?.Stop();
        }
        protected override bool OnExiting(Screen next)
        {
            Background.FadeColour(Color4.White, 500);
            Beatmap.Value.Track?.Start();
            return base.OnExiting(next);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager config)
        {
            savedTime = Time.Current;
            savedTime2 = savedTime;
            tick = audio.Sample.Get($@"Gameplay/tick");
            Children = new Drawable[]
            {
                bar = new tickBar
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Container
                {
                    Position = new Vector2(0 , -25),
                    Height = 50,
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Children = new Drawable[]
                    {
                        offset = new SettingsSlider<double>
                        {
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            LabelText = "Offset",
                            Bindable = config.GetBindable<double>(OsuSetting.AudioOffset),
                        },
                    }
                },
            };
        }

        protected override void Update()
        {
            base.Update();
            if (Time.Current >= savedTime2 + 500 + offset.Bindable.Value && offset.Bindable.Value < 0)
            {
                savedTime2 = savedTime + 500;
                try
                {
                    //tick.Play();
                }
                catch
                {

                }
                bar.SeekBarPop.Position = bar.SeekBar.Position;
                bar.SeekBarPop.FadeOutFromOne(500);
                bar.SeekBarPop.Scale = new Vector2(1, 5);
                bar.SeekBarPop.ScaleTo(new Vector2(1), 500);
            }
            if (Time.Current >= savedTime2 + 500 + offset.Bindable.Value && offset.Bindable.Value >= 0)
            {
                savedTime2 = savedTime;
                try
                {
                    //tick.Play();
                }
                catch
                {

                }
                bar.SeekBarPop.Position = bar.SeekBar.Position;
                bar.SeekBarPop.FadeOutFromOne(500);
                bar.SeekBarPop.Scale = new Vector2(1, 5);
                bar.SeekBarPop.ScaleTo(new Vector2(1) , 500);
            }
            if (Time.Current >= savedTime + 500)
            {
                savedTime = savedTime + 500;
                bar.SavedTime = savedTime;
            }
        }
    }

    internal class tickBar : BeatSyncedContainer
    {
        public Box SeekBar;
        public Box SeekBarPop;
        private int measure = 0;
        public double SavedTime;

        private SampleChannel tick;
        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager config)
        {
            tick = audio.Sample.Get($@"Gameplay/tick");
        }
        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);
            tick.Play();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = Color4.White,
                    Size = new Vector2(600 , 4),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Box
                {
                    Position = new Vector2(300 , 0),
                    Colour = Color4.White,
                    Size = new Vector2(4 , 30),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Box
                {
                    Position = new Vector2(-300 , 0),
                    Colour = Color4.White,
                    Size = new Vector2(4 , 30),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Box
                {
                    Position = new Vector2(-300 / 2 , 0),
                    Colour = Color4.White,
                    Size = new Vector2(3.5f , 22),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Box
                {
                    Position = new Vector2(-0 , 0),
                    Colour = Color4.White,
                    Size = new Vector2(4 , 26),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                new Box
                {
                    Position = new Vector2(300 / 2 , 0),
                    Colour = Color4.White,
                    Size = new Vector2(3.5f , 22),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                SeekBar = new Box
                {
                    Position = new Vector2(-300 , 0),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(2 , 20),
                },
                SeekBarPop = new Box
                {
                    Position = new Vector2(0 , 0),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(1 , 30),
                    AlwaysPresent = true,
                },
            };
        }

        protected override void Update()
        {
            base.Update();
            seekBarPosition();
        }

        private Vector2 seekBarPosition()
        {
            float minX = (measure) * 150;

            Vector2 position = new Vector2((((((float)Time.Current - (float)SavedTime) / 500) * 150) + 300), 0);

            position.X %= 150;
            position.X += minX - 300;

            SeekBar.Position = position;
            return SeekBar.Position;
        }
    }
}
