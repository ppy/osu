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
                defaultIndicator.Colour = ColourInfo.GradientHorizontal(colours.Yellow.Opacity(0.8f), colours.Yellow.Opacity(0));
                defaultIndicator.Alpha = 0f;
                AddInternal(defaultIndicator);
            }
        }

        private class SettingsItemDefaultIndicator<T> : Box
        {
            internal readonly Bindable<T> Bindable = new Bindable<T>();

            private bool hovering;

            public SettingsItemDefaultIndicator()
            {
                Bindable.ValueChanged += value => updateAlpha();

                RelativeSizeAxes = Axes.Y;
                Width = SettingsOverlay.CONTENT_MARGINS;
                Alpha = 0f;
            }

            public override bool HandleInput => true;

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args) => true;

            protected override bool OnClick(InputState state)
            {
                Bindable.SetDefault();
                return true;
            }

            protected override bool OnHover(InputState state)
            {
                hovering = true;
                updateAlpha();
                return true;
            }

            protected override void OnHoverLost(InputState state)
            {
                hovering = false;
                updateAlpha();
            }

            private void updateAlpha() =>
                Alpha = Bindable.IsDefault ? 0f : hovering ? 1f : 0.5f;
        }
    }
}
