// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Audio;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneEditorSamplePlayback : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestSlidingSampleStopsOnSeek()
        {
            DrawableSlider slider = null;
            DrawableSample[] samples = null;

            AddStep("get first slider", () =>
            {
                slider = Editor.ChildrenOfType<DrawableSlider>().OrderBy(s => s.HitObject.StartTime).First();
                samples = slider.ChildrenOfType<DrawableSample>().ToArray();
            });

            AddStep("start playback", () => EditorClock.Start());

            AddUntilStep("wait for slider sliding then seek", () =>
            {
                if (!slider.Tracking.Value)
                    return false;

                if (!samples.Any(s => s.Playing))
                    return false;

                EditorClock.Seek(20000);
                return true;
            });

            AddAssert("slider samples are not playing", () => samples.Length == 5 && samples.All(s => s.Played && !s.Playing));
        }
    }
}
