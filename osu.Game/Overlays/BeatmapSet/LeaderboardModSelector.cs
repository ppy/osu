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

namespace osu.Game.Overlays.BeatmapSet
{
    public class LeaderboardModSelector : Container
    {
        public Bindable<IEnumerable<Mod>> SelectedMods = new Bindable<IEnumerable<Mod>>();

        private RulesetInfo ruleset = new RulesetInfo();
        private readonly FillFlowContainer<SelectableModIcon> modsContainer;

        public LeaderboardModSelector()
        {
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            Child = modsContainer = new FillFlowContainer<SelectableModIcon>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
            };
        }

        public void ResetRuleset(RulesetInfo ruleset)
        {
            SelectedMods.Value = new List<Mod>();

            if (ruleset == null)
            {
                this.ruleset = ruleset;
                modsContainer.Clear();
                return;
            }

            if (this.ruleset?.Equals(ruleset) ?? false)
            {
                deselectAll();
                return;
            }

            this.ruleset = ruleset;

            modsContainer.Clear();

            foreach (var mod in ruleset.CreateInstance().GetAllMods())
            {
                if (mod.Ranked)
                    modsContainer.Add(new SelectableModIcon(mod));
            }

            foreach (var mod in modsContainer)
                mod.OnSelectionChanged += selectionChanged;
        }

        private void selectionChanged(Mod mod, bool selected)
        {
            var mods = SelectedMods.Value?.ToList() ?? new List<Mod>();

            if (selected)
                mods.Add(mod);
            else
                mods.Remove(mod);

            SelectedMods.Value = mods;
        }

        private void deselectAll()
        {
            foreach (var mod in modsContainer)
                mod.Selected.Value = false;
        }

        private class SelectableModIcon : Container
        {
            private const float modScale = 0.4f;
            private const int duration = 200;

            public readonly BindableBool Selected = new BindableBool(false);
            public Action<Mod, bool> OnSelectionChanged;

            private readonly ModIcon modIcon;
            private readonly Mod mod;

            public SelectableModIcon(Mod mod)
            {
                this.mod = mod;

                Size = new Vector2(40);
                Children = new Drawable[]
                {
                    modIcon = new ModIcon(mod)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Scale = new Vector2(modScale),
                    },
                    new HoverClickSounds()
                };

                Selected.BindValueChanged(_ => updateState());
                Selected.TriggerChange();
            }

            protected override bool OnClick(ClickEvent e)
            {
                Selected.Value = !Selected.Value;
                OnSelectionChanged?.Invoke(mod, Selected.Value);

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

            private void updateState() => modIcon.FadeColour(Selected.Value || IsHovered ? Color4.White : Color4.Gray, duration, Easing.OutQuint);
        }
    }
}
