// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// A flowing display of all gameplay keys. Individual keys can be added using <see cref="InputTrigger"/> implementations.
    /// </summary>
    public abstract partial class KeyCounterDisplay : CompositeDrawable, IAttachableSkinComponent, ISerialisableDrawable
    {
        /// <summary>
        /// Whether the key counter should be visible regardless of the configuration value.
        /// This is true by default, but can be changed.
        /// </summary>
        public Bindable<bool> AlwaysVisible { get; } = new Bindable<bool>(true);

        /// <summary>
        /// The <see cref="KeyCounter"/>s contained in this <see cref="KeyCounterDisplay"/>.
        /// </summary>
        public IEnumerable<KeyCounter> Counters => KeyFlow;

        protected abstract FillFlowContainer<KeyCounter> KeyFlow { get; }

        protected readonly Bindable<bool> ConfigVisibility = new Bindable<bool>();

        [Resolved]
        private KeyCounterController controller { get; set; } = null!;

        protected abstract void UpdateVisibility();

        /// <summary>
        /// Add a <see cref="InputTrigger"/> to this display.
        /// </summary>
        public void Add(InputTrigger trigger)
        {
            var keyCounter = CreateCounter(trigger);

            KeyFlow.Add(keyCounter);
        }

        /// <summary>
        /// Add a range of <see cref="InputTrigger"/> to this display.
        /// </summary>
        public void AddRange(IEnumerable<InputTrigger> triggers) => triggers.ForEach(Add);

        protected abstract KeyCounter CreateCounter(InputTrigger trigger);

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, DrawableRuleset? drawableRuleset)
        {
            config.BindWith(OsuSetting.KeyOverlay, ConfigVisibility);

            if (drawableRuleset != null)
                AlwaysVisible.BindTo(drawableRuleset.HasReplayLoaded);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            controller.OnNewTrigger += Add;
            AddRange(controller.Triggers);

            AlwaysVisible.BindValueChanged(_ => UpdateVisibility());
            ConfigVisibility.BindValueChanged(_ => UpdateVisibility(), true);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            controller.OnNewTrigger -= Add;
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
