// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets;

namespace osu.Game.Skinning.Select
{
    public partial class LegacyRulesetFooterButton : LegacyFooterButton
    {
        private Sprite modeIcon = null!;

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        public LegacyRulesetFooterButton()
            : base("mode")
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(modeIcon = new Sprite
            {
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.Centre,
                X = 57.6f / 2 * 1.6f,
                Y = -35 * 1.6f,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(r =>
            {
                modeIcon.Texture = skin.GetTexture($@"mode-{r.NewValue.ShortName}-small");
            }, true);
        }
    }
}
