// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Allocation;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using OpenTK;

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

        private readonly RestoreDefaultValueButton restoreDefaultButton;

        public bool ShowsDefaultIndicator = true;

        public virtual string LabelText
        {
            get { return text?.Text ?? string.Empty; }
            set
            {
                if (text == null)
                {
                    // construct lazily for cases where the label is not needed (may be provided by the Control).
                    Add(text = new OsuSpriteText());
                    FlowContent.SetLayoutPosition(text, -1);
                }

                text.Text = value;
            }
        }

        // hold a reference to the provided bindable so we don't have to in every settings section.
        private Bindable<T> bindable;

        public virtual Bindable<T> Bindable
        {
            get { return bindable; }

            set
            {
                bindable = value;
                controlWithCurrent?.Current.BindTo(bindable);
                if (ShowsDefaultIndicator)
                {
                    restoreDefaultButton.Bindable = bindable.GetBoundCopy();
                    restoreDefaultButton.Bindable.TriggerChange();
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

            InternalChildren = new Drawable[]
            {
                restoreDefaultButton = new RestoreDefaultValueButton(),
                FlowContent = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = SettingsOverlay.CONTENT_MARGINS },
                    Child = Control = CreateControl()
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (controlWithCurrent != null)
                controlWithCurrent.Current.DisabledChanged += disabled => { Colour = disabled ? Color4.Gray : Color4.White; };
        }

        private class RestoreDefaultValueButton : Container, IHasTooltip
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

            [BackgroundDependencyLoader]
            private void load(OsuColour colour)
            {
                buttonColour = colour.Yellow;

                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 3,
                    Masking = true,
                    Colour = buttonColour,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = buttonColour.Opacity(0.1f),
                        Type = EdgeEffectType.Glow,
                        Radius = 2,
                    },
                    Size = new Vector2(0.33f, 0.8f),
                    Child = new Box { RelativeSizeAxes = Axes.Both },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                UpdateState();
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
                return false;
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

                this.FadeTo(bindable.IsDefault ? 0f :
                    hovering && !bindable.Disabled ? 1f : 0.65f, 200, Easing.OutQuint);
                this.FadeColour(bindable.Disabled ? Color4.Gray : buttonColour, 200, Easing.OutQuint);
            }
        }
    }
}
