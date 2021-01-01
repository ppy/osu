// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
    public abstract class SettingsItem<T> : Container, IFilterable, ISettingsItem, IHasCurrentValue<T>
    {
        protected abstract Drawable CreateControl();

        protected Drawable Control { get; }

        private IHasCurrentValue<T> controlWithCurrent => Control as IHasCurrentValue<T>;

        protected override Container<Drawable> Content => FlowContent;

        protected readonly FillFlowContainer FlowContent;

        private SpriteText labelText;

        private bool showsDefualtIndicator;

        public virtual bool ShowsDefaultIndicator
        {
            get => showsDefualtIndicator;
            set
            {
                showsDefualtIndicator = value;
                restoreDefaultButton.UpdateState();
            }
        }

        public virtual string LabelText
        {
            get => labelText?.Text ?? string.Empty;
            set
            {
                if (labelText == null)
                {
                    // construct lazily for cases where the label is not needed (may be provided by the Control).
                    FlowContent.Insert(-1, labelText = new OsuSpriteText());

                    updateDisabled();
                }

                labelText.Text = value;
            }
        }

        [Obsolete("Use Current instead")] // Can be removed 20210406
        public Bindable<T> Bindable
        {
            get => Current;
            set => Current = value;
        }

        public virtual Bindable<T> Current
        {
            get => controlWithCurrent.Current;
            set => controlWithCurrent.Current = value;
        }

        public virtual IEnumerable<string> FilterTerms => Keywords == null ? new[] { LabelText } : new List<string>(Keywords) { LabelText }.ToArray();

        public IEnumerable<string> Keywords { get; set; }

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }

        public event Action SettingChanged;

        private readonly RestoreDefaultValueButton restoreDefaultButton;

        protected SettingsItem()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Right = SettingsPanel.CONTENT_MARGINS };

            InternalChildren = new Drawable[]
            {
                restoreDefaultButton = new RestoreDefaultValueButton(this),
                FlowContent = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                    Child = Control = CreateControl()
                },
            };

            // all bindable logic is in constructor intentionally to support "CreateSettingsControls" being used in a context it is
            // never loaded, but requires bindable storage.
            if (controlWithCurrent != null)
            {
                controlWithCurrent.Current.ValueChanged += _ => SettingChanged?.Invoke();
                controlWithCurrent.Current.DisabledChanged += _ => updateDisabled();
                restoreDefaultButton.Bindable = controlWithCurrent.Current;
            }
        }

        private void updateDisabled()
        {
            if (labelText != null)
                labelText.Alpha = controlWithCurrent.Current.Disabled ? 0.3f : 1;
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
                    bindable.DefaultChanged += _ => UpdateState();
                    UpdateState();
                }
            }

            private Color4 buttonColour;

            private bool hovering;

            private readonly SettingsItem<T> item;

            public RestoreDefaultValueButton(SettingsItem<T> item)
            {
                this.item = item;

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

            public string TooltipText => "revert to default";

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

            public void UpdateState()
            {
                if (bindable == null)
                    return;

                if (bindable.IsDefault || !item.ShowsDefaultIndicator)
                    this.FadeOut(200, Easing.OutQuint);
                else
                    this.FadeTo(hovering && !bindable.Disabled ? 1f : 0.65f, 200, Easing.OutQuint);

                this.FadeColour(bindable.Disabled ? Color4.Gray : buttonColour, 200, Easing.OutQuint);
            }
        }
    }
}
