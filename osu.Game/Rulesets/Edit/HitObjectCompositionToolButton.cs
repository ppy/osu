// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Screens.Edit.Components.RadioButtons;

namespace osu.Game.Rulesets.Edit
{
    public abstract partial class HitObjectCompositionToolButton : EditorRadioButton
    {
        public CompositionTool Tool { get; }

        protected HitObjectCompositionToolButton(CompositionTool tool, Action<CompositionTool>? action)
            : base(tool.Name, () => action?.Invoke(tool), tool.CreateIcon)
        {
            Tool = tool;

            Selected.BindDisabledChanged(isDisabled =>
            {
                TooltipText = isDisabled ? "Add at least one timing point first!" : Tool.TooltipText;
            }, true);
        }
    }

    public partial class HitObjectCompositionToolButton<TAction> : HitObjectCompositionToolButton, IKeyBindingHandler<TAction>
        where TAction : struct, Enum
    {
        public new CompositionTool<TAction> Tool => (CompositionTool<TAction>)base.Tool;

        public HitObjectCompositionToolButton(CompositionTool<TAction> tool, Action<CompositionTool>? action)
            : base(tool, action)
        {
        }

        public bool OnPressed(KeyBindingPressEvent<TAction> e)
        {
            if (Nullable.Equals(e.Action, Tool.Action) && Enabled.Value)
            {
                Select();
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<TAction> e)
        {
        }
    }
}
