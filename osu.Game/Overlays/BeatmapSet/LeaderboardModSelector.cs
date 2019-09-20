// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Framework.Bindables;
using System.Collections.Generic;
using osu.Game.Rulesets;
using osuTK;
using osu.Game.Rulesets.UI;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;
using System;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.BeatmapSet
{
    public class LeaderboardModSelector : CompositeDrawable
    {
        public readonly Bindable<IEnumerable<Mod>> SelectedMods = new Bindable<IEnumerable<Mod>>();
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
            SelectedMods.Value = new List<Mod>();

            modsContainer.Clear();

            if (ruleset.NewValue == null)
                return;

            modsContainer.Add(new ModButton(new NoMod()));

            ruleset.NewValue.CreateInstance().GetAllMods().ForEach(mod =>
            {
                if (mod.Ranked)
                    modsContainer.Add(new ModButton(mod));
            });

            modsContainer.ForEach(button => button.OnSelectionChanged += selectionChanged);
        }

        private void selectionChanged(Mod mod, bool selected)
        {
            var mods = SelectedMods.Value.ToList();

            if (selected)
                mods.Add(mod);
            else
                mods.Remove(mod);

            SelectedMods.Value = mods;

            if (!mods.Any() && !IsHovered)
                highlightAll();
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (!SelectedMods.Value.Any())
                modsContainer.ForEach(button =>
                {
                    if (!button.IsHovered)
                        button.Highlighted.Value = false;
                });

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            if (!SelectedMods.Value.Any())
                highlightAll();
        }

        public void DeselectAll() => modsContainer.ForEach(mod => mod.Selected.Value = false);

        private void highlightAll() => modsContainer.ForEach(mod => mod.Highlighted.Value = true);

        private class ModButton : ModIcon
        {
            private const float mod_scale = 0.4f;
            private const int duration = 200;

            public readonly BindableBool Selected = new BindableBool();
            public Action<Mod, bool> OnSelectionChanged;

            public ModButton(Mod mod)
                : base(mod)
            {
                Scale = new Vector2(mod_scale);
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
                Selected.Value = !Selected.Value;
                return base.OnClick(e);
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return base.OnHover(e);
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

        private class NoMod : Mod
        {
            public override string Name => "NoMod";

            public override string Acronym => "NM";

            public override double ScoreMultiplier => 1;

            public override IconUsage Icon => FontAwesome.Solid.Ban;

            public override ModType Type => ModType.System;
        }
    }
}
