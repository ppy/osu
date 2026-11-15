// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneTimelineBlueprintContainer : TimelineTestScene
    {
        public override Drawable CreateTestComponent() => new TimelineBlueprintContainer(Composer);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            EditorClock.Seek(2000);

            AddStep("add effect points", () =>
            {
                EditorBeatmap.ControlPointInfo.Add(3000, new EffectControlPoint { KiaiMode = true });
                EditorBeatmap.ControlPointInfo.Add(4400, new EffectControlPoint());
                EditorBeatmap.ControlPointInfo.Add(5000, new EffectControlPoint { KiaiMode = true });
                TimelineArea.Timeline.Zoom = 1f;
            });
        }
    }
}
