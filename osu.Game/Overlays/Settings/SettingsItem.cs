// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
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

        private readonly RestoreDefaultValueButton restoreDefaultValueButton = new RestoreDefaultValueButton();

        public bool ShowsDefaultIndicator = true;

        private Color4? restoreDefaultValueColour;

        public Color4 RestoreDefaultValueColour
        {
            get { return restoreDefaultValueColour ?? Color4.White; }
            set
            {
                restoreDefaultValueColour = value;
                restoreDefaultValueButton?.SetButtonColour(RestoreDefaultValueColour);
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
                    restoreDefaultValueButton.Bindable = bindable.GetBoundCopy();
                    restoreDefaultValueButton.Bindable.TriggerChange();
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

            if (restoreDefaultValueButton != null)
            {
                if (!restoreDefaultValueColour.HasValue)
                    restoreDefaultValueColour = colours.Yellow;
                restoreDefaultValueButton.SetButtonColour(RestoreDefaultValueColour);
                AddInternal(restoreDefaultValueButton);
            }
        }

        private class RestoreDefaultValueButton : Box, IHasTooltip
        {
            private Bindable<T> bindable;
            public Bindable<T> Bindable
            {
                get { return bindable; }
                set
                {
                    bindable = value;
                    bindable.ValueChanged += newValue => UpdateState();
                    bindable.DisabledChanged += disabled => UpdateState();
                }
            }

            private Color4 buttonColour;

            private bool hovering;

            public RestoreDefaultValueButton()
            {
                RelativeSizeAxes = Axes.Y;
                Width = SettingsOverlay.CONTENT_MARGINS;
                Alpha = 0f;
            }

            public string TooltipText => "Revert to default";

            protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

            protected override bool OnMouseUp(InputState state, MouseUpEventArgs args) => true;

            protected override bool OnClick(InputState state)
            {
                if (bindable != null && !bindable.Disabled)
                    bindable.SetDefault();
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

            public void SetButtonColour(Color4 buttonColour)
            {
                this.buttonColour = buttonColour;
                UpdateState();
            }

            public void UpdateState()
            {
                if (bindable == null)
                    return;
                var colour = bindable.Disabled ? Color4.Gray : buttonColour;
                this.FadeTo(bindable.IsDefault ? 0f : hovering && !bindable.Disabled ? 1f : 0.5f, 200, Easing.OutQuint);
                this.FadeColour(ColourInfo.GradientHorizontal(colour.Opacity(0.8f), colour.Opacity(0)), 200, Easing.OutQuint);
            }
        }
    }
}
