﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.SearchableList;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Direct
{
    public class FilterControl : SearchableListFilterControl<DirectSortCriteria, RankStatus>
    {
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();
        private FillFlowContainer<RulesetToggleButton> modeButtons;

        protected override Color4 BackgroundColour => OsuColour.FromHex(@"384552");
        protected override DirectSortCriteria DefaultTab => DirectSortCriteria.Ranked;
        protected override Drawable CreateSupplementaryControls()
        {
            modeButtons = new FillFlowContainer<RulesetToggleButton>
            {
                AutoSizeAxes = Axes.Both,
                Spacing = new Vector2(10f, 0f),
            };

            return modeButtons;
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuGame game, RulesetStore rulesets, OsuColour colours)
        {
            DisplayStyleControl.Dropdown.AccentColour = colours.BlueDark;

            Ruleset.BindTo(game?.Ruleset ?? new Bindable<RulesetInfo> { Value = rulesets.GetRuleset(0) });
            foreach (var r in rulesets.AvailableRulesets)
            {
                modeButtons.Add(new RulesetToggleButton(Ruleset, r));
            }
        }

        private class RulesetToggleButton : OsuClickableContainer
        {
            private Drawable icon
            {
                get { return iconContainer.Icon; }
                set { iconContainer.Icon = value; }
            }

            private RulesetInfo ruleset;
            public RulesetInfo Ruleset
            {
                get { return ruleset; }
                set
                {
                    ruleset = value;
                    icon = Ruleset.CreateInstance().CreateIcon();
                }
            }

            private readonly Bindable<RulesetInfo> bindable;

            private readonly ConstrainedIconContainer iconContainer;

            private void Bindable_ValueChanged(RulesetInfo obj)
            {
                iconContainer.FadeTo(Ruleset.ID == obj?.ID ? 1f : 0.5f, 100);
            }

            public RulesetToggleButton(Bindable<RulesetInfo> bindable, RulesetInfo ruleset)
            {
                this.bindable = bindable;
                AutoSizeAxes = Axes.Both;

                Children = new[]
                {
                    iconContainer = new ConstrainedIconContainer
                    {
                        Origin = Anchor.TopLeft,
                        Anchor = Anchor.TopLeft,
                        Size = new Vector2(32),
                    }
                };

                Ruleset = ruleset;
                bindable.ValueChanged += Bindable_ValueChanged;
                Bindable_ValueChanged(bindable.Value);
                Action = () => bindable.Value = Ruleset;
            }

            protected override void Dispose(bool isDisposing)
            {
                if (bindable != null)
                    bindable.ValueChanged -= Bindable_ValueChanged;
                base.Dispose(isDisposing);
            }
        }
    }

    public enum DirectSortCriteria
    {
        Title,
        Artist,
        Creator,
        Difficulty,
        Ranked,
        Rating,
        Plays,
    }
}
