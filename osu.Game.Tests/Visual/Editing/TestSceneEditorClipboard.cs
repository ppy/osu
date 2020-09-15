// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneEditorClipboard : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        [Test]
        public void TestCutRemovesObjects()
        {
            var addedObject = new HitCircle { StartTime = 1000 };

            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            AddStep("select added object", () => EditorBeatmap.SelectedHitObjects.Add(addedObject));

            AddStep("cut hitobject", () => Editor.Cut());

            AddAssert("no hitobjects in beatmap", () => EditorBeatmap.HitObjects.Count == 0);
        }

        [TestCase(1000)]
        [TestCase(2000)]
        public void TestCutPaste(double newTime)
        {
            var addedObject = new HitCircle { StartTime = 1000 };

            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            AddStep("select added object", () => EditorBeatmap.SelectedHitObjects.Add(addedObject));

            AddStep("cut hitobject", () => Editor.Cut());

            AddStep("move forward in time", () => EditorClock.Seek(newTime));

            AddStep("paste hitobject", () => Editor.Paste());

            AddAssert("is one object", () => EditorBeatmap.HitObjects.Count == 1);

            AddAssert("new object selected", () => EditorBeatmap.SelectedHitObjects.Single().StartTime == newTime);
        }

        [Test]
        public void TestCutPasteSlider()
        {
            var addedObject = new Slider
            {
                StartTime = 1000,
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100, 0), PathType.Bezier)
                    }
                }
            };

            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            AddStep("select added object", () => EditorBeatmap.SelectedHitObjects.Add(addedObject));

            AddStep("cut hitobject", () => Editor.Cut());

            AddStep("paste hitobject", () => Editor.Paste());

            AddAssert("is one object", () => EditorBeatmap.HitObjects.Count == 1);

            AddAssert("path matches", () =>
            {
                var path = ((Slider)EditorBeatmap.HitObjects.Single()).Path;
                return path.ControlPoints.Count == 2 && path.ControlPoints.SequenceEqual(addedObject.Path.ControlPoints);
            });
        }

        [Test]
        public void TestCutPasteSpinner()
        {
            var addedObject = new Spinner
            {
                StartTime = 1000,
                Duration = 5000
            };

            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            AddStep("select added object", () => EditorBeatmap.SelectedHitObjects.Add(addedObject));

            AddStep("cut hitobject", () => Editor.Cut());

            AddStep("paste hitobject", () => Editor.Paste());

            AddAssert("is one object", () => EditorBeatmap.HitObjects.Count == 1);

            AddAssert("duration matches", () => ((Spinner)EditorBeatmap.HitObjects.Single()).Duration == 5000);
        }

        [Test]
        public void TestCopyPaste()
        {
            var addedObject = new HitCircle { StartTime = 1000 };

            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            AddStep("select added object", () => EditorBeatmap.SelectedHitObjects.Add(addedObject));

            AddStep("copy hitobject", () => Editor.Copy());

            AddStep("move forward in time", () => EditorClock.Seek(2000));

            AddStep("paste hitobject", () => Editor.Paste());

            AddAssert("are two objects", () => EditorBeatmap.HitObjects.Count == 2);

            AddAssert("new object selected", () => EditorBeatmap.SelectedHitObjects.Single().StartTime == 2000);
        }

        [Test]
        public void TestCutNothing()
        {
            AddStep("cut hitobject", () => Editor.Cut());
            AddAssert("are no objects", () => EditorBeatmap.HitObjects.Count == 0);
        }

        [Test]
        public void TestCopyNothing()
        {
            AddStep("copy hitobject", () => Editor.Copy());
            AddAssert("are no objects", () => EditorBeatmap.HitObjects.Count == 0);
        }

        [Test]
        public void TestPasteNothing()
        {
            AddStep("paste hitobject", () => Editor.Paste());
            AddAssert("are no objects", () => EditorBeatmap.HitObjects.Count == 0);
        }
    }
}
