// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;

namespace osu.Game.Tests.Beatmaps
{
    [TestFixture]
    public class WorkingBeatmapTest
    {
        [Test]
        public void TestGetPlayableSuccess()
        {
            var working = new TestNeverLoadsWorkingBeatmap();

            working.ResetEvent.Set();

            Assert.NotNull(working.GetPlayableBeatmap(new OsuRuleset().RulesetInfo));
        }

        [Test]
        public void TestGetPlayableCancellationToken()
        {
            var working = new TestNeverLoadsWorkingBeatmap();

            var cts = new CancellationTokenSource();
            var loadStarted = new ManualResetEventSlim();
            var loadCompleted = new ManualResetEventSlim();

            Task.Factory.StartNew(() =>
            {
                loadStarted.Set();
                Assert.Throws<OperationCanceledException>(() => working.GetPlayableBeatmap(new OsuRuleset().RulesetInfo, cancellationToken: cts.Token));
                loadCompleted.Set();
            }, TaskCreationOptions.LongRunning);

            Assert.IsTrue(loadStarted.Wait(10000));

            cts.Cancel();

            Assert.IsTrue(loadCompleted.Wait(10000));

            working.ResetEvent.Set();
        }

        [Test]
        public void TestGetPlayableDefaultTimeout()
        {
            var working = new TestNeverLoadsWorkingBeatmap();

            Assert.Throws<OperationCanceledException>(() => working.GetPlayableBeatmap(new OsuRuleset().RulesetInfo));

            working.ResetEvent.Set();
        }

        [Test]
        public void TestGetPlayableRulesetLoadFailure()
        {
            var working = new TestWorkingBeatmap(new Beatmap());

            // by default mocks return nulls if not set up, which is actually desired here to simulate a ruleset load failure scenario.
            var ruleset = new Mock<IRulesetInfo>();

            Assert.Throws<RulesetLoadException>(() => working.GetPlayableBeatmap(ruleset.Object));
        }

        public class TestNeverLoadsWorkingBeatmap : TestWorkingBeatmap
        {
            public ManualResetEventSlim ResetEvent = new ManualResetEventSlim();

            public TestNeverLoadsWorkingBeatmap()
                : base(new Beatmap())
            {
            }

            protected override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap, Ruleset ruleset) => new TestConverter(beatmap, ResetEvent);

            public class TestConverter : IBeatmapConverter
            {
                private readonly ManualResetEventSlim resetEvent;

                public TestConverter(IBeatmap beatmap, ManualResetEventSlim resetEvent)
                {
                    this.resetEvent = resetEvent;
                    Beatmap = beatmap;
                }

                public event Action<HitObject, IEnumerable<HitObject>> ObjectConverted;

                protected virtual void OnObjectConverted(HitObject arg1, IEnumerable<HitObject> arg2) => ObjectConverted?.Invoke(arg1, arg2);

                public IBeatmap Beatmap { get; }

                public bool CanConvert() => true;

                public IBeatmap Convert(CancellationToken cancellationToken = default)
                {
                    resetEvent.Wait(cancellationToken);
                    return new OsuBeatmap();
                }
            }
        }
    }
}
