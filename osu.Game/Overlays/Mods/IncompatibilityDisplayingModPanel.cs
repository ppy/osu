// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Overlays.Mods
{
    public class IncompatibilityDisplayingModPanel : ModPanel, IHasCustomTooltip<Mod>
    {
        private readonly BindableBool incompatible = new BindableBool();

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> selectedMods { get; set; }

        public IncompatibilityDisplayingModPanel(Mod mod)
            : base(mod)
        {
        }

        protected override void LoadComplete()
        {
            selectedMods.BindValueChanged(_ => updateIncompatibility(), true);
            incompatible.BindValueChanged(_ => Scheduler.AddOnce(UpdateState));
            // base call will run `UpdateState()` first time and finish transforms.
            base.LoadComplete();
        }

        private void updateIncompatibility()
        {
            incompatible.Value = selectedMods.Value.Count > 0 && !selectedMods.Value.Contains(Mod) && !ModUtils.CheckCompatibleSet(selectedMods.Value.Append(Mod));
        }

        protected override void UpdateState()
        {
            Action = incompatible.Value ? () => { } : (Action)Active.Toggle;

            if (incompatible.Value)
            {
                Colour4 backgroundColour = ColourProvider.Background5;
                Colour4 textBackgroundColour = ColourProvider.Background4;

                Content.TransformTo(nameof(BorderColour), ColourInfo.GradientVertical(backgroundColour, textBackgroundColour), TRANSITION_DURATION, Easing.OutQuint);
                Background.FadeColour(backgroundColour, TRANSITION_DURATION, Easing.OutQuint);

                SwitchContainer.ResizeWidthTo(IDLE_SWITCH_WIDTH, TRANSITION_DURATION, Easing.OutQuint);
                SwitchContainer.FadeColour(Colour4.Gray, TRANSITION_DURATION, Easing.OutQuint);
                MainContentContainer.TransformTo(nameof(Padding), new MarginPadding
                {
                    Left = IDLE_SWITCH_WIDTH,
                    Right = CORNER_RADIUS
                }, TRANSITION_DURATION, Easing.OutQuint);

                TextBackground.FadeColour(textBackgroundColour, TRANSITION_DURATION, Easing.OutQuint);
                TextFlow.FadeColour(Colour4.White.Opacity(0.5f), TRANSITION_DURATION, Easing.OutQuint);
                return;
            }

            SwitchContainer.FadeColour(Colour4.White, TRANSITION_DURATION, Easing.OutQuint);
            base.UpdateState();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (incompatible.Value)
                return true; // bypasses base call purposely in order to not play out the intermediate state animation.

            return base.OnMouseDown(e);
        }

        #region IHasCustomTooltip

        public ITooltip<Mod> GetCustomTooltip() => new IncompatibilityDisplayingTooltip();

        public Mod TooltipContent => Mod;

        #endregion
    }
}
