// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using ManagedBass.Fx;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Audio.Effects;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Settings;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public partial class AfToggleSection : SettingsSection
    {
        public override LocalisableString Header => "Toggles";

        public override Drawable CreateIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Regular.Dizzy,
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new ToggleSettings(),
            };
        }

        public partial class ToggleSettings : SettingsSubsection
        {
            [Resolved]
            private OsuGameBase game { get; set; } = null!;

            [Resolved]
            private GameHost host { get; set; } = null!;

            [Resolved]
            private MusicController musicController { get; set; } = null!;

            protected override LocalisableString Header => "You wanted toggles, we give you toggles";

            private List<string> words = new List<string>();

            private List<Action<ValueChangedEvent<bool>>> heroActions = new List<Action<ValueChangedEvent<bool>>>();

            private readonly List<ScheduledDelegate> tasks = new List<ScheduledDelegate>();

            private GlobalActionContainer target = null!;

            private AutoWahParameters autoWahParameters = null!;
            private ChorusParameters chorusParameters = null!;
            private PhaserParameters phaserParameters = null!;
            private Bindable<double> balanceAdjustment = null!;

            private Sprite? dvdLogo;
            private Vector2 dvdLogoVelocity = new Vector2(0.005f);

            private ScheduledDelegate? dvdLogoMovement;

            private AudioManager audio = null!;

            private double lastUpdate;

            [Resolved]
            private TextureStore textures { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                autoWahParameters = new AutoWahParameters { fDryMix = 0.5f, fWetMix = 0.5f };
                chorusParameters = new ChorusParameters { fDryMix = 0.5f, fWetMix = 0.5f };
                phaserParameters = new PhaserParameters { fDryMix = 0.5f, fWetMix = 0.5f };
                balanceAdjustment = new Bindable<double>(1);

                audio = game.Audio;

                target = game.ChildrenOfType<GlobalActionContainer>().First();
                target.Anchor = Anchor.Centre;
                target.Origin = Anchor.Centre;

                reset();
            }

            protected override void Update()
            {
                base.Update();

                lastUpdate = Clock.CurrentTime;

                tasks.RemoveAll(t => t.Cancelled || t.Completed);

                if (tasks.Count > 40)
                    explode();
            }

            private void explode()
            {
                foreach (var t in tasks)
                    t.Cancel();

                target.FadeColour(Color4.Black, 2000);
                target.ScaleTo(1)
                      .ScaleTo(10, 4000);

                audio.Samples.Get("Gameplay/Argon/failsound-alt")?.Play();
                musicController.DuckMomentarily(2000, new DuckParameters
                {
                    DuckDuration = 0,
                    DuckVolumeTo = 0,
                    DuckCutoffTo = AudioFilter.MAX_LOWPASS_CUTOFF,
                    RestoreDuration = 3000,
                    RestoreEasing = Easing.InQuint,
                });

                host.UpdateThread.Scheduler.AddDelayed(() =>
                {
                    foreach (var c in getCheckboxes())
                    {
                        c.Current.Disabled = false;
                        c.Current.Value = false;
                    }

                    target.FinishTransforms();
                    target.ScaleTo(1)
                          .FadeColour(Color4.White, 2000, Easing.OutQuint);

                    Schedule(() =>
                    {
                        var settingsOverlay = game.ChildrenOfType<SettingsOverlay>().First();

                        settingsOverlay.SectionsContainer.ScrollTo(settingsOverlay.ChildrenOfType<AfToggleSection>().Single());
                    });

                    Clear();

                    AddRange(new Drawable[]
                    {
                        new OsuTextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            TextAnchor = Anchor.TopCentre,
                            Text = "..on second thought, no more toggles for you."
                        },
                        new SettingsButtonV2
                        {
                            Text = "But please peppy",
                            Action = () =>
                            {
                                if (Children.Count < 5 || RNG.NextSingle() > 0.08f)
                                {
                                    audio.Samples.Get("Gameplay/sectionfail")?.Play();
                                    Add(new OsuTextFlowContainer(p => p.Font = OsuFont.Default.With(size: RNG.Next(10, 30)))
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        TextAnchor = Anchor.TopCentre,
                                        Text = "no"
                                    });
                                }
                                else
                                {
                                    audio.Samples.Get("Gameplay/sectionpass")?.Play();
                                    reset();
                                }
                            }
                        }
                    });
                }, 3000);
            }

            private void reset()
            {
                Clear();

                words = new List<string>(
                [
                    "toggle", "toggleability", "inversion", "momentum", "uncertainty", "viscosity", "gravity", "shyness", "peer pressure", "amnesia", "rebellion", "telepathy", "commitment issues",
                    "democracy", "entropy", "recursion", "quantum superposition", "paperclips", "stubbornness", "existentialism", "nostalgia", "anxiety", "procrastination", "synesthesia",
                    "narcissism",
                    "chaos",
                    "determinism", "nihilism", "optimism", "pessimism", "apathy", "enthusiasm", "seven twenty seven", "lethargy", "hyperactivity", "confusion", "clarity", "ambiguity", "precision",
                    "vagueness",
                    "specificity", "generality", "locality", "annihilation", "nonlocality", "causality", "acausality", "reversibility", "irreversibility", "coherence", "decoherence", "entanglement",
                    "disentanglement", "superposition"
                ]);

                heroActions = new List<Action<ValueChangedEvent<bool>>>
                {
                    val => target.FadeColour(OsuColour.Gray(val.NewValue ? 0.5f : 1), 2500, Easing.OutPow10),
                    val =>
                    {
                        if (val.NewValue)
                        {
                            target.ScaleTo(new Vector2(RNG.NextBool() ? -1 : 1, RNG.NextBool() ? -1 : 1))
                                  .Delay(1500)
                                  .ScaleTo(Vector2.One);
                        }
                    },
                    val =>
                    {
                        if (val.NewValue)
                        {
                            target.FadeColour(new Color4(
                                (byte)RNG.Next(200, 255),
                                (byte)RNG.Next(200, 255),
                                (byte)RNG.Next(200, 255),
                                255), 1500, Easing.OutQuint);
                        }
                        else
                            target.FadeColour(Color4.White, 2000);
                    },
                    val =>
                    {
                        if (val.NewValue)
                            target.RotateTo(RNG.NextSingle(-2f, 2f), 4000, Easing.InOutSine);
                        else
                            target.RotateTo(0, 4000, Easing.InOutSine);
                    },
                    val => target.ScaleTo(val.NewValue ? 1.05f : 1f, 3000, Easing.OutQuint),
                    val =>
                    {
                        if (val.NewValue)
                            target.ScaleTo(1.02f, 2000, Easing.InOutSine).Then().ScaleTo(0.98f, 2000, Easing.InOutSine).Loop();
                        else
                            target.ScaleTo(1f, 1000);
                    },
                    val =>
                    {
                        if (val.NewValue)
                        {
                            var colours = new[] { new Color4(255, 230, 230, 255), new Color4(230, 255, 230, 255), new Color4(230, 230, 255, 255) };
                            target.FadeColour(colours[RNG.Next(0, colours.Length)], 300).Loop();
                        }
                        else
                            target.FadeColour(Color4.White, 3000);
                    },
                    val =>
                    {
                        if (val.NewValue)
                            target.MoveToX(10, 500, Easing.OutQuad).Then().MoveToX(0, 500, Easing.InQuad);
                    },
                    val =>
                    {
                        if (val.NewValue)
                            target.MoveToY(-10, 500, Easing.OutQuad).Then().MoveToY(0, 500, Easing.InQuad);
                    },
                    _ => game.ChildrenOfType<Toolbar.Toolbar>().FirstOrDefault()?.ToggleVisibility(),
                    val =>
                    {
                        if (val.NewValue)
                            audio.TrackMixer.AddEffect(chorusParameters);
                        else
                            audio.TrackMixer.RemoveEffect(chorusParameters);
                    },
                    val =>
                    {
                        if (val.NewValue)
                            audio.TrackMixer.AddEffect(autoWahParameters);
                        else
                            audio.TrackMixer.RemoveEffect(autoWahParameters);
                    },
                    val =>
                    {
                        if (val.NewValue)
                            audio.TrackMixer.AddEffect(phaserParameters);
                        else
                            audio.TrackMixer.RemoveEffect(phaserParameters);
                    },

                    // game blinks at you
                    val =>
                    {
                        if (val.NewValue)
                        {
                            target.ScaleTo(new Vector2(1, 0), 50, Easing.OutQuint)
                                  .Then().ScaleTo(Vector2.One, 50, Easing.OutQuint);
                        }
                    },

                    // messing with balance
                    val =>
                    {
                        if (val.NewValue)
                        {
                            audio.AddAdjustment(AdjustableProperty.Balance, balanceAdjustment);
                            this.TransformBindableTo(balanceAdjustment, 0.3f, 5_000)
                                .Then().TransformBindableTo(balanceAdjustment, 0.7f, 10_000)
                                .Then().TransformBindableTo(balanceAdjustment, 0.5f, 5_000)
                                .Loop();
                        }
                        else
                        {
                            audio.RemoveAdjustment(AdjustableProperty.Balance, balanceAdjustment);
                        }
                    },

                    // do a barrel roll! (press Z or R twice)
                    val =>
                    {
                        if (val.NewValue)
                        {
                            target.RotateTo(360, 1000)
                                  .Then().RotateTo(0);
                        }
                    },
                    val =>
                    {
                        if (val.NewValue)
                        {
                            if (dvdLogo == null)
                            {
                                target.Add(dvdLogo = new Sprite
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Depth = float.MinValue,
                                    Size = new Vector2(30),
                                    Texture = textures.Get(@"Menu/logo"),
                                    RelativePositionAxes = Axes.Both,
                                    Position = new Vector2(RNG.NextSingle(), RNG.NextSingle()),
                                    Alpha = 0,
                                });
                            }

                            dvdLogo.Alpha = 1;
                            dvdLogoMovement = host.UpdateThread.Scheduler.AddDelayed(() =>
                            {
                                var logoPosition = dvdLogo.ScreenSpaceDrawQuad;
                                var bounds = target.ScreenSpaceDrawQuad;

                                if (logoPosition.TopLeft.X < bounds.TopLeft.X)
                                    dvdLogoVelocity = new Vector2(0.005f, dvdLogoVelocity.Y);
                                else if (logoPosition.BottomRight.X > bounds.BottomRight.X)
                                    dvdLogoVelocity = new Vector2(-0.005f, dvdLogoVelocity.Y);

                                if (logoPosition.TopLeft.Y < bounds.TopLeft.Y)
                                    dvdLogoVelocity = new Vector2(dvdLogoVelocity.X, 0.005f);
                                else if (logoPosition.BottomRight.Y > bounds.BottomRight.Y)
                                    dvdLogoVelocity = new Vector2(dvdLogoVelocity.X, -0.005f);

                                dvdLogo.Position += dvdLogoVelocity;
                            }, 15, true);
                        }
                        else
                        {
                            dvdLogo?.Hide();
                            dvdLogoMovement?.Cancel();
                        }
                    },
                    val =>
                    {
                        if (val.NewValue)
                        {
                            string[] samples =
                            [
                                "Keyboard/key-caps.mp3",
                                "Keyboard/key-confirm.mp3",
                                "Keyboard/key-delete.mp3",
                                "Keyboard/key-movement.mp3",
                                "Keyboard/key-press-1.mp3",
                                "Keyboard/key-press-2.mp3",
                                "Keyboard/key-press-3.mp3",
                                "Keyboard/key-press-4.mp3",
                                "Keyboard/deselect.wav",
                                "Keyboard/key-invalid.wav",
                                "Keyboard/select-all.wav",
                                "Keyboard/select-char.wav",
                                "Keyboard/select-word.wav"
                            ];

                            var repeat = Scheduler.AddDelayed(() =>
                            {
                                audio.Samples.Get(samples[RNG.Next(0, samples.Length)])?.Play();
                            }, 100, true);

                            Scheduler.AddDelayed(repeat.Cancel, 1000);
                        }
                    }
                };

                addNewToggle();
            }

            private void doRandomThing()
            {
                // avoid runaway if settings panel closes
                if (Math.Abs(Clock.CurrentTime - lastUpdate) > 100)
                    return;

                tasks.Add(host.UpdateThread.Scheduler.AddDelayed(() =>
                {
                    var formCheckBoxes = getCheckboxes();

                    if (heroActions.Count > 0 && RNG.NextSingle() > 0.5f)
                    {
                        addNewToggle();
                        return;
                    }

                    foreach (var c in formCheckBoxes.Take(Math.Max(1, formCheckBoxes.Length / 8)))
                    {
                        if (RNG.NextSingle() > 0.5f && formCheckBoxes.Count(b => !b.Current.Disabled) > 5)
                            c.Current.Disabled = !c.Current.Disabled;

                        if (c.Current.Disabled && RNG.NextSingle() > 0.4f)
                            c.Current.Disabled = false;

                        if (RNG.NextSingle() > 0.3f)
                            c.TriggerClick();
                    }
                }, RNG.Next(400, 2000)));
            }

            private FormCheckBox[] getCheckboxes() =>
                Children.OfType<SettingsItemV2>().Select(s => s.Control)
                        .OfType<FormCheckBox>()
                        .OrderBy(_ => RNG.Next(-1, 1))
                        .ToArray();

            private void addNewToggle()
            {
                if (words.Count == 0)
                    return;

                FormCheckBox checkbox;

                string word = words[RNG.Next(0, words.Count)];
                words.Remove(word);

                Add(new SettingsItemV2(checkbox = new FormCheckBox
                {
                    Caption = $"Toggle {word}"
                })
                {
                    Depth = RNG.NextSingle(),
                });

                checkbox.Current.BindValueChanged(val =>
                {
                    if (val.NewValue)
                        doRandomThing();
                });

                if (RNG.NextSingle() > 0.6f)
                {
                    int i = RNG.Next(0, heroActions.Count);
                    checkbox.Current.BindValueChanged(heroActions[i]);
                    heroActions.RemoveAt(i);
                }
            }
        }
    }
}
