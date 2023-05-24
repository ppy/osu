// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    public abstract partial class OsuSkinnableTestScene : SkinnableTestScene
    {
        private Container content;

        protected override Container<Drawable> Content
        {
            get
            {
                if (content == null)
                    base.Content.Add(content = new OsuInputManager(new OsuRuleset().RulesetInfo));

                return content;
            }
        }

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();
    }
}
