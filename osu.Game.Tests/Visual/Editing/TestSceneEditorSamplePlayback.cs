// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneEditorSamplePlayback : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestSlidingSampleStopsOnSeek()
        {
            DrawableSlider slider = null;
            SkinnableSample[] loopingSamples = null;
            SkinnableSample[] onceOffSamples = null;

            AddStep("get first slider", () =>
            {
                slider = Editor.ChildrenOfType<DrawableSlider>().OrderBy(s => s.HitObject.StartTime).First();
                onceOffSamples = slider.ChildrenOfType<SkinnableSample>().Where(s => !s.Looping).ToArray();
                loopingSamples = slider.ChildrenOfType<SkinnableSample>().Where(s => s.Looping).ToArray();
            });

            AddStep("start playback", () => EditorClock.Start());

            AddUntilStep("wait for slider sliding then seek", () =>
            {
                if (!slider.Tracking.Value)
                    return false;

                if (!loopingSamples.Any(s => s.Playing))
                    return false;

                EditorClock.Seek(20000);
                return true;
            });

            AddAssert("non-looping samples are playing", () => onceOffSamples.Length == 4 && loopingSamples.All(s => s.Played || s.Playing));
            AddAssert("looping samples are not playing", () => loopingSamples.Length == 1 && loopingSamples.All(s => s.Played && !s.Playing));
        }
    }
}
