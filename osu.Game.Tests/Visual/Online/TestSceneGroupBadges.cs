// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Profile.Header.Components;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneGroupBadges : OsuTestScene
    {
        public TestSceneGroupBadges()
        {
            var groups = new[]
            {
                new APIUser(),
                new APIUser
                {
                    Groups = new[]
                    {
                        new APIUserGroup { Colour = "#EB47D0", ShortName = "DEV", Name = "Developers" },
                    }
                },
                new APIUser
                {
                    Groups = new[]
                    {
                        new APIUserGroup { Colour = "#EB47D0", ShortName = "DEV", Name = "Developers" },
                        new APIUserGroup { Colour = "#A347EB", ShortName = "BN", Name = "Beatmap Nominators", Playmodes = new[] { "osu", "taiko" } }
                    }
                },
                new APIUser
                {
                    Groups = new[]
                    {
                        new APIUserGroup { Colour = "#0066FF", ShortName = "PPY", Name = "peppy" },
                        new APIUserGroup { Colour = "#EB47D0", ShortName = "DEV", Name = "Developers" },
                        new APIUserGroup { Colour = "#A347EB", ShortName = "BN", Name = "Beatmap Nominators", Playmodes = new[] { "osu", "taiko" } }
                    }
                },
                new APIUser
                {
                    Groups = new[]
                    {
                        new APIUserGroup { Colour = "#0066FF", ShortName = "PPY", Name = "peppy" },
                        new APIUserGroup { Colour = "#EB47D0", ShortName = "DEV", Name = "Developers" },
                        new APIUserGroup { Colour = "#999999", ShortName = "ALM", Name = "osu! Alumni" },
                        new APIUserGroup { Colour = "#A347EB", ShortName = "BN", Name = "Beatmap Nominators", Playmodes = new[] { "osu", "taiko" } },
                        new APIUserGroup { Colour = "#A347EB", ShortName = "BN", Name = "Beatmap Nominators (Probationary)", Playmodes = new[] { "osu", "taiko" }, IsProbationary = true }
                    }
                }
            };

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.DarkGray
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(40),
                    Children = new[]
                    {
                        new FillFlowContainer<GroupBadgeFlow>
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(5),
                            ChildrenEnumerable = groups.Select(g => new GroupBadgeFlow { User = { Value = g } })
                        },
                    }
                }
            };
        }
    }
}
