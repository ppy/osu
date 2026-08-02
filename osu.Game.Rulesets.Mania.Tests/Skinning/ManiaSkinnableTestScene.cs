// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    /// <summary>
    /// A test scene for skinnable mania components.
    /// </summary>
    public abstract partial class ManiaSkinnableTestScene : SkinnableTestScene
    {
        [Cached]
        private readonly StageDefinition stage = new StageDefinition(4);

        protected override Ruleset CreateRulesetForSkinProvider() => new ManiaRuleset();

        protected ManiaSkinnableTestScene()
        {
            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.SlateGray.Opacity(0.2f),
                Depth = 1
            });
        }
    }
}
