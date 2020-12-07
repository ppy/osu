// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Mods
{
    public class ModSettingsContainer : VisibilityContainer
    {
        public readonly IBindable<IReadOnlyList<Mod>> SelectedMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        public IBindable<bool> HasSettingsForSelection => hasSettingsForSelection;

        private readonly Bindable<bool> hasSettingsForSelection = new Bindable<bool>();

        private readonly FillFlowContainer<ModControlSection> modSettingsContent;

        private readonly Container content;

        private const double transition_duration = 400;

        public ModSettingsContainer()
        {
            RelativeSizeAxes = Axes.Both;

            Child = content = new Container
            {
                Masking = true,
                CornerRadius = 10,
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Both,
                X = 1,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new Color4(0, 0, 0, 192)
                    },
                    new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = modSettingsContent = new FillFlowContainer<ModControlSection>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(0f, 10f),
                            Padding = new MarginPadding(20),
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedMods.BindValueChanged(modsChanged, true);
        }

        private void modsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            modSettingsContent.Clear();

            foreach (var mod in mods.NewValue)
            {
                var settings = mod.CreateSettingsControls().ToList();
                if (settings.Count > 0)
                    modSettingsContent.Add(new ModControlSection(mod, settings));
            }

            bool hasSettings = modSettingsContent.Count > 0;

            if (!hasSettings)
                Hide();

            hasSettingsForSelection.Value = hasSettings;
        }

        protected override bool OnMouseDown(MouseDownEvent e) => true;
        protected override bool OnHover(HoverEvent e) => true;

        protected override void PopIn()
        {
            this.FadeIn(transition_duration, Easing.OutQuint);
            content.MoveToX(0, transition_duration, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(transition_duration, Easing.OutQuint);
            content.MoveToX(1, transition_duration, Easing.OutQuint);
        }
    }
}
