// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests
{
    public abstract class ManiaInputTestCase : OsuTestCase
    {
        private readonly Container<Drawable> content;
        protected override Container<Drawable> Content => content ?? base.Content;

        protected ManiaInputTestCase(int keys)
        {
            base.Content.Add(content = new LocalInputManager(keys));
        }

        private class LocalInputManager : ManiaInputManager
        {
            public LocalInputManager(int variant)
                : base(new ManiaRuleset().RulesetInfo, variant)
            {
            }

            protected override RulesetKeyBindingContainer CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                => new LocalKeyBindingContainer(ruleset, variant, unique);

            private class LocalKeyBindingContainer : RulesetKeyBindingContainer
            {
                public LocalKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                    : base(ruleset, variant, unique)
                {
                }

                protected override void ReloadMappings()
                {
                    KeyBindings = DefaultKeyBindings;
                }
            }
        }
    }
}
