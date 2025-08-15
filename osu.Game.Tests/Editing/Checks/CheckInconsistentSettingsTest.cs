// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osuTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Tests.Beatmaps;
using osu.Game.Storyboards;

namespace osu.Game.Tests.Editing.Checks
{
    [TestFixture]
    public class CheckInconsistentSettingsTest
    {
        private CheckInconsistentSettings check = null!;

        [SetUp]
        public void Setup()
        {
            check = new CheckInconsistentSettings();
        }

        [Test]
        public void TestConsistentSettings()
        {
            var beatmaps = createBeatmapSetWithSettings(
                createSettings(audioLeadIn: 1000, countdown: CountdownType.Normal, epilepsyWarning: false),
                createSettings(audioLeadIn: 1000, countdown: CountdownType.Normal, epilepsyWarning: false)
            );
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestInconsistentAudioLeadIn()
        {
            var beatmaps = createBeatmapSetWithSettings(
                createSettings(audioLeadIn: 1000),
                createSettings(audioLeadIn: 2000)
            );
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentSettings.IssueTemplateInconsistentSetting);
            Assert.That(issues[0].ToString(), Contains.Substring("Audio lead-in"));
            Assert.That(issues[0].ToString(), Contains.Substring("1000"));
            Assert.That(issues[0].ToString(), Contains.Substring("2000"));
        }

        [Test]
        public void TestInconsistentCountdown()
        {
            var beatmaps = createBeatmapSetWithSettings(
                createSettings(countdown: CountdownType.Normal),
                createSettings(countdown: CountdownType.None)
            );
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentSettings.IssueTemplateInconsistentSetting);
            Assert.That(issues[0].ToString(), Contains.Substring("Countdown"));
            Assert.That(issues[0].ToString(), Contains.Substring("Normal"));
            Assert.That(issues[0].ToString(), Contains.Substring("None"));
        }

        [Test]
        public void TestInconsistentCountdownOffset()
        {
            var beatmaps = createBeatmapSetWithSettings(
                createSettings(countdownOffset: 100),
                createSettings(countdownOffset: 200)
            );
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentSettings.IssueTemplateInconsistentSetting);
            Assert.That(issues[0].ToString(), Contains.Substring("Countdown offset"));
        }

        [Test]
        public void TestInconsistentEpilepsyWarning()
        {
            var beatmaps = createBeatmapSetWithSettings(
                createSettings(epilepsyWarning: true),
                createSettings(epilepsyWarning: false)
            );
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentSettings.IssueTemplateInconsistentSetting);
            Assert.That(issues[0].ToString(), Contains.Substring("Epilepsy warning"));
        }

        [Test]
        public void TestInconsistentLetterboxInBreaks()
        {
            var beatmaps = createBeatmapSetWithSettings(
                createSettings(letterboxInBreaks: true),
                createSettings(letterboxInBreaks: false)
            );
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentSettings.IssueTemplateInconsistentSetting);
            Assert.That(issues[0].ToString(), Contains.Substring("Letterbox during breaks"));
        }

        [Test]
        public void TestInconsistentSamplesMatchPlaybackRate()
        {
            var beatmaps = createBeatmapSetWithSettings(
                createSettings(samplesMatchPlaybackRate: true),
                createSettings(samplesMatchPlaybackRate: false)
            );
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentSettings.IssueTemplateInconsistentSetting);
            Assert.That(issues[0].ToString(), Contains.Substring("Samples match playback rate"));
        }

        [Test]
        public void TestInconsistentWidescreenSupport()
        {
            var beatmaps = createBeatmapSetWithSettings(
                createSettings(widescreenStoryboard: true),
                createSettings(widescreenStoryboard: false)
            );
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            Assert.That(check.Run(context), Is.Empty);
        }

        [Test]
        public void TestInconsistentWidescreenSupportWithStoryboard()
        {
            var beatmaps = createBeatmapSetWithSettings(
                createSettings(widescreenStoryboard: true),
                createSettings(widescreenStoryboard: false)
            );
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps, hasStoryboard: true);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentSettings.IssueTemplateInconsistentSetting);
            Assert.That(issues[0].ToString(), Contains.Substring("Widescreen support"));
        }

        [Test]
        public void TestInconsistentSliderTickRate()
        {
            var beatmaps = createBeatmapSetWithSettings(
                createSettings(sliderTickRate: 1.0),
                createSettings(sliderTickRate: 2.0)
            );
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Template is CheckInconsistentSettings.IssueTemplateInconsistentSetting);
            Assert.That(issues[0].ToString(), Contains.Substring("Tick Rate"));
        }

        [Test]
        public void TestMultipleInconsistencies()
        {
            var beatmaps = createBeatmapSetWithSettings(
                createSettings(audioLeadIn: 1000, countdown: CountdownType.Normal, epilepsyWarning: false),
                createSettings(audioLeadIn: 2000, countdown: CountdownType.None, epilepsyWarning: true)
            );
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            var issues = check.Run(context).ToList();

            Assert.That(issues, Has.Count.EqualTo(3));
            Assert.That(issues.Count(i => i.ToString().Contains("Audio lead-in")), Is.EqualTo(1));
            Assert.That(issues.Count(i => i.ToString().Contains("Countdown")), Is.EqualTo(1));
            Assert.That(issues.Count(i => i.ToString().Contains("Epilepsy warning")), Is.EqualTo(1));
        }

        [Test]
        public void TestSingleDifficulty()
        {
            var beatmaps = createBeatmapSetWithSettings(
                createSettings(audioLeadIn: 1000)
            );
            var context = createContextWithMultipleDifficulties(beatmaps.First(), beatmaps);

            Assert.That(check.Run(context), Is.Empty);
        }

        private Beatmap createSettings(
            double audioLeadIn = 0,
            CountdownType countdown = CountdownType.None,
            int countdownOffset = 0,
            bool epilepsyWarning = false,
            bool letterboxInBreaks = false,
            bool samplesMatchPlaybackRate = false,
            bool widescreenStoryboard = false,
            double sliderTickRate = 1.0)
        {
            return new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    DifficultyName = "Test Difficulty",
                    StarRating = 5.0
                },
                AudioLeadIn = audioLeadIn,
                Countdown = countdown,
                CountdownOffset = countdownOffset,
                EpilepsyWarning = epilepsyWarning,
                LetterboxInBreaks = letterboxInBreaks,
                SamplesMatchPlaybackRate = samplesMatchPlaybackRate,
                WidescreenStoryboard = widescreenStoryboard,
                Difficulty = new BeatmapDifficulty
                {
                    SliderTickRate = sliderTickRate
                }
            };
        }

        private IBeatmap[] createBeatmapSetWithSettings(params IBeatmap[] beatmaps)
        {
            var beatmapSet = new BeatmapSetInfo();

            for (int i = 0; i < beatmaps.Length; i++)
            {
                beatmaps[i].BeatmapInfo.DifficultyName = $"Difficulty {i + 1}";
                beatmaps[i].BeatmapInfo.BeatmapSet = beatmapSet;
                beatmapSet.Beatmaps.Add(beatmaps[i].BeatmapInfo);
            }

            return beatmaps;
        }

        private BeatmapVerifierContext createContextWithMultipleDifficulties(IBeatmap currentBeatmap, IBeatmap[] allDifficulties, bool hasStoryboard = false)
        {
            Storyboard? storyboard = null;

            if (hasStoryboard)
            {
                storyboard = new Storyboard();
                storyboard.GetLayer("Background").Add(new StoryboardSprite("test.png", Anchor.Centre, Vector2.Zero));
            }

            var verifiedCurrentBeatmap = new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(currentBeatmap, storyboard), currentBeatmap);
            var verifiedOtherBeatmaps = allDifficulties.Select(b => new BeatmapVerifierContext.VerifiedBeatmap(new TestWorkingBeatmap(b, storyboard), b)).ToList();

            return new BeatmapVerifierContext(verifiedCurrentBeatmap, verifiedOtherBeatmaps, DifficultyRating.ExpertPlus);
        }
    }
}
