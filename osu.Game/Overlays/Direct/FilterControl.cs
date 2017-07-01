// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays.SearchableList;

namespace osu.Game.Overlays.Direct
{
    public class FilterControl : SearchableListFilterControl<DirectSortCritera, RankStatus>
    {
        private FillFlowContainer<RulesetToggleButton> modeButtons;

        protected override Color4 BackgroundColour => OsuColour.FromHex(@"384552");
        protected override DirectSortCritera DefaultTab => DirectSortCritera.Title;
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
        private void load(OsuGame game, RulesetDatabase rulesets, OsuColour colours)
        {
            DisplayStyleControl.Dropdown.AccentColour = colours.BlueDark;

            var b = new Bindable<RulesetInfo>(); //backup bindable incase the game is null
            foreach (var r in rulesets.AllRulesets)
            {
                modeButtons.Add(new RulesetToggleButton(game?.Ruleset ?? b, r));
            }
        }

        private class RulesetToggleButton : OsuClickableContainer
        {
            private readonly TextAwesome icon;

            private RulesetInfo ruleset;
            public RulesetInfo Ruleset
            {
                get { return ruleset; }
                set
                {
                    ruleset = value;
                    icon.Icon = Ruleset.CreateInstance().Icon;
                }
            }

            private readonly Bindable<RulesetInfo> bindable;

            private void Bindable_ValueChanged(RulesetInfo obj)
            {
                icon.FadeTo(Ruleset.ID == obj?.ID ? 1f : 0.5f, 100);
            }

            public RulesetToggleButton(Bindable<RulesetInfo> bindable, RulesetInfo ruleset)
            {
                this.bindable = bindable;
                AutoSizeAxes = Axes.Both;

                Children = new[]
                {
                    icon = new TextAwesome
                    {
                        Origin = Anchor.TopLeft,
                        Anchor = Anchor.TopLeft,
                        TextSize = 32,
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

    public enum DirectSortCritera
    {
        Title,
        Artist,
        Creator,
        Difficulty,
        Ranked,
        Rating,
    }
}
