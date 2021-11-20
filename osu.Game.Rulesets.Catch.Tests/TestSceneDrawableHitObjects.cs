// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestSceneDrawableHitObjects : OsuTestScene
    {
        private DrawableCatchRuleset drawableRuleset;
        private double playfieldTime => drawableRuleset.Playfield.Time.Current;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            var controlPointInfo = new ControlPointInfo();
            controlPointInfo.Add(0, new TimingControlPoint());

            IWorkingBeatmap beatmap = CreateWorkingBeatmap(new Beatmap
            {
                HitObjects = new List<HitObject> { new Fruit() },
                BeatmapInfo = new BeatmapInfo
                {
                    BaseDifficulty = new BeatmapDifficulty(),
                    Metadata = new BeatmapMetadata
                    {
                        Artist = @"Unknown",
                        Title = @"You're breathtaking",
                        AuthorString = @"Everyone",
                    },
                    Ruleset = new CatchRuleset().RulesetInfo
                },
                ControlPointInfo = controlPointInfo
            });

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    drawableRuleset = new DrawableCatchRuleset(new CatchRuleset(), beatmap.GetPlayableBeatmap(new CatchRuleset().RulesetInfo))
                }
            };
        });

        [Test]
        public void TestFruits()
        {
            AddStep("hit fruits", () => spawnFruits(true));
            AddUntilStep("wait for completion", () => playfieldIsEmpty);
            AddAssert("catcher state is idle", () => catcherState == CatcherAnimationState.Idle);

            AddStep("miss fruits", () => spawnFruits());
            AddUntilStep("wait for completion", () => playfieldIsEmpty);
            AddAssert("catcher state is failed", () => catcherState == CatcherAnimationState.Fail);
        }

        [Test]
        public void TestJuicestream()
        {
            AddStep("hit juicestream", () => spawnJuiceStream(true));
            AddUntilStep("wait for completion", () => playfieldIsEmpty);
            AddAssert("catcher state is idle", () => catcherState == CatcherAnimationState.Idle);

            AddStep("miss juicestream", () => spawnJuiceStream());
            AddUntilStep("wait for completion", () => playfieldIsEmpty);
            AddAssert("catcher state is failed", () => catcherState == CatcherAnimationState.Fail);
        }

        [Test]
        public void TestBananas()
        {
            AddStep("hit bananas", () => spawnBananas(true));
            AddUntilStep("wait for completion", () => playfieldIsEmpty);
            AddAssert("catcher state is idle", () => catcherState == CatcherAnimationState.Idle);

            AddStep("miss bananas", () => spawnBananas());
            AddUntilStep("wait for completion", () => playfieldIsEmpty);
            AddAssert("catcher state is idle", () => catcherState == CatcherAnimationState.Idle);
        }

        private bool playfieldIsEmpty => !((CatchPlayfield)drawableRuleset.Playfield).AllHitObjects.Any(h => h.IsAlive);

        private CatcherAnimationState catcherState => ((CatchPlayfield)drawableRuleset.Playfield).Catcher.CurrentState;

        private void spawnFruits(bool hit = false)
        {
            for (int i = 1; i <= 4; i++)
            {
                var fruit = new Fruit
                {
                    X = getXCoords(hit),
                    LastInCombo = i % 4 == 0,
                    StartTime = playfieldTime + 800 + (200 * i)
                };

                fruit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                addToPlayfield(new DrawableFruit(fruit));
            }
        }

        private void spawnJuiceStream(bool hit = false)
        {
            float xCoords = getXCoords(hit);

            var juice = new JuiceStream
            {
                X = xCoords,
                StartTime = playfieldTime + 1000,
                Path = new SliderPath(PathType.Linear, new[]
                {
                    Vector2.Zero,
                    new Vector2(0, 200)
                })
            };

            juice.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            if (juice.NestedHitObjects.Last() is CatchHitObject tail)
                tail.LastInCombo = true; // usually the (Catch)BeatmapProcessor would do this for us when necessary

            addToPlayfield(new DrawableJuiceStream(juice));
        }

        private void spawnBananas(bool hit = false)
        {
            for (int i = 1; i <= 4; i++)
            {
                var banana = new Banana
                {
                    X = getXCoords(hit),
                    LastInCombo = i % 4 == 0,
                    StartTime = playfieldTime + 800 + (200 * i)
                };

                banana.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                addToPlayfield(new DrawableBanana(banana));
            }
        }

        private float getXCoords(bool hit)
        {
            const float x_offset = 0.2f * CatchPlayfield.WIDTH;
            float xCoords = CatchPlayfield.CENTER_X;

            if (drawableRuleset.Playfield is CatchPlayfield catchPlayfield)
                catchPlayfield.Catcher.X = xCoords - x_offset;

            if (hit)
                xCoords -= x_offset;
            else
                xCoords += x_offset;

            return xCoords;
        }

        private void addToPlayfield(DrawableCatchHitObject drawable)
        {
            foreach (var mod in SelectedMods.Value.OfType<IApplicableToDrawableHitObject>())
                mod.ApplyToDrawableHitObject(drawable);

            drawableRuleset.Playfield.Add(drawable);
        }
    }
}
