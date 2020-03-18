// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Tests.Visual.Editor
{
    [TestFixture]
    public class TestSceneTimelineBlueprintContainer : TimelineTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(TimelineHitObjectBlueprint),
        };

        public override Drawable CreateTestComponent() => new TimelineBlueprintContainer();

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Clock.Seek(10000);
        }
    }
}
