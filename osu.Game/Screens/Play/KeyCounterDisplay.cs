// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osuTK;

namespace osu.Game.Screens.Play
{
    public abstract partial class KeyCounterDisplay : Container<KeyCounter>
    {
        protected readonly Bindable<bool> ConfigVisibility = new Bindable<bool>();

        protected FillFlowContainer<KeyCounter> KeyFlow = new FillFlowContainer<KeyCounter>();

        protected override Container<KeyCounter> Content => KeyFlow;

        /// <summary>
        /// Whether the key counter should be visible regardless of the configuration value.
        /// This is true by default, but can be changed.
        /// </summary>
        public readonly Bindable<bool> AlwaysVisible = new Bindable<bool>(true);

        protected override void Update()
        {
            base.Update();

            // Don't use autosize as it will shrink to zero when KeyFlow is hidden.
            // In turn this can cause the display to be masked off screen and never become visible again.
            Size = KeyFlow.Size;
        }

        public override void Add(KeyCounter key)
        {
            if (!CheckType(key))
                throw new ArgumentException($"{key.GetType()} is not a supported {nameof(KeyCounter)}.", nameof(key));

            base.Add(key);
            key.IsCounting = IsCounting;
        }

        protected virtual bool CheckType(KeyCounter key) => true;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.KeyOverlay, ConfigVisibility);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AlwaysVisible.BindValueChanged(_ => UpdateVisibility());
            ConfigVisibility.BindValueChanged(_ => UpdateVisibility(), true);
        }

        private bool isCounting = true;

        public bool IsCounting
        {
            get => isCounting;
            set
            {
                if (value == isCounting) return;

                isCounting = value;
                foreach (var child in Children)
                    child.IsCounting = value;
            }
        }

        protected abstract void UpdateVisibility();

        public override bool HandleNonPositionalInput => receptor == null;
        public override bool HandlePositionalInput => receptor == null;

        private Receptor? receptor;

        public void SetReceptor(Receptor receptor)
        {
            if (this.receptor != null)
                throw new InvalidOperationException("Cannot set a new receptor when one is already active");

            this.receptor = receptor;
        }

        public virtual KeyCounter CreateKeyCounter(KeyCounter.InputTrigger trigger) => new DefaultKeyCounter(trigger);

        public partial class Receptor : Drawable
        {
            protected readonly KeyCounterDisplay Target;

            public Receptor(KeyCounterDisplay target)
            {
                RelativeSizeAxes = Axes.Both;
                Depth = float.MinValue;
                Target = target;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            protected override bool Handle(UIEvent e)
            {
                switch (e)
                {
                    case KeyDownEvent:
                    case KeyUpEvent:
                    case MouseDownEvent:
                    case MouseUpEvent:
                        return Target.Children.Any(c => c.TriggerEvent(e));
                }

                return base.Handle(e);
            }
        }
    }
}
