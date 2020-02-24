// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Home.Friends;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneUserSortTabControl : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(UserSortTabControl),
            typeof(OverlaySortTabControl<>),
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        public TestSceneUserSortTabControl()
        {
            UserSortTabControl control;
            OsuSpriteText current;

            Add(control = new UserSortTabControl
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            Add(current = new OsuSpriteText());

            control.Current.BindValueChanged(criteria => current.Text = $"Criteria: {criteria.NewValue}", true);
        }
    }
}
