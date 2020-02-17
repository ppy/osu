// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using System.Linq;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko
{
    public class TaikoInputManager : RulesetInputManager<TaikoAction>
    {
        protected override RulesetKeyBindingContainer CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
            => new TaikoKeyBindingContainer(ruleset, variant, unique);

        public TaikoAction? BlockedRim
        {
            get => ((TaikoKeyBindingContainer)KeyBindingContainer).BlockedRim;
            set => ((TaikoKeyBindingContainer)KeyBindingContainer).BlockedRim = value;
        }

        public TaikoAction? BlockedCentre
        {
            get => ((TaikoKeyBindingContainer)KeyBindingContainer).BlockedCentre;
            set => ((TaikoKeyBindingContainer)KeyBindingContainer).BlockedCentre = value;
        }

        public int BlockedKeystrokes => ((TaikoKeyBindingContainer)KeyBindingContainer).BlockedKeystrokes;

        public TaikoAction? LastRim => ((TaikoKeyBindingContainer)KeyBindingContainer).LastRim;

        public TaikoAction? LastCentre => ((TaikoKeyBindingContainer)KeyBindingContainer).LastCentre;

        public TaikoInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }

        public class TaikoKeyBindingContainer : RulesetKeyBindingContainer
        {
            public TaikoAction? BlockedRim;
            public TaikoAction? BlockedCentre;
            public int BlockedKeystrokes;

            public TaikoAction? LastRim;
            public TaikoAction? LastCentre;

            public TaikoKeyBindingContainer(RulesetInfo info, int variant, SimultaneousBindingMode unique)
                : base(info, variant, unique)
            {
            }

            protected override bool Handle(UIEvent e)
            {
                if (e is KeyDownEvent ev)
                {
                    var pressedCombination = KeyCombination.FromInputState(e.CurrentState);
                    var combos = KeyBindings.ToList().FindAll(m => m.KeyCombination.IsPressed(pressedCombination, KeyCombinationMatchingMode.Any));

                    var rims = combos.FindAll(c => c.GetAction<TaikoAction>() == TaikoAction.LeftRim || c.GetAction<TaikoAction>() == TaikoAction.RightRim);
                    var centres = combos.FindAll(c => c.GetAction<TaikoAction>() == TaikoAction.LeftCentre || c.GetAction<TaikoAction>() == TaikoAction.RightCentre);

                    var rimActions = rims.Select(c => c.GetAction<TaikoAction>());
                    var centreActions = centres.Select(c => c.GetAction<TaikoAction>());

                    bool bothLeft = (rimActions.Count() == 1 && rimActions.First() == TaikoAction.LeftRim) && (centreActions.Count() == 1 && centreActions.First() == TaikoAction.LeftCentre);
                    bool bothRight = (rimActions.Count() == 1 && rimActions.First() == TaikoAction.RightRim) && (centreActions.Count() == 1 && centreActions.First() == TaikoAction.RightCentre);

                    bool bothRim = rimActions.Count() == 2 && !centreActions.Any();
                    bool bothCentre = centreActions.Count() == 2 && !rimActions.Any();

                    if (rims.Count == 1)
                        LastRim = rims.First().GetAction<TaikoAction>();

                    if (centres.Count == 1)
                        LastCentre = centres.First().GetAction<TaikoAction>();

                    if (bothRim)
                    {
                        LastRim = null;
                        BlockedRim = null;
                        return base.Handle(e);
                    }

                    if (bothCentre)
                    {
                        LastCentre = null;
                        BlockedCentre = null;
                        return base.Handle(e);
                    }

                    if (bothLeft || bothRight)
                    {
                        LastRim = null;
                        BlockedRim = null;
                        LastCentre = null;
                        BlockedCentre = null;
                        return base.Handle(e);
                    }

                    var single = combos.Find(c => c.KeyCombination.Keys.Any(k => k == KeyCombination.FromKey(ev.Key)))?.GetAction<TaikoAction>();

                    if (single != null && (single == BlockedRim || single == BlockedCentre))
                    {
                        if (!ev.Repeat)
                            BlockedKeystrokes++;

                        return false;
                    }
                }

                return base.Handle(e);
            }
        }
    }

    public enum TaikoAction
    {
        [Description("Left (rim)")]
        LeftRim,

        [Description("Left (centre)")]
        LeftCentre,

        [Description("Right (centre)")]
        RightCentre,

        [Description("Right (rim)")]
        RightRim
    }
}
