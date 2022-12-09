// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Replays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osu.Game.Storyboards;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneTrianglesSpinnerRotation : TestSceneOsuPlayer
    {
        private const double spinner_start_time = 100;
        private const double spinner_duration = 6000;

        [Resolved]
        private SkinManager skinManager { get; set; } = null!;

        [Resolved]
        private AudioManager audioManager { get; set; } = null!;

        protected override bool Autoplay => true;

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new ScoreExposedPlayer();

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard? storyboard = null)
            => new ClockBackedTestWorkingBeatmap(beatmap, storyboard, new FramedClock(new ManualClock { Rate = 1 }), audioManager);

        private DrawableSpinner drawableSpinner = null!;
        private SpriteIcon spinnerSymbol => drawableSpinner.ChildrenOfType<SpriteIcon>().Single();

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("set triangles skin", () => skinManager.CurrentSkinInfo.Value = TrianglesSkin.CreateInfo().ToLiveUnmanaged());

            AddUntilStep("wait for track to start running", () => Beatmap.Value.Track.IsRunning);
            AddStep("retrieve spinner", () => drawableSpinner = (DrawableSpinner)Player.DrawableRuleset.Playfield.AllHitObjects.First());
        }

        [Test]
        public void TestSymbolMiddleRewindingRotation()
        {
            double finalSpinnerSymbolRotation = 0, spinnerSymbolRotationTolerance = 0;

            addSeekStep(spinner_start_time + 5000);
            AddStep("retrieve spinner symbol rotation", () =>
            {
                finalSpinnerSymbolRotation = spinnerSymbol.Rotation;
                spinnerSymbolRotationTolerance = Math.Abs(finalSpinnerSymbolRotation * 0.05f);
            });

            addSeekStep(spinner_start_time + 2500);
            AddAssert("symbol rotation rewound",
                () => spinnerSymbol.Rotation, () => Is.EqualTo(finalSpinnerSymbolRotation / 2).Within(spinnerSymbolRotationTolerance));

            addSeekStep(spinner_start_time + 5000);
            AddAssert("is symbol rotation almost same",
                () => spinnerSymbol.Rotation, () => Is.EqualTo(finalSpinnerSymbolRotation).Within(spinnerSymbolRotationTolerance));
        }

        [Test]
        public void TestSymbolRotationDirection([Values(true, false)] bool clockwise)
        {
            if (clockwise)
                transformReplay(flip);

            addSeekStep(5000);
            AddAssert("spinner symbol direction correct", () => clockwise ? spinnerSymbol.Rotation > 0 : spinnerSymbol.Rotation < 0);
        }

        private Replay flip(Replay scoreReplay) => new Replay
        {
            Frames = scoreReplay
                     .Frames
                     .Cast<OsuReplayFrame>()
                     .Select(replayFrame =>
                     {
                         var flippedPosition = new Vector2(OsuPlayfield.BASE_SIZE.X - replayFrame.Position.X, replayFrame.Position.Y);
                         return new OsuReplayFrame(replayFrame.Time, flippedPosition, replayFrame.Actions.ToArray());
                     })
                     .Cast<ReplayFrame>()
                     .ToList()
        };

        private void addSeekStep(double time)
        {
            AddStep($"seek to {time}", () => Player.GameplayClockContainer.Seek(time));
            AddUntilStep("wait for seek to finish", () => Player.DrawableRuleset.FrameStableClock.CurrentTime, () => Is.EqualTo(time).Within(100));
        }

        private void transformReplay(Func<Replay, Replay> replayTransformation) => AddStep("set replay", () =>
        {
            var drawableRuleset = this.ChildrenOfType<DrawableOsuRuleset>().Single();
            var score = drawableRuleset.ReplayScore;
            var transformedScore = new Score
            {
                ScoreInfo = score.ScoreInfo,
                Replay = replayTransformation.Invoke(score.Replay)
            };
            drawableRuleset.SetReplayScore(transformedScore);
        });

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new Beatmap
        {
            HitObjects = new List<HitObject>
            {
                new Spinner
                {
                    Position = new Vector2(256, 192),
                    StartTime = spinner_start_time,
                    Duration = spinner_duration
                },
            }
        };

        private partial class ScoreExposedPlayer : TestPlayer
        {
            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

            public ScoreExposedPlayer()
                : base(false, false)
            {
            }
        }
    }
}
