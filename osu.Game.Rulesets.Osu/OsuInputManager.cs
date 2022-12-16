// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using System.Linq;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Framework.Input.StateChanges.Events;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Osu
{
    public partial class OsuInputManager : RulesetInputManager<OsuAction>
    {
        private readonly OsuTouchInputMapper touchInputMapper;

        public IEnumerable<OsuAction> PressedActions => KeyBindingContainer.PressedActions;

        /// <summary>
        /// Whether gameplay input buttons should be allowed.
        /// Defaults to <c>true</c>, generally used for mods like Relax which turn off main inputs.
        /// </summary>
        /// <remarks>
        /// Of note, auxiliary inputs like the "smoke" key are left usable.
        /// </remarks>
        public bool AllowGameplayInputs
        {
            set => ((OsuKeyBindingContainer)KeyBindingContainer).AllowGameplayInputs = value;
        }

        /// <summary>
        /// Whether the user's cursor movement events should be accepted.
        /// Can be used to block only movement while still accepting button input.
        /// </summary>
        public bool AllowUserCursorMovement { get; set; } = true;

        protected override KeyBindingContainer<OsuAction> CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new OsuKeyBindingContainer(ruleset, variant, unique);

        public OsuInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
            touchInputMapper = new OsuTouchInputMapper(this) { RelativeSizeAxes = Axes.Both };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(touchInputMapper);
        }

        protected override bool Handle(UIEvent e)
        {
            if (e is MouseMoveEvent or TouchMoveEvent && !AllowUserCursorMovement) return false;

            return base.Handle(e);
        }

        protected override bool HandleMouseTouchStateChange(TouchStateChangeEvent e) =>
            touchInputMapper.IsTapTouch(e.Touch.Source) || base.HandleMouseTouchStateChange(e);

        public void EnableStreamMode(TouchSource cursorSource)
        {
            var touchPosition = CurrentState.Mouse.Position;

            var cursorTouch = new Touch(cursorSource, touchPosition);
            var cursorInput = new TouchInput(cursorTouch, false);

            // Enables stream mode by disabling mouse input from the touch cursor.
            var streamModeEvent = new TouchStateChangeEvent(CurrentState, cursorInput, cursorTouch, false, touchPosition);

            base.HandleMouseTouchStateChange(streamModeEvent);
        }

        private partial class OsuKeyBindingContainer : RulesetKeyBindingContainer
        {
            private bool allowGameplayInputs = true;

            /// <summary>
            /// Whether gameplay input buttons should be allowed.
            /// Defaults to <c>true</c>, generally used for mods like Relax which turn off main inputs.
            /// </summary>
            /// <remarks>
            /// Of note, auxiliary inputs like the "smoke" key are left usable.
            /// </remarks>
            public bool AllowGameplayInputs
            {
                get => allowGameplayInputs;
                set
                {
                    allowGameplayInputs = value;
                    ReloadMappings();
                }
            }

            public OsuKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                : base(ruleset, variant, unique)
            {
            }

            protected override void ReloadMappings(IQueryable<RealmKeyBinding> realmKeyBindings)
            {
                base.ReloadMappings(realmKeyBindings);

                if (!AllowGameplayInputs)
                    KeyBindings = KeyBindings.Where(b => b.GetAction<OsuAction>() == OsuAction.Smoke).ToList();
            }
        }
    }

    public enum OsuAction
    {
        [Description("Left button")]
        LeftButton,

        [Description("Right button")]
        RightButton,

        [Description("Smoke")]
        Smoke,
    }
}
