// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

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
            get => text?.Text ?? string.Empty;
            set
            {
                if (text == null)
                {
                    // construct lazily for cases where the label is not needed (may be provided by the Control).
                    FlowContent.Insert(-1, text = new OsuSpriteText());
                }

                text.Text = value;
            }
        }

        // hold a reference to the provided bindable so we don't have to in every settings section.
        private Bindable<T> bindable;

        public virtual Bindable<T> Bindable
        {
            get => bindable;

            set
            {
                if (bindable != null)
                    controlWithCurrent?.Current.UnbindFrom(bindable);

                bindable = value;
                controlWithCurrent?.Current.BindTo(bindable);

                if (ShowsDefaultIndicator)
                {
                    restoreDefaultButton.Bindable = bindable.GetBoundCopy();
                    restoreDefaultButton.Bindable.TriggerChange();
                }
            }
        }

        public virtual IEnumerable<string> FilterTerms => new[] { LabelText };

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }

        protected SettingsItem()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Right = SettingsPanel.CONTENT_MARGINS };

            InternalChildren = new Drawable[]
            {
                restoreDefaultButton = new RestoreDefaultValueButton(),
                FlowContent = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
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
                get => bindable;
                set
                {
                    bindable = value;
                    bindable.ValueChanged += _ => UpdateState();
                    bindable.DisabledChanged += _ => UpdateState();
                }
            }

            private Color4 buttonColour;

            private bool hovering;

            public RestoreDefaultValueButton()
            {
                RelativeSizeAxes = Axes.Y;
                Width = SettingsPanel.CONTENT_MARGINS;
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

            protected override bool OnMouseDown(MouseDownEvent e) => true;

            protected override bool OnMouseUp(MouseUpEvent e) => true;

            protected override bool OnClick(ClickEvent e)
            {
                if (bindable != null && !bindable.Disabled)
                    bindable.SetDefault();
                return true;
            }

            protected override bool OnHover(HoverEvent e)
            {
                hovering = true;
                UpdateState();
                return false;
            }

            protected override void OnHoverLost(HoverLostEvent e)
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
