// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
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

        private const int duration = 100;

        protected void UpdateVisibility()
        {
            bool visible = AlwaysVisible.Value || ConfigVisibility.Value;

            // Isolate changing visibility of the key counters from fading this component.
            KeyFlow.FadeTo(visible ? 1 : 0, duration);

            // Ensure a valid size is immediately obtained even if partially off-screen
            // See https://github.com/ppy/osu/issues/14793.
            KeyFlow.AlwaysPresent = visible;
        }

        protected abstract KeyCounter CreateCounter(InputTrigger trigger);

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, DrawableRuleset? drawableRuleset)
        {
            AutoSizeAxes = Axes.Both;

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
