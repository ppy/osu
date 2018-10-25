using osu.Core.Containers.Shawdooow;
using osu.Core.Screens.Evast;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Framework.Timing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Play;
using osu.Mods.Evast.Visualizers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Mods.MapMixer
{
    public class MapMixer : BeatmapScreen
    {
        private SettingsSlider<double> clockPitch;
        private SettingsSlider<double> clockSpeed;
        private double pitch = 1;
        private double speed = 1;
        public static BindableDouble ClockPitch;
        public static BindableDouble ClockSpeed;
        private Bindable<double> dimLevel;
        private static readonly Bindable<bool> sync_pitch = new Bindable<bool> { Default = true, Value = true };
        private static readonly Bindable<bool> classic_sounds = new Bindable<bool> { Default = false, Value = false };
        protected override float BackgroundBlur => 10;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager config)
        {
            dimLevel = config.GetBindable<double>(OsuSetting.DimLevel);

            ClockPitch = new BindableDouble() { MinValue = 0f, Default = 1, Value = pitch, MaxValue = 2 };
            ClockSpeed = new BindableDouble() { MinValue = 0f, Default = 1, Value = speed, MaxValue = 2 };

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    Position = new Vector2(0 , 25),
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,

                    Size = new Vector2(0.3f, 0.16f),

                    Children = new Drawable[]
                    {
                        new SettingsCheckbox
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Bindable = sync_pitch,
                            LabelText = "Sync Pitch"
                        },
                        new SettingsCheckbox
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Bindable = classic_sounds,
                            LabelText = "Classic"
                        }
                    }
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
                        clockPitch = new SettingsSlider<double>
                        {
                            Origin = Anchor.BottomCentre,
                            Anchor = Anchor.Centre,
                            LabelText = "Pitch",
                            Bindable = ClockPitch,
                            KeyboardStep = 0.05f,
                        },
                        clockSpeed = new SettingsSlider<double>
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.Centre,
                            LabelText = "Clock Speed",
                            Bindable = ClockSpeed,
                            KeyboardStep = 0.05f
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
                    Action = () => Push(new Player()),
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
                    Action = () => Push(new Visualizer()),
                    Position = new Vector2(60 , -100),
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
                    //AlternateBindable = classic_sounds,
                    //AlternateAudioManager = ClassicRuleset.ClassicAudio
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
                    Bind = Key.N
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
                    Bind = Key.M
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
                    Bind = Key.B
                },
            };
        }

        protected override void Update()
        {
            base.Update();

            if (sync_pitch.Value)
            {
                ClockSpeed.Value = ClockPitch.Value;
                clockSpeed.Bindable.Value = clockPitch.Bindable.Value;
            }

            applyRateAdjustments();
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            setClockSpeed(Beatmap.Value.Track);
        }

        private void changeClockSpeeds(float value)
        {
            ClockPitch.Value = value;
            ClockSpeed.Value = value;
        }

        private void setClockSpeed(IAdjustableClock clock)
        {
            if (clock is IHasPitchAdjust pitchAdjust)
                clockPitch.Bindable.Value = pitchAdjust.PitchAdjust;
            clockSpeed.Bindable.Value = clock.Rate;
        }

        private void applyRateAdjustments()
        {
            if (Beatmap.Value.Track != null)
                applyToClock(Beatmap.Value.Track);
        }

        private void applyToClock(IAdjustableClock clock)
        {
            if (clock is IHasPitchAdjust pitchAdjust)
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

                    clock.Rate = clockSpeed.Bindable.Value;

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
        private int measure;
        private float measureLength = 1;
        private float lastMeasureTime = 1;

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
            if (lastMeasureTime <= (float)(Beatmap.Value.Track.CurrentTime - measureLength * 0.9f) || lastMeasureTime > (float)Beatmap.Value.Track.CurrentTime)
                lastMeasureTime = (float)Beatmap.Value.Track.CurrentTime;
            lastBeatTime = (float)Beatmap.Value.Track.CurrentTime;
            if(MapMixer.ClockPitch.Value > 0)
                measure++;
            if (MapMixer.ClockPitch.Value < 0)
                measure--;
            if (measure > 4)
                measure = 1;
            if (measure < 1)
                measure = 4;
        }

        protected override void Update()
        {
            base.Update();

            if (Beatmap.Value.Track.IsRunning)
                updateSeekBarPosition();
        }

        private void updateSeekBarPosition()
        {
            measure = (int)((((float)Beatmap.Value.Track.CurrentTime - lastMeasureTime) / measureLength) * 4);
            float minX = (measure) * 150;
            
            Vector2 position = new Vector2((((((float)Beatmap.Value.Track.CurrentTime - lastBeatTime) / beatLength) * 150) + 300), 0);
            
            position.X %= 150;
            position.X += minX - 300;

            seekBar.Position = position;
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
