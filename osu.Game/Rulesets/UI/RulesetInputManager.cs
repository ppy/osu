// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.UI
{
    public abstract class RulesetInputManager<T> : DatabasedKeyBindingInputManager<T>, ICanAttachKeyCounter
        where T : struct
    {
        protected RulesetInputManager(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique) : base(ruleset, variant, unique)
        {
        }

        public void Attach(KeyCounterCollection keyCounter)
        {
            var receptor = new ActionReceptor(keyCounter);
            Add(receptor);
            keyCounter.SetReceptor(receptor);

            keyCounter.AddRange(DefaultKeyBindings.Select(b => b.GetAction<T>()).Distinct().Select(b => new KeyCounterAction<T>(b)));
        }

        public class ActionReceptor : KeyCounterCollection.Receptor, IKeyBindingHandler<T>
        {
            public ActionReceptor(KeyCounterCollection target)
                : base(target)
            {
            }

            public bool OnPressed(T action) => Target.Children.OfType<KeyCounterAction<T>>().Any(c => c.OnPressed(action));

            public bool OnReleased(T action) => Target.Children.OfType<KeyCounterAction<T>>().Any(c => c.OnReleased(action));
        }
    }

    public interface ICanAttachKeyCounter
    {
        void Attach(KeyCounterCollection keyCounter);
    }
}