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
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Settings
{
    public abstract class SettingsItem<T> : Container, IFilterable, ISettingsItem, IHasCurrentValue<T>, IHasTooltip
    {
        protected abstract Drawable CreateControl();

        protected Drawable Control { get; }

        private IHasCurrentValue<T> controlWithCurrent => Control as IHasCurrentValue<T>;

        protected override Container<Drawable> Content => FlowContent;

        protected readonly FillFlowContainer FlowContent;

        private SpriteText labelText;

        private OsuTextFlowContainer warningText;

        public bool ShowsDefaultIndicator = true;

        public string TooltipText { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        public virtual LocalisableString LabelText
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

        /// <summary>
        /// Text to be displayed at the bottom of this <see cref="SettingsItem{T}"/>.
        /// Generally used to recommend the user change their setting as the current one is considered sub-optimal.
        /// </summary>
        public string WarningText
        {
            set
            {
                if (warningText == null)
                {
                    // construct lazily for cases where the label is not needed (may be provided by the Control).
                    FlowContent.Add(warningText = new OsuTextFlowContainer
                    {
                        Colour = colours.Yellow,
                        Margin = new MarginPadding { Bottom = 5 },
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    });
                }

                warningText.Alpha = string.IsNullOrWhiteSpace(value) ? 0 : 1;
                warningText.Text = value;
            }
        }

        public virtual Bindable<T> Current
        {
            get => controlWithCurrent.Current;
            set => controlWithCurrent.Current = value;
        }

        public virtual IEnumerable<string> FilterTerms => Keywords == null ? new[] { LabelText.ToString() } : new List<string>(Keywords) { LabelText.ToString() }.ToArray();

        public IEnumerable<string> Keywords { get; set; }

        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1 : 0);
        }

        public bool FilteringActive { get; set; }

        public event Action SettingChanged;

        protected SettingsItem()
        {
            RestoreDefaultValueButton restoreDefaultButton;

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
                    Children = new[]
                    {
                        Control = CreateControl(),
                    },
                },
            };

            // all bindable logic is in constructor intentionally to support "CreateSettingsControls" being used in a context it is
            // never loaded, but requires bindable storage.
            if (controlWithCurrent != null)
            {
                controlWithCurrent.Current.ValueChanged += _ => SettingChanged?.Invoke();
                controlWithCurrent.Current.DisabledChanged += _ => updateDisabled();

                if (ShowsDefaultIndicator)
                    restoreDefaultButton.Bindable = controlWithCurrent.Current;
            }
        }

        private void updateDisabled()
        {
            if (labelText != null)
                labelText.Alpha = controlWithCurrent.Current.Disabled ? 0.3f : 1;
        }

        protected internal class RestoreDefaultValueButton : Container, IHasTooltip
        {
            public override bool IsPresent => base.IsPresent || Scheduler.HasPendingTasks;

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

            public RestoreDefaultValueButton()
            {
                RelativeSizeAxes = Axes.Y;
                Width = SettingsPanel.CONTENT_MARGINS;
                Padding = new MarginPadding { Vertical = 1.5f };
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
                    Width = 0.33f,
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

            public void UpdateState() => Scheduler.AddOnce(updateState);

            private void updateState()
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
