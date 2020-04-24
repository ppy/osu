// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public class TestSceneDrawableTaikoMascot : TaikoSkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[]
        {
            typeof(DrawableTaikoMascot),
        }).ToList();

        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo info = new ScrollingTestContainer.TestScrollingInfo
        {
            Direction = { Value = ScrollingDirection.Left },
            TimeRange = { Value = 5000 },
        };

        private readonly List<DrawableTaikoMascot> mascots = new List<DrawableTaikoMascot>();
        private readonly List<SkinnableDrawable> skinnables = new List<SkinnableDrawable>();
        private readonly List<TaikoPlayfield> playfields = new List<TaikoPlayfield>();

        [Test]
        public void TestStateTextures()
        {
            AddStep("Create mascot (idle)", () =>
            {
                skinnables.Clear();
                SetContents(() =>
                {
                    var skinnable = getMascot();
                    skinnables.Add(skinnable);
                    return skinnable;
                });
            });

            AddUntilStep("Wait for SkinnableDrawable", () => skinnables.Any(d => d.Drawable is DrawableTaikoMascot));

            AddStep("Collect mascots", () =>
            {
                mascots.Clear();

                foreach (var skinnable in skinnables)
                {
                    if (skinnable.Drawable is DrawableTaikoMascot mascot)
                        mascots.Add(mascot);
                }
            });

            AddStep("Clear state", () => setState(TaikoMascotAnimationState.Clear));

            AddStep("Kiai state", () => setState(TaikoMascotAnimationState.Kiai));

            AddStep("Fail state", () => setState(TaikoMascotAnimationState.Fail));
        }

        private void setState(TaikoMascotAnimationState state)
        {
            foreach (var mascot in mascots)
            {
                if (mascot == null)
                    continue;

                mascot.Dumb = true;
                mascot.State = state;
            }
        }

        private SkinnableDrawable getMascot() =>
            new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.TaikoDon), _ => new Container(), confineMode: ConfineMode.ScaleToFit)
            {
                RelativePositionAxes = Axes.Both
            };

        [Test]
        public void TestPlayfield()
        {
            AddStep("Create playfield", () =>
            {
                playfields.Clear();
                SetContents(() =>
                {
                    var playfield = new TaikoPlayfield(new ControlPointInfo())
                    {
                        Height = 0.4f,
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                    };

                    playfields.Add(playfield);

                    return playfield;
                });
            });

            AddUntilStep("Wait for SkinnableDrawable", () => playfields.Any(p => p.ChildrenOfType<DrawableTaikoMascot>().Any()));

            AddStep("Collect mascots", () =>
            {
                mascots.Clear();

                foreach (var playfield in playfields)
                {
                    var mascot = playfield.ChildrenOfType<DrawableTaikoMascot>().SingleOrDefault();

                    if (mascot != null)
                        mascots.Add(mascot);
                }
            });

            AddStep("Create hit (miss)", () =>
            {
                foreach (var playfield in playfields)
                    addJudgement(playfield, HitResult.Miss);
            });

            AddAssert("Check if state is fail", () => mascots.Where(d => d != null).All(d => d.PlayfieldState.Value == TaikoMascotAnimationState.Fail));

            AddStep("Create hit (great)", () =>
            {
                foreach (var playfield in playfields)
                    addJudgement(playfield, HitResult.Great);
            });

            AddAssert("Check if state is idle", () => mascots.Where(d => d != null).All(d => d.PlayfieldState.Value == TaikoMascotAnimationState.Idle));
        }

        private void addJudgement(TaikoPlayfield playfield, HitResult result)
        {
            playfield.OnNewResult(new DrawableRimHit(new Hit()), new JudgementResult(new HitObject(), new TaikoJudgement()) { Type = result });
        }
    }
}
