// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Overlays.Profile.Sections;
using System;
using System.Collections.Generic;
using osu.Framework.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneShowMoreButton : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ShowMoreButton),
        };

        public TestSceneShowMoreButton()
        {
            ShowMoreButton button;

            Add(button = new ShowMoreButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Action = () => { }
            });

            AddStep("switch loading state", () => button.IsLoading = !button.IsLoading);
        }
    }
}
