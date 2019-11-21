// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Framework.Bindables;
using osu.Game.Rulesets;
using osuTK;
using osu.Game.Rulesets.UI;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;
using System;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;

namespace osu.Game.Overlays.BeatmapSet
{
    public class LeaderboardModSelector : CompositeDrawable
    {
        public readonly BindableList<Mod> SelectedMods = new BindableList<Mod>();
        public readonly Bindable<RulesetInfo> Ruleset = new Bindable<RulesetInfo>();

        private readonly FillFlowContainer<ModButton> modsContainer;

        public LeaderboardModSelector()
        {
            AutoSizeAxes = Axes.Both;
            InternalChild = modsContainer = new FillFlowContainer<ModButton>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Full,
                Spacing = new Vector2(4),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Ruleset.BindValueChanged(onRulesetChanged, true);
        }

        private void onRulesetChanged(ValueChangedEvent<RulesetInfo> ruleset)
        {
            SelectedMods.Clear();
            modsContainer.Clear();

            if (ruleset.NewValue == null)
                return;

            modsContainer.Add(new ModButton(new ModNoMod()));
            modsContainer.AddRange(ruleset.NewValue.CreateInstance().GetAllMods().Where(m => m.Ranked).Select(m => new ModButton(m)));

            modsContainer.ForEach(button => button.OnSelectionChanged = selectionChanged);
        }

        private void selectionChanged(Mod mod, bool selected)
        {
            if (selected)
                SelectedMods.Add(mod);
            else
                SelectedMods.Remove(mod);

            updateHighlighted();
        }

        private void updateHighlighted()
        {
            if (SelectedMods.Any())
                return;

            modsContainer.Children.Where(button => !button.IsHovered).ForEach(button => button.IsActive.Value = !IsHovered);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateHighlighted();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateHighlighted();
        }

        public void DeselectAll() => modsContainer.ForEach(mod => mod.Highlighted.Value = false);

        private class ModButton : ModIcon
        {
            private const int duration = 200;

            public readonly BindableBool IsActive = new BindableBool();
            public Action<Mod, bool> OnSelectionChanged;

            public ModButton(Mod mod)
                : base(mod)
            {
                Scale = new Vector2(0.4f);
                Add(new HoverClickSounds());
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                IsActive.BindValueChanged(hovered =>
                {
                    if (Highlighted.Value)
                        return;

                    this.FadeColour(hovered.NewValue ? Color4.White : Color4.DimGray, duration, Easing.OutQuint);
                }, true);
            }

            protected override void OnHighlightedChanged(ValueChangedEvent<bool> highlighted)
            {
                base.OnHighlightedChanged(highlighted);
                OnSelectionChanged?.Invoke(Mod, highlighted.NewValue);
                IsActive.TriggerChange();
            }

            protected override bool OnClick(ClickEvent e)
            {
                Highlighted.Toggle();
                return true;
            }

            protected override bool OnHover(HoverEvent e)
            {
                IsActive.Value = true;
                return false;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                IsActive.Value = false;
            }
        }
    }
}
