// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Rulesets.UI;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// A flowing display of all gameplay keys. Individual keys can be added using <see cref="InputTrigger"/> implementations.
    /// </summary>
    public abstract partial class KeyCounterDisplay : CompositeDrawable, ISerialisableDrawable
    {
        /// <summary>
        /// Whether the key counter should be visible regardless of the configuration value.
        /// This is true by default, but can be changed.
        /// </summary>
        public Bindable<bool> AlwaysVisible { get; } = new Bindable<bool>(true);

        protected abstract FillFlowContainer<KeyCounter> KeyFlow { get; }

        protected readonly Bindable<bool> ConfigVisibility = new Bindable<bool>();

        private readonly IBindableList<InputTrigger> triggers = new BindableList<InputTrigger>();

        [Resolved]
        private InputCountController controller { get; set; } = null!;

        protected abstract void UpdateVisibility();

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

            triggers.BindTo(controller.Triggers);
            triggers.BindCollectionChanged(triggersChanged, true);

            AlwaysVisible.BindValueChanged(_ => UpdateVisibility());
            ConfigVisibility.BindValueChanged(_ => UpdateVisibility(), true);
        }

        private void triggersChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            KeyFlow.Clear();
            foreach (var trigger in controller.Triggers)
                KeyFlow.Add(CreateCounter(trigger));
        }

        public bool UsesFixedAnchor { get; set; }
    }
}
