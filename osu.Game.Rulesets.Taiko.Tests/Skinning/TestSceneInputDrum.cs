// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Taiko.UI;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public partial class TestSceneInputDrum : TaikoSkinnableTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            var playfield = new TaikoPlayfield();

            var beatmap = CreateWorkingBeatmap(new TaikoRuleset().RulesetInfo).GetPlayableBeatmap(new TaikoRuleset().RulesetInfo);

            foreach (var h in beatmap.HitObjects)
                playfield.Add(h);

            SetContents(_ => new TaikoInputManager(new TaikoRuleset().RulesetInfo)
            {
                RelativeSizeAxes = Axes.Both,
                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(200),
                    Child = new InputDrum()
                }
            });
        }
    }
}
