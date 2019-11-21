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

        private RulesetInfo ruleset;

        public RulesetInfo Ruleset
        {
            get => ruleset;
            set
            {
                if (ruleset?.Equals(value) ?? false)
                {
                    DeselectAll();
                    return;
                }

                ruleset = value;

                SelectedMods.Clear();
                modsContainer.Clear();

                if (ruleset == null)
                    return;

                modsContainer.Add(new ModButton(new ModNoMod()));
                modsContainer.AddRange(ruleset.CreateInstance().GetAllMods().Where(m => m.Ranked).Select(m => new ModButton(m)));

                modsContainer.ForEach(button => button.OnSelectionChanged = selectionChanged);
            }
        }

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

        private void selectionChanged(Mod mod, bool selected)
        {
            if (selected)
                SelectedMods.Add(mod);
            else
                SelectedMods.Remove(mod);

            if (!SelectedMods.Any() && !IsHovered)
                highlightAll();
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (!SelectedMods.Any())
                modsContainer.Children.Where(button => !button.IsHovered).ForEach(button => button.Highlighted.Value = false);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            if (!SelectedMods.Any())
                highlightAll();
        }

        public void DeselectAll() => modsContainer.ForEach(mod => mod.Selected.Value = false);

        private void highlightAll() => modsContainer.ForEach(mod => mod.Highlighted.Value = true);

        private class ModButton : ModIcon
        {
            private const int duration = 200;

            public readonly BindableBool Selected = new BindableBool();
            public Action<Mod, bool> OnSelectionChanged;

            public ModButton(Mod mod)
                : base(mod)
            {
                Scale = new Vector2(0.4f);
                Highlighted.Value = true;
                Add(new HoverClickSounds());
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Selected.BindValueChanged(selected =>
                {
                    updateState();
                    OnSelectionChanged?.Invoke(Mod, selected.NewValue);
                });
            }

            protected override bool OnClick(ClickEvent e)
            {
                Selected.Toggle();
                return true;
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return false;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                updateState();
            }

            private void updateState() => Highlighted.Value = IsHovered || Selected.Value;

            protected override void OnHighlightedChanged(ValueChangedEvent<bool> highlighted) =>
                this.FadeColour(highlighted.NewValue ? Color4.White : Color4.Gray, duration, Easing.OutQuint);
        }
    }
}
