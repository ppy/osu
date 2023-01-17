// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Skinning.Legacy;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Skinning;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneCursorParticles : TestSceneOsuPlayer
    {
        protected override bool Autoplay => autoplay;
        protected override bool HasCustomSteps => true;

        private bool autoplay;
        private IBeatmap currentBeatmap;

        [Resolved]
        private SkinManager skinManager { get; set; }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => currentBeatmap ?? base.CreateBeatmap(ruleset);

        [Test]
        public void TestLegacyBreakParticles()
        {
            LegacyCursorParticles cursorParticles = null;

            createLegacyTest(false, () => new Beatmap
            {
                Breaks =
                {
                    new BreakPeriod(8500, 10000),
                },
                HitObjects =
                {
                    new HitCircle
                    {
                        StartTime = 8000,
                        Position = OsuPlayfield.BASE_SIZE / 2,
                    },
                    new HitCircle
                    {
                        StartTime = 11000,
                        Position = OsuPlayfield.BASE_SIZE / 2,
                    },
                }
            });

            AddUntilStep("fetch cursor particles", () =>
            {
                cursorParticles = this.ChildrenOfType<LegacyCursorParticles>().SingleOrDefault();
                return cursorParticles != null;
            });

            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(Player.ScreenSpaceDrawQuad.Centre));

            AddAssert("particles are being spawned", () => cursorParticles.Active);

            AddStep("press left mouse button", () => InputManager.PressButton(MouseButton.Left));
            AddWaitStep("wait a bit", 5);
            AddStep("press right mouse button", () => InputManager.PressButton(MouseButton.Right));
            AddWaitStep("wait a bit", 5);
            AddStep("release left mouse button", () => InputManager.ReleaseButton(MouseButton.Left));
            AddWaitStep("wait a bit", 5);
            AddStep("release right mouse button", () => InputManager.ReleaseButton(MouseButton.Right));

            AddUntilStep("wait for beatmap start", () => !Player.IsBreakTime.Value);
            AddAssert("particle spawning stopped", () => !cursorParticles.Active);

            AddUntilStep("wait for break", () => Player.IsBreakTime.Value);
            AddAssert("particles are being spawned", () => cursorParticles.Active);

            AddUntilStep("wait for break end", () => !Player.IsBreakTime.Value);
            AddAssert("particle spawning stopped", () => !cursorParticles.Active);
        }

        [Test]
        public void TestLegacyKiaiParticles()
        {
            LegacyCursorParticles cursorParticles = null;
            DrawableSpinner spinner = null;
            DrawableSlider slider = null;

            createLegacyTest(true, () =>
                {
                    var controlPointInfo = new ControlPointInfo();
                    controlPointInfo.Add(0, new EffectControlPoint { KiaiMode = true });
                    controlPointInfo.Add(5000, new EffectControlPoint { KiaiMode = false });

                    return new Beatmap
                    {
                        ControlPointInfo = controlPointInfo,
                        HitObjects =
                        {
                            new Spinner
                            {
                                StartTime = 0,
                                Duration = 1000,
                                Position = OsuPlayfield.BASE_SIZE / 2,
                            },
                            new Slider
                            {
                                StartTime = 2500,
                                RepeatCount = 0,
                                Position = OsuPlayfield.BASE_SIZE / 2,
                                Path = new SliderPath(new[]
                                {
                                    new PathControlPoint(Vector2.Zero),
                                    new PathControlPoint(new Vector2(100, 0)),
                                })
                            },
                            new HitCircle
                            {
                                StartTime = 4500,
                                Position = OsuPlayfield.BASE_SIZE / 2,
                            },
                        },
                    };
                }
            );

            AddUntilStep("fetch cursor particles", () =>
            {
                cursorParticles = this.ChildrenOfType<LegacyCursorParticles>().SingleOrDefault();
                return cursorParticles != null;
            });

            AddUntilStep("wait for spinner tracking", () =>
            {
                spinner = this.ChildrenOfType<DrawableSpinner>().SingleOrDefault();
                return spinner?.RotationTracker.Tracking == true;
            });
            AddAssert("particles are being spawned", () => cursorParticles.Active);

            AddUntilStep("spinner tracking stopped", () => !spinner.RotationTracker.Tracking);
            AddAssert("particle spawning stopped", () => !cursorParticles.Active);

            AddUntilStep("wait for slider tracking", () =>
            {
                slider = this.ChildrenOfType<DrawableSlider>().SingleOrDefault();
                return slider?.Tracking.Value == true;
            });
            AddAssert("particles are being spawned", () => cursorParticles.Active);

            AddUntilStep("slider tracking stopped", () => !slider.Tracking.Value);
            AddAssert("particle spawning stopped", () => !cursorParticles.Active);
        }

        private void createLegacyTest(bool autoplay, Func<IBeatmap> beatmap) => CreateTest(() =>
        {
            AddStep("set beatmap", () =>
            {
                this.autoplay = autoplay;
                currentBeatmap = beatmap();
            });
            AddStep("setup default legacy skin", () =>
            {
                skinManager.CurrentSkinInfo.Value = skinManager.DefaultClassicSkin.SkinInfo;
            });
        });
    }
}
