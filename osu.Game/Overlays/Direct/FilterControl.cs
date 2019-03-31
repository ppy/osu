// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.SearchableList;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Direct
{
    public class FilterControl : SearchableListFilterControl<DirectSortCriteria, BeatmapSearchCategory>
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
        private void load(RulesetStore rulesets, OsuColour colours, Bindable<RulesetInfo> ruleset)
        {
            DisplayStyleControl.Dropdown.AccentColour = colours.BlueDark;

            Ruleset.Value = ruleset.Value ?? rulesets.GetRuleset(0);
            foreach (var r in rulesets.AvailableRulesets)
                modeButtons.Add(new RulesetToggleButton(Ruleset, r));
        }

        private class RulesetToggleButton : OsuClickableContainer
        {
            private Drawable icon
            {
                get => iconContainer.Icon;
                set => iconContainer.Icon = value;
            }

            private RulesetInfo ruleset;

            public RulesetInfo Ruleset
            {
                get => ruleset;
                set
                {
                    ruleset = value;
                    icon = Ruleset.CreateInstance().CreateIcon();
                }
            }

            private readonly Bindable<RulesetInfo> bindable;

            private readonly ConstrainedIconContainer iconContainer;

            private void Bindable_ValueChanged(ValueChangedEvent<RulesetInfo> e)
            {
                iconContainer.FadeTo(Ruleset.ID == e.NewValue?.ID ? 1f : 0.5f, 100);
            }

            public override bool HandleNonPositionalInput => !bindable.Disabled && base.HandleNonPositionalInput;
            public override bool HandlePositionalInput => !bindable.Disabled && base.HandlePositionalInput;

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
                Bindable_ValueChanged(new ValueChangedEvent<RulesetInfo>(bindable.Value, bindable.Value));
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
        Relevance,
        Title,
        Artist,
        Creator,
        Difficulty,
        Ranked,
        Rating,
        Plays,
        Favourites,
    }
}
