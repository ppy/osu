// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using ManagedBass.Fx;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Mixing;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osuTK;

namespace osu.Game.Tests.Visual.Audio
{
    public class TestSceneMixerCompressor : OsuTestScene
    {
        private AudioMixer globalMixer;
        private AudioMixer sampleMixer;

        private readonly CompressorParameters globalCompParams;
        private readonly BindableBool globalEnabled;
        private readonly BindableFloat globalAttack;
        private readonly BindableFloat globalRelease;
        private readonly BindableFloat globalThreshold;
        private readonly BindableFloat globalGain;
        private readonly BindableFloat globalRatio;

        private readonly CompressorParameters sampleCompParams;
        private readonly BindableBool sampleEnabled;
        private readonly BindableFloat sampleAttack;
        private readonly BindableFloat sampleRelease;
        private readonly BindableFloat sampleThreshold;
        private readonly BindableFloat sampleGain;
        private readonly BindableFloat sampleRatio;

        private Track track;
        private bool trackPlaying;
        private readonly TriangleButton playingButton;
        private readonly TriangleButton spammingButton;

        private WaveformTestBeatmap beatmap;
        private Sample spamSample1;
        private Sample spamSample2;
        private Sample spamSample3;

        private double spamSample1LastPlayed;
        private double spamSample2LastPlayed;

        private bool spamming;

        private readonly float[,,] presets =
        {
            {
                { 0, 10, 200, -15, 0, 3 },
                { 0, 10, 200, -15, 0, 3 },
            },
            {
                { 1, 100, 500, -3, 0, 1000 },
                { 1, 50, 300, -7, 0, 1000 },
            },
            {
                { 1, 50, 300, -3, 0, 1000 },
                { 1, 50, 300, -7, 0, 1000 },
            },
            {
                { 1, 1.2f, 400, -3, 0, 100 },
                { 1, 1.2f, 400, -7, 0, 100 },
            },
        };

        public TestSceneMixerCompressor()
        {
            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Width = 300,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "GlobalMixer",
                                Padding = new MarginPadding(10),
                                Font = OsuFont.GetFont(size: 24)
                            },
                            new SettingsCheckbox
                            {
                                LabelText = "enabled",
                                Current = { BindTarget = globalEnabled = new BindableBool() }
                            },
                            new SettingsSlider<float>
                            {
                                LabelText = "attack",
                                Padding = new MarginPadding(20),
                                Current =
                                {
                                    BindTarget = globalAttack = new BindableFloat(10f)
                                    {
                                        MinValue = 0.01f,
                                        MaxValue = 1000f,
                                        Precision = 0.01f
                                    }
                                }
                            },
                            new SettingsSlider<float>
                            {
                                LabelText = "release",
                                Padding = new MarginPadding(20),
                                Current =
                                {
                                    BindTarget = globalRelease = new BindableFloat(200f)
                                    {
                                        MinValue = 0.01f,
                                        MaxValue = 5000f,
                                        Precision = 0.01f
                                    }
                                }
                            },
                            new SettingsSlider<float>
                            {
                                LabelText = "threshold",
                                Padding = new MarginPadding(20),
                                Current =
                                {
                                    BindTarget = globalThreshold = new BindableFloat(-15f)
                                    {
                                        MinValue = -60f,
                                        MaxValue = 0f
                                    }
                                }
                            },
                            new SettingsSlider<float>
                            {
                                LabelText = "gain",
                                Padding = new MarginPadding(20),
                                Current =
                                {
                                    BindTarget = globalGain = new BindableFloat
                                    {
                                        MinValue = -60f,
                                        MaxValue = 10f, // max is actually 60f, but allowing that makes it easy to accidentally deafen yourself
                                    }
                                }
                            },
                            new SettingsSlider<float>
                            {
                                LabelText = "ratio",
                                Padding = new MarginPadding(20),
                                Current =
                                {
                                    BindTarget = globalRatio = new BindableFloat(3f)
                                    {
                                        MinValue = 1f,
                                        MaxValue = 1000f
                                    }
                                }
                            },
                        },
                    },
                    new FillFlowContainer
                    {
                        Width = 300,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = "SampleMixer",
                                Padding = new MarginPadding(10),
                                Font = OsuFont.GetFont(size: 24)
                            },
                            new SettingsCheckbox
                            {
                                LabelText = "enabled",
                                Current = { BindTarget = sampleEnabled = new BindableBool() }
                            },
                            new SettingsSlider<float>
                            {
                                LabelText = "attack",
                                Padding = new MarginPadding(20),
                                Current =
                                {
                                    BindTarget = sampleAttack = new BindableFloat(10f)
                                    {
                                        MinValue = 0.01f,
                                        MaxValue = 1000f,
                                        Precision = 0.01f
                                    }
                                }
                            },
                            new SettingsSlider<float>
                            {
                                LabelText = "release",
                                Padding = new MarginPadding(20),
                                Current =
                                {
                                    BindTarget = sampleRelease = new BindableFloat(200f)
                                    {
                                        MinValue = 0.01f,
                                        MaxValue = 5000f,
                                        Precision = 0.01f
                                    }
                                }
                            },
                            new SettingsSlider<float>
                            {
                                LabelText = "threshold",
                                Padding = new MarginPadding(20),
                                Current =
                                {
                                    BindTarget = sampleThreshold = new BindableFloat(-15f)
                                    {
                                        MinValue = -60f,
                                        MaxValue = 0f
                                    }
                                }
                            },
                            new SettingsSlider<float>
                            {
                                LabelText = "gain",
                                Padding = new MarginPadding(20),
                                Current =
                                {
                                    BindTarget = sampleGain = new BindableFloat
                                    {
                                        MinValue = -60f,
                                        MaxValue = 10f, // max is actually 60f, but allowing that makes it easy to accidentally deafen yourself
                                    }
                                }
                            },
                            new SettingsSlider<float>
                            {
                                LabelText = "ratio",
                                Padding = new MarginPadding(20),
                                Current =
                                {
                                    BindTarget = sampleRatio = new BindableFloat(3f)
                                    {
                                        MinValue = 1f,
                                        MaxValue = 1000f
                                    }
                                }
                            },
                        }
                    },
                    new FillFlowContainer
                    {
                        Padding = new MarginPadding(20),
                        Spacing = new Vector2(10),
                        Width = 300,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            playingButton = new TriangleButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 1,
                                Text = "Start Track",
                                Action = toggleTrack,
                            },
                            spammingButton = new TriangleButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 1,
                                Text = "Start Sample Spam",
                                Action = toggleSampleSpam,
                            },
                            new TriangleButton
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 1,
                                Text = "Play Select Sample",
                                Action = () => spamSample3?.Play(),
                            },
                            new OsuSpriteText
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 1f,
                                Text = "Presets",
                                Padding = new MarginPadding(10),
                                Font = OsuFont.GetFont(size: 16),
                            },
                            new TriangleButton
                            {
                                Text = "off",
                                RelativeSizeAxes = Axes.X,
                                Width = 0.2f,
                                Action = () =>
                                {
                                    applyGlobalCompPreset(0);
                                    applySampleCompPreset(0);
                                }
                            },
                            new TriangleButton
                            {
                                Text = "1",
                                RelativeSizeAxes = Axes.X,
                                Width = 0.2f,
                                Action = () =>
                                {
                                    applyGlobalCompPreset(1);
                                    applySampleCompPreset(1);
                                }
                            },
                            new TriangleButton
                            {
                                Text = "2",
                                RelativeSizeAxes = Axes.X,
                                Width = 0.2f,
                                Action = () =>
                                {
                                    applyGlobalCompPreset(2);
                                    applySampleCompPreset(2);
                                }
                            },
                            new TriangleButton
                            {
                                Text = "3",
                                RelativeSizeAxes = Axes.X,
                                Width = 0.2f,
                                Action = () =>
                                {
                                    applyGlobalCompPreset(3);
                                    applySampleCompPreset(3);
                                }
                            },
                        }
                    }
                }
            };

            globalCompParams = new CompressorParameters
            {
                fAttack = globalAttack.Value,
                fRelease = globalRelease.Value,
                fThreshold = globalThreshold.Value,
                fGain = globalGain.Value,
                fRatio = globalRatio.Value
            };

            sampleCompParams = new CompressorParameters
            {
                fAttack = sampleAttack.Value,
                fRelease = sampleRelease.Value,
                fThreshold = sampleThreshold.Value,
                fGain = sampleGain.Value,
                fRatio = sampleRatio.Value
            };

            globalEnabled.ValueChanged += globalCompToggle;
            globalAttack.ValueChanged += globalCompUpdate;
            globalRelease.ValueChanged += globalCompUpdate;
            globalThreshold.ValueChanged += globalCompUpdate;
            globalGain.ValueChanged += globalCompUpdate;
            globalRatio.ValueChanged += globalCompUpdate;

            sampleEnabled.ValueChanged += sampleCompToggle;
            sampleAttack.ValueChanged += sampleCompUpdate;
            sampleRelease.ValueChanged += sampleCompUpdate;
            sampleThreshold.ValueChanged += sampleCompUpdate;
            sampleGain.ValueChanged += sampleCompUpdate;
            sampleRatio.ValueChanged += sampleCompUpdate;
        }

        private void applySampleCompPreset(int preset)
        {
            sampleEnabled.Value = presets[preset, 1, 0] == 1;
            sampleAttack.Value = presets[preset, 1, 1];
            sampleRelease.Value = presets[preset, 1, 2];
            sampleThreshold.Value = presets[preset, 1, 3];
            sampleGain.Value = presets[preset, 1, 4];
            sampleRatio.Value = presets[preset, 1, 5];
        }

        private void applyGlobalCompPreset(int preset)
        {
            globalEnabled.Value = presets[preset, 0, 0] == 1;
            globalAttack.Value = presets[preset, 0, 1];
            globalRelease.Value = presets[preset, 0, 2];
            globalThreshold.Value = presets[preset, 0, 3];
            globalGain.Value = presets[preset, 0, 4];
            globalRatio.Value = presets[preset, 0, 5];
        }

        private void globalCompToggle(ValueChangedEvent<bool> obj)
        {
            if (obj.NewValue)
                addCompressor(globalCompParams, globalMixer);
            else
                removeCompressor(globalCompParams, globalMixer);
        }

        private void globalCompUpdate(ValueChangedEvent<float> _)
        {
            globalCompParams.fAttack = globalAttack.Value;
            globalCompParams.fRelease = globalRelease.Value;
            globalCompParams.fThreshold = globalThreshold.Value;
            globalCompParams.fGain = globalGain.Value;
            globalCompParams.fRatio = globalRatio.Value;

            updateCompressor(globalCompParams, globalMixer);
        }

        private void sampleCompToggle(ValueChangedEvent<bool> obj)
        {
            if (obj.NewValue)
                addCompressor(sampleCompParams, sampleMixer);
            else
                removeCompressor(sampleCompParams, sampleMixer);
        }

        private void sampleCompUpdate(ValueChangedEvent<float> _)
        {
            sampleCompParams.fAttack = sampleAttack.Value;
            sampleCompParams.fRelease = sampleRelease.Value;
            sampleCompParams.fThreshold = sampleThreshold.Value;
            sampleCompParams.fGain = sampleGain.Value;
            sampleCompParams.fRatio = sampleRatio.Value;

            updateCompressor(sampleCompParams, sampleMixer);
        }

        private void addCompressor(CompressorParameters compressor, AudioMixer mixer)
        {
            mixer.Effects.Add(compressor);

            Logger.Log($"{mixer.Identifier} EFFECT COUNT: {mixer.Effects.Count}");
        }

        private void removeCompressor(CompressorParameters compressor, AudioMixer mixer)
        {
            int effectIndex = mixer.Effects.IndexOf(compressor);

            if (effectIndex < 0) return;

            mixer.Effects.RemoveAt(effectIndex);

            Logger.Log($"{mixer.Identifier} EFFECT COUNT: {mixer.Effects.Count}");
        }

        private void updateCompressor(CompressorParameters compressor, AudioMixer mixer)
        {
            int effectIndex = mixer.Effects.IndexOf(compressor);

            if (effectIndex < 0) return;

            if (mixer.Effects[effectIndex] is CompressorParameters)
                mixer.Effects[effectIndex] = compressor;
        }

        private void toggleSampleSpam()
        {
            spammingButton.Text = spamming ? "Start Sample Spam" : "Stop Sample Spam";
            spamming = !spamming;
        }

        protected override void Update()
        {
            base.Update();

            if (!spamming) return;

            if (Time.Current - spamSample1LastPlayed >= 50)
            {
                spamSample1.Play();
                spamSample1LastPlayed = Time.Current;
            }

            if (Time.Current - spamSample2LastPlayed >= 500)
            {
                spamSample2.Play();
                spamSample2LastPlayed = Time.Current;
            }
        }

        private void toggleTrack()
        {
            if (trackPlaying)
            {
                track.Stop();
                playingButton.Text = "Start Track";
            }
            else
            {
                track.Start();
                playingButton.Text = "Stop Track";
            }

            trackPlaying = !trackPlaying;
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            globalMixer = audio.GlobalMixer;
            sampleMixer = audio.SampleMixer;

            beatmap = new WaveformTestBeatmap(audio);
            track = beatmap.LoadTrack();

            spamSample1 = audio.Samples.Get(@"SongSelect/select-difficulty");
            spamSample2 = audio.Samples.Get(@"SongSelect/select-expand");
            spamSample3 = audio.Samples.Get(@"SongSelect/confirm-selection");
        }

        protected override void Dispose(bool isDisposing)
        {
            globalMixer.Effects.Clear();
            sampleMixer.Effects.Clear();

            track.Dispose();

            spamSample1.Dispose();
            spamSample2.Dispose();
            spamSample3.Dispose();

            base.Dispose(isDisposing);
        }
    }
}
