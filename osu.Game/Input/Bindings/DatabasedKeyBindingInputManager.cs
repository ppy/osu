using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets;

namespace osu.Game.Input.Bindings
{
    /// <summary>
    /// A KeyBindingInputManager with a database backing for custom overrides.
    /// </summary>
    /// <typeparam name="T">The type of the custom action.</typeparam>
    public abstract class DatabasedKeyBindingInputManager<T> : KeyBindingInputManager<T>
        where T : struct
    {
        private readonly RulesetInfo ruleset;

        private readonly int? variant;

        private BindingStore store;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="ruleset">A reference to identify the current <see cref="Ruleset"/>. Used to lookup mappings. Null for global mappings.</param>
        /// <param name="variant">An optional variant for the specified <see cref="Ruleset"/>. Used when a ruleset has more than one possible keyboard layouts.</param>
        /// <param name="concurrencyMode">Specify how to deal with multiple matches of <see cref="KeyCombination"/>s and <see cref="T"/>s.</param>
        protected DatabasedKeyBindingInputManager(RulesetInfo ruleset = null, int? variant = null, ConcurrentActionMode concurrencyMode = ConcurrentActionMode.None)
            : base(concurrencyMode)
        {
            this.ruleset = ruleset;
            this.variant = variant;
        }

        [BackgroundDependencyLoader]
        private void load(BindingStore bindings)
        {
            store = bindings;
        }

        protected override void ReloadMappings()
        {
            // load defaults
            base.ReloadMappings();

            var rulesetId = ruleset?.ID;

            // load from database if present.
            if (store != null)
            {
                foreach (var b in store.Query<DatabasedKeyBinding>(b => b.RulesetID == rulesetId && b.Variant == variant))
                    Mappings.Add(b);
            }
        }
    }
}