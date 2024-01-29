// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu
{
    public partial class OsuInputManager : RulesetInputManager<OsuAction>
    {
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
            get => ((OsuKeyBindingContainer)KeyBindingContainer).AllowGameplayInputs;
            set => ((OsuKeyBindingContainer)KeyBindingContainer).AllowGameplayInputs = value;
        }

        /// <summary>
        /// Whether the user's cursor movement events should be accepted.
        /// Can be used to block only movement while still accepting button input.
        /// </summary>
        public bool AllowUserCursorMovement { get; set; } = true;

        protected override KeyBindingContainer<OsuAction> CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new OsuKeyBindingContainer(ruleset, variant, unique);

        public bool CheckScreenSpaceActionPressJudgeable(Vector2 screenSpacePosition) =>
            // This is a very naive but simple approach.
            //
            // Based on user feedback of more nuanced scenarios (where touch doesn't behave as expected),
            // this can be expanded to a more complex implementation, but I'd still want to keep it as simple as we can.
            NonPositionalInputQueue.OfType<DrawableHitCircle.HitReceptor>().Any(c => c.ReceivePositionalInputAt(screenSpacePosition));

        public OsuInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new OsuTouchInputMapper(this) { RelativeSizeAxes = Axes.Both });
        }

        protected override bool Handle(UIEvent e)
        {
            if ((e is MouseMoveEvent || e is TouchMoveEvent) && !AllowUserCursorMovement) return false;

            return base.Handle(e);
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
                    KeyBindings = KeyBindings.Where(static b => b.GetAction<OsuAction>() == OsuAction.Smoke).ToList();
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
