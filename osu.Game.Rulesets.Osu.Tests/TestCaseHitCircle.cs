// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using OpenTK;
using osu.Game.Rulesets.Osu.Judgements;
using System.Collections.Generic;
using System;
using osu.Game.Rulesets.Mods;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Osu.Replays;
using osu.Framework.Input;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestCaseHitCircle : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableHitCircle)
        };

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private int depthIndex;
        protected readonly List<Mod> Mods = new List<Mod>();

        public TestCaseHitCircle()
        {
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Miss Big Single", () => testSingle(2));
            AddStep("Miss Medium Single", () => testSingle(5));
            AddStep("Miss Small Single", () => testSingle(7));
            AddStep("Hit Big Single", () => testSingle(2, true));
            AddStep("Hit Medium Single", () => testSingle(5, true));
            AddStep("Hit Small Single", () => testSingle(7, true));
            AddStep("Miss Big Stream", () => testStream(2));
            AddStep("Miss Medium Stream", () => testStream(5));
            AddStep("Miss Small Stream", () => testStream(7));
            AddStep("Hit Big Stream", () => testStream(2, true));
            AddStep("Hit Medium Stream", () => testStream(5, true));
            AddStep("Hit Small Stream", () => testStream(7, true));
            AddStep("Understream Medium Stream", () => testUnderstream(5));
            AddStep("Overstream Medium Stream", () => testOverstream(5));
        }

        private void testSingle(float circleSize, bool auto = false, double timeOffset = 0, Vector2? positionOffset = null)
        {
            positionOffset = positionOffset ?? Vector2.Zero;

            var circle = new HitCircle
            {
                StartTime = Time.Current + 1000 + timeOffset,
                Position = positionOffset.Value,
            };

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = circleSize });

            var drawable = new TestDrawableHitCircle(circle, auto)
            {
                Anchor = Anchor.Centre,
                Depth = depthIndex++
            };

            foreach (var mod in Mods.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(new[] { drawable });

            Add(drawable);
        }

        private void testStream(float circleSize, bool auto = false)
        {
            Vector2 pos = new Vector2(-250, 0);

            for (int i = 0; i <= 1000; i += 100)
            {
                testSingle(circleSize, auto, i, pos);
                pos.X += 50;
            }
        }
        private void testOffstream(float circleSize, double hitError, double hitErrorDelta)
        {
            RemoveAll(new Predicate<Drawable>(Drawable => true));
            testStream(circleSize, false);

            List<ReplayFrame> replayFrames = new List<ReplayFrame>();
            IEnumerable<DrawableHitCircle> hitCircles = Children.OfType<DrawableHitCircle>().Reverse();
            replayFrames.Add(new OsuReplayFrame(hitCircles.First().HitObject.StartTime + hitError - 1, Children.OfType<DrawableHitCircle>().Last().Position));
            foreach (var hitCircle in hitCircles)
            {
                replayFrames.Add(new OsuReplayFrame(hitCircle.HitObject.StartTime + hitError, new Vector2(hitCircle.Position.X, hitCircle.Position.Y), OsuAction.LeftButton));
                replayFrames.Add(new OsuReplayFrame(hitCircle.HitObject.StartTime + hitError + 1, new Vector2(hitCircle.Position.X, hitCircle.Position.Y)));
                hitError += hitErrorDelta;
            }
            Replay replay = new Replay();
            replay.Frames = replayFrames;
            OsuReplayInputHandler replayHandler = new OsuReplayInputHandler(replay);
            replayHandler.GamefieldToScreenSpace = clickPos => ToScreenSpace(hitCircles.First().AnchorPosition + clickPos + new Vector2(StepsContainer.DrawWidth, 0));
            Children.OfType<DrawableHitCircle>().First().OsuActionInputManager.ReplayInputHandler = replayHandler;
        }
        private void testUnderstream(float circleSize)
        {
            testOffstream(circleSize, -125, 30);
        }
        private void testOverstream(float circleSize)
        {
            testOffstream(circleSize, 125, -30);
        }

        private class TestDrawableHitCircle : DrawableHitCircle
        {
            private readonly bool auto;

            public TestDrawableHitCircle(HitCircle h, bool auto) : base(h)
            {
                this.auto = auto;
                OnJudgement += onJudgement;
            }

            private void onJudgement(DrawableHitObject judegedObject, Judgement judgement)
            {
                DrawableOsuJudgement drawable = new DrawableOsuJudgement(judgement, judegedObject);
                drawable.RelativeAnchorPosition = new Vector2(0.5f, 0.5f);
                drawable.Origin = Anchor.Centre;
                drawable.Position = Position;
                ((Container)Parent).Add(drawable);
            }

            protected override void CheckForJudgements(bool userTriggered, double timeOffset)
            {
                if (auto && !userTriggered && timeOffset > 0)
                {
                    // force success
                    AddJudgement(new OsuJudgement
                    {
                        Result = HitResult.Great
                    });
                    State.Value = ArmedState.Hit;
                }
                else
                    base.CheckForJudgements(userTriggered, timeOffset);
            }
        }
    }
}
