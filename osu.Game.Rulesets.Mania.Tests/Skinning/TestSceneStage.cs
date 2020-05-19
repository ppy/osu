// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.UI;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public class TestSceneStage : ManiaSkinnableTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            SetContents(() =>
            {
                ManiaAction normalAction = ManiaAction.Key1;
                ManiaAction specialAction = ManiaAction.Special1;

                return new ManiaInputManager(new ManiaRuleset().RulesetInfo, 4)
                {
                    Child = new Stage(0, new StageDefinition { Columns = 4 }, ref normalAction, ref specialAction)
                };
            });
        }
    }
}
