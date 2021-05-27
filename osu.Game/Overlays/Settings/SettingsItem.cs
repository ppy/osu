// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
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
            RestoreDefaultValueButton<T> restoreDefaultButton;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Right = SettingsPanel.CONTENT_MARGINS };

            InternalChildren = new Drawable[]
            {
                restoreDefaultButton = new RestoreDefaultValueButton<T>(),
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
                    restoreDefaultButton.Current = controlWithCurrent.Current;
            }
        }

        private void updateDisabled()
        {
            if (labelText != null)
                labelText.Alpha = controlWithCurrent.Current.Disabled ? 0.3f : 1;
        }
    }
}