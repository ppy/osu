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
using osu.Game.Screens.Play;
using osu.Game.Screens.Backgrounds;
using osu.Game.Configuration;
using OpenTK.Input;

namespace osu.Game.Screens.Symcol.Screens
{
    public class SymcolMapMixer : OsuScreen
    {
        private SettingsSlider<double> clockPitch;
        private SettingsSlider<double> clockSpeed;
        private double pitch = 1;
        private double speed = 1;
        public static BindableDouble ClockPitch;
        public static BindableDouble ClockSpeed;
        private Bindable<double> dimLevel;
        private static Bindable<bool> syncPitch = new Bindable<bool> { Default = true, Value = true };
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);
        private bool suspended = false;
        private readonly Bindable<WorkingBeatmap> workingBeatmap = new Bindable<WorkingBeatmap>();

        private OsuScreen player;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager config, OsuGameBase game)
        {
            workingBeatmap.BindTo(game.Beatmap);

            dimLevel = config.GetBindable<double>(OsuSetting.DimLevel);

            ClockPitch = new BindableDouble() { MinValue = 0f, Default = 1, Value = pitch, MaxValue = 2 };
            ClockSpeed = new BindableDouble() { MinValue = 0f, Default = 1, Value = speed, MaxValue = 2 };

            Children = new Drawable[]
            {
                new Container
                {
                    Position = new Vector2(0 , -25),
                    Height = 50,
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Children = new Drawable[]
                    {
                        clockPitch = new SettingsSlider<double>
                        {
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.Centre,
                            LabelText = "Pitch",
                            Bindable = ClockPitch,
                        },
                        clockSpeed = new SettingsSlider<double>
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.Centre,
                            LabelText = "Clock Speed",
                            Bindable = ClockSpeed,
                        },
                        new SettingsCheckbox
                        {
                            Origin = Anchor.BottomLeft,
                            Anchor = Anchor.TopLeft,
                            Bindable = syncPitch
                        }
                    }
                },
                new SymcolButton
                {
                    ButtonName = "Play",
                    Depth = -2,
                    Origin = Anchor.BottomRight,
                    Anchor = Anchor.BottomRight,
                    ButtonColorTop = Color4.DarkBlue,
                    ButtonColorBottom = Color4.Blue,
                    ButtonSize = 40,
                    Action = play,
                    Position = new Vector2(-60 , -100),
                },
                new SymcolButton
                {
                    ButtonName = "BG",
                    Depth = -2,
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    ButtonColorTop = Color4.DarkBlue,
                    ButtonColorBottom = Color4.Blue,
                    ButtonSize = 40,
                    Action = evastBackground,
                    Position = new Vector2(60 , 100),
                },

                //Sounds Bar
                new MusicBar
                {
                    RelativeSizeAxes = Axes.X,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Position = new Vector2(0 , 180),
                },
                new HitSoundBoard
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                },

                //Pitch Settings
                new SymcolButton
                {
                    ButtonName = "1x",
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkGoldenrod,
                    ButtonColorBottom = Color4.Goldenrod,
                    ButtonSize = 50,
                    Action = () => changeClockSpeeds(1f),
                    Position = new Vector2(0 , 250),
                    Bind = Key.V
                },
                new SymcolButton
                {
                    ButtonName = "1.5x",
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkGoldenrod,
                    ButtonColorBottom = Color4.Goldenrod,
                    ButtonSize = 50,
                    Action = () => changeClockSpeeds(1.5f),
                    Position = new Vector2(200 , 250),
                    Bind = Key.B
                },
                new SymcolButton
                {
                    ButtonName = "0.75x",
                    Depth = -2,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    ButtonColorTop = Color4.DarkGoldenrod,
                    ButtonColorBottom = Color4.Goldenrod,
                    ButtonSize = 50,
                    Action = () => changeClockSpeeds(0.75f),
                    Position = new Vector2(-200, 250),
                    Bind = Key.C
                },
            };
        }

        private void play()
        {
            if (player != null) return;

            LoadComponentAsync(player = new PlayerLoader(new Player()), l => Push(player));
        }

        private void evastBackground()
        {
            
        }

        protected override void Update()
        {
            base.Update();

            if (syncPitch.Value)
            {
                ClockSpeed.Value = ClockPitch.Value;
                clockSpeed.Bindable.Value = clockPitch.Bindable.Value;
            }

            applyRateAdjustments();

            if(!suspended)
                changeBackground(Beatmap, 1, 0);
        }

        private void changeBackground(WorkingBeatmap beatmap, float alpha, double duration)
        {
            var backgroundModeBeatmap = Background as BackgroundScreenBeatmap;
            if (backgroundModeBeatmap != null)
            {
                backgroundModeBeatmap.Beatmap = beatmap;
                backgroundModeBeatmap.BlurTo(new Vector2(10), duration);
                backgroundModeBeatmap.FadeTo(alpha, duration);
            }
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            changeBackground(Beatmap, 1 , 0);
            setClockSpeed(workingBeatmap.Value.Track);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);
            suspended = false;
            changeBackground(Beatmap, 1, 1500);
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);
            suspended = true;
            changeBackground(Beatmap, 0, 1500);
        }

        private void changeClockSpeeds(float value)
        {
            ClockPitch.Value = value;
            ClockSpeed.Value = value;
        }

        private void setClockSpeed(IAdjustableClock clock)
        {
            var pitchAdjust = clock as IHasPitchAdjust;
            clockPitch.Bindable.Value = pitchAdjust.PitchAdjust;
            clockSpeed.Bindable.Value = clock.Rate;
        }

        private void applyRateAdjustments()
        {
            if (workingBeatmap.Value.Track == null) return;
            else
                ApplyToClock(workingBeatmap.Value.Track);
        }

        private void ApplyToClock(IAdjustableClock clock)
        {
            var pitchAdjust = clock as IHasPitchAdjust;
            if (pitchAdjust != null)
            {
                if (clockPitch.Bindable.Value > 1)
                {
                    pitchAdjust.PitchAdjust = clockPitch.Bindable.Value;
                    pitch = pitchAdjust.PitchAdjust;

                    if (clockSpeed.Bindable.Value > 1)
                        clock.Rate = (clockSpeed.Bindable.Value - ((clockPitch.Bindable.Value - 1) / 2)) - ((clockSpeed.Bindable.Value - 1) / 2);
                    else
                        clock.Rate = (clockSpeed.Bindable.Value - ((clockPitch.Bindable.Value - 1) / 2)) + ((clockSpeed.Bindable.Value - 1) * 0.5f);

                    speed = clock.Rate;
                }
                else if (clockPitch.Bindable.Value < 1)
                {
                    pitchAdjust.PitchAdjust = clockPitch.Bindable.Value;
                    pitch = pitchAdjust.PitchAdjust;

                    if (clockSpeed.Bindable.Value < 1)
                        clock.Rate = (clockSpeed.Bindable.Value + ((clockPitch.Bindable.Value - 1) * -2)) + ((clockSpeed.Bindable.Value - 1) * 0.5f);
                    else
                        clock.Rate = (clockSpeed.Bindable.Value + ((clockPitch.Bindable.Value - 1) * -2)) - ((clockSpeed.Bindable.Value - 1) / 2);

                    speed = clock.Rate;
                }
                else
                {
                    pitchAdjust.PitchAdjust = clockPitch.Bindable.Value;
                    pitch = pitchAdjust.PitchAdjust;

                    if (clockSpeed.Bindable.Value < 1)
                        clock.Rate = clockSpeed.Bindable.Value + ((clockSpeed.Bindable.Value - 1) * -0.5f);
                    else
                        clock.Rate = clockSpeed.Bindable.Value - ((clockSpeed.Bindable.Value - 1) / 2);

                    speed = clock.Rate;
                }
            }
                
            else
                clock.Rate = clockSpeed.Bindable.Value;
        }
    }

    internal class MusicBar : BeatSyncedContainer
    {
        private Box seekBar;
        private float beatLength = 1;
        private float lastBeatTime = 1;
        private int measure = 0;
        private float measureLength = 1;
        private float lastMeasureTime = 1;
        private readonly Bindable<WorkingBeatmap> workingBeatmap = new Bindable<WorkingBeatmap>();

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            workingBeatmap.BindTo(game.Beatmap);
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
                seekBar = new Box
                {
                    Position = new Vector2(-300 , 0),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(2 , 20),
                },
            };
        }

        protected override void OnNewBeat(int beatIndex, TimingControlPoint timingPoint, EffectControlPoint effectPoint, TrackAmplitudes amplitudes)
        {
            base.OnNewBeat(beatIndex, timingPoint, effectPoint, amplitudes);
            beatLength = (float)timingPoint.BeatLength;
            measureLength = (float)timingPoint.BeatLength * 4;
            if (lastMeasureTime <= (float)(workingBeatmap.Value.Track.CurrentTime - measureLength * 0.9f) || lastMeasureTime > (float)workingBeatmap.Value.Track.CurrentTime)
                lastMeasureTime = (float)workingBeatmap.Value.Track.CurrentTime;
            lastBeatTime = (float)workingBeatmap.Value.Track.CurrentTime;
            if(SymcolMapMixer.ClockPitch.Value > 0)
                measure++;
            if (SymcolMapMixer.ClockPitch.Value < 0)
                measure--;
            if (measure > 4)
                measure = 1;
            if (measure < 1)
                measure = 4;
        }

        protected override void Update()
        {
            base.Update();

            if (workingBeatmap.Value.Track.IsRunning)
                seekBarPosition();
        }

        private Vector2 seekBarPosition()
        {
            measure = (int)((((float)workingBeatmap.Value.Track.CurrentTime - lastMeasureTime) / measureLength) * 4);
            float minX = (measure) * 150;
            
            Vector2 position = new Vector2((((((float)workingBeatmap.Value.Track.CurrentTime - lastBeatTime) / beatLength) * 150) + 300), 0);
            
            position.X %= 150;
            position.X += minX - 300;

            seekBar.Position = position;
            return seekBar.Position;
        }

        private void halfBeat()
        {

        }

        private void quarterBeat()
        {

        }

        private void generateMeasure(float x)
        {

        }
    }



    internal class MusicBarTick : Container
    {
        private Box box;
        private Container glow;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Children = new Drawable[]
            {
                box = new Box
                {
                    Depth = -2,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                glow = new Container
                {
                    Alpha = 0.25f,
                    Depth = 0,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Radius = 4,
                    },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                },
            };
        }
        public void Activate(float beatLength , float flashIntensity)
        {
            glow.Alpha = 0.5f * flashIntensity;
            glow.FadeTo(0.25f, beatLength);
        }
    }
}
