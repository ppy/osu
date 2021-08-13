// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public class TestSceneTimelineBlueprintContainer : TimelineTestScene
    {
        public override Drawable CreateTestComponent() => new TimelineBlueprintContainer(Composer);

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Clock.Seek(10000);
        }
    }
}
