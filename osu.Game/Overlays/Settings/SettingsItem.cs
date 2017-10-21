// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Settings
{
    public abstract class SettingsItem<T> : Container, IFilterable
    {
        protected abstract Drawable CreateControl();

        protected Drawable Control { get; }

        private IHasCurrentValue<T> controlWithCurrent => Control as IHasCurrentValue<T>;

        protected override Container<Drawable> Content => FlowContent;

        protected readonly FillFlowContainer FlowContent;

        private SpriteText text;

        private readonly SettingsItemDefaultIndicator<T> defaultIndicator = new SettingsItemDefaultIndicator<T>();

        public bool ShowsDefaultIndicator = true;

        private Color4? defaultIndicatorColour;

        public Color4 DefaultIndicatorColour
        {
            get { return defaultIndicatorColour ?? Color4.White; }
            set
            {
                defaultIndicatorColour = value;
                defaultIndicator?.SetIndicatorColour(DefaultIndicatorColour);
            }
        }

        public virtual string LabelText
        {
            get { return text?.Text ?? string.Empty; }
            set
            {
                if (text == null)
                {
                    // construct lazily for cases where the label is not needed (may be provided by the Control).
                    Add(text = new OsuSpriteText { Depth = 1 });
                }

                text.Text = value;
            }
        }

        // hold a reference to the provided bindable so we don't have to in every settings section.
        private Bindable<T> bindable;

        public virtual Bindable<T> Bindable
        {
            get
            {
                return bindable;
            }

            set
            {
                bindable = value;
                controlWithCurrent?.Current.BindTo(bindable);
                if (ShowsDefaultIndicator)
                {
                    defaultIndicator.Bindable.BindTo(bindable);
                    defaultIndicator.Bindable.TriggerChange();
                }
            }
        }

        public IEnumerable<string> FilterTerms => new[] { LabelText };

        public bool MatchingFilter
        {
            set
            {
                // probably needs a better transition.
                this.FadeTo(value ? 1 : 0);
            }
        }

        protected SettingsItem()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Right = SettingsOverlay.CONTENT_MARGINS };

            FlowContent = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding { Left = SettingsOverlay.CONTENT_MARGINS, Right = 5 },
            };

            if ((Control = CreateControl()) != null)
            {
                if (controlWithCurrent != null)
                    controlWithCurrent.Current.DisabledChanged += disabled => { Colour = disabled ? Color4.Gray : Color4.White; };
                FlowContent.Add(Control);
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddInternal(FlowContent);

            if (defaultIndicator != null)
            {
                if (!defaultIndicatorColour.HasValue)
                    defaultIndicatorColour = colours.Yellow;
                defaultIndicator.SetIndicatorColour(DefaultIndicatorColour);
                AddInternal(defaultIndicator);
            }
        }

        private class SettingsItemDefaultIndicator<T> : Box
        {
            internal readonly Bindable<T> Bindable = new Bindable<T>();

            private Color4 indicatorColour;

            private bool hovering;

            public SettingsItemDefaultIndicator()
            {
                Bindable.ValueChanged += value => UpdateState();
                Bindable.DisabledChanged += disabled => UpdateState();

                RelativeSizeAxes = Axes.Y;
                Width = SettingsOverlay.CONTENT_MARGINS;
                Alpha = 0f;
            }

            public override bool HandleInput => true;

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args) => true;

            protected override bool OnClick(InputState state)
            {
                if (!Bindable.Disabled)
                    Bindable.SetDefault();
                return true;
            }

            protected override bool OnHover(InputState state)
            {
                hovering = true;
                UpdateState();
                return true;
            }

            protected override void OnHoverLost(InputState state)
            {
                hovering = false;
                UpdateState();
            }

            internal void SetIndicatorColour(Color4 indicatorColour)
            {
                this.indicatorColour = indicatorColour;
                UpdateState();
            }

            internal void UpdateState()
            {
                var colour = Bindable.Disabled ? Color4.Gray : indicatorColour;
                Alpha = Bindable.IsDefault ? 0f : (hovering && !Bindable.Disabled) ? 1f : 0.5f;
                Colour = ColourInfo.GradientHorizontal(colour.Opacity(0.8f), colour.Opacity(0));
            }
        }
    }
}
