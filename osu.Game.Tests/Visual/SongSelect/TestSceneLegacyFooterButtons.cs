// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osu.Game.Skinning.Select;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneLegacyFooterButtons : OsuTestScene
    {
        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            const float ruleset_width = 57.6f * 1.6f;
            const float button_width = 48f * 1.6f;

            Child = new SkinProvidingContainer(skins.DefaultClassicSkin)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.None,
                AutoSizeAxes = Axes.Both,
                Child = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        new LegacyRulesetFooterButton(),
                        new LegacyFooterButton("mods") { X = ruleset_width },
                        new LegacyFooterButton("random") { X = ruleset_width + button_width },
                        new LegacyFooterButton("options") { X = ruleset_width + button_width * 2 },
                    }
                },
            };
        });
    }
}
