// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.Settings
{
    public abstract partial class SettingsItem<T> : Container, IConditionalFilterable, ISettingsItem, IHasCurrentValue<T>, IHasTooltip
    {
        protected abstract Drawable CreateControl();

        protected Drawable Control { get; }

        /// <summary>
        /// The source component if this <see cref="SettingsItem{T}"/> was created via <see cref="SettingSourceAttribute"/>.
        /// </summary>
        public object SettingSourceObject { get; internal set; }

        public const string CLASSIC_DEFAULT_SEARCH_TERM = @"has-classic-default";

        private IHasCurrentValue<T> controlWithCurrent => Control as IHasCurrentValue<T>;

        protected override Container<Drawable> Content => FlowContent;

        protected readonly FillFlowContainer FlowContent;

        private SpriteText labelText;

        private OsuTextFlowContainer noticeText;

        public bool ShowsDefaultIndicator = true;
        private readonly Container defaultValueIndicatorContainer;

        public LocalisableString TooltipText { get; set; }

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
                updateLayout();
            }
        }

        /// <summary>
        /// Clear any warning text.
        /// </summary>
        public void ClearNoticeText()
        {
            noticeText?.Expire();
            noticeText = null;
        }

        /// <summary>
        /// Set the text to be displayed at the bottom of this <see cref="SettingsItem{T}"/>.
        /// Generally used to provide feedback to a user about a sub-optimal setting.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="isWarning">Whether the text is in a warning state. Will decide how this is visually represented.</param>
        public void SetNoticeText(LocalisableString text, bool isWarning = false)
        {
            ClearNoticeText();

            // construct lazily for cases where the label is not needed (may be provided by the Control).
            FlowContent.Add(noticeText = new LinkFlowContainer(cp => cp.Colour = isWarning ? colours.Yellow : colours.Green)
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Margin = new MarginPadding { Bottom = 5 },
                Text = text,
            });
        }

        public virtual Bindable<T> Current
        {
            get => controlWithCurrent.Current;
            set => controlWithCurrent.Current = value;
        }

        public virtual IEnumerable<LocalisableString> FilterTerms
        {
            get
            {
                var keywords = new List<LocalisableString>(Keywords?.Select(k => (LocalisableString)k) ?? Array.Empty<LocalisableString>())
                {
                    LabelText
                };

                if (HasClassicDefault)
                    keywords.Add(CLASSIC_DEFAULT_SEARCH_TERM);

                return keywords;
            }
        }

        public IEnumerable<string> Keywords { get; set; }

        private bool matchingFilter = true;

        public bool MatchingFilter
        {
            get => matchingFilter;
            set
            {
                bool wasPresent = IsPresent;

                matchingFilter = value;

                if (IsPresent != wasPresent)
                    Invalidate(Invalidation.Presence);
            }
        }

        public override bool IsPresent => base.IsPresent && MatchingFilter;

        public bool FilteringActive { get; set; }

        public BindableBool CanBeShown { get; } = new BindableBool(true);
        IBindable<bool> IConditionalFilterable.CanBeShown => CanBeShown;

        public event Action SettingChanged;

        private T classicDefault;

        public bool HasClassicDefault { get; private set; }

        /// <summary>
        /// A "classic" default value for this setting.
        /// </summary>
        public T ClassicDefault
        {
            set
            {
                classicDefault = value;
                HasClassicDefault = true;
            }
        }

        public void ApplyClassicDefault()
        {
            if (!HasClassicDefault)
                throw new InvalidOperationException($"Cannot apply a classic default to a setting which doesn't have one defined via {nameof(ClassicDefault)}.");

            Current.Value = classicDefault;
        }

        public void ApplyDefault() => Current.SetDefault();

        protected SettingsItem()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Padding = new MarginPadding { Right = SettingsPanel.CONTENT_MARGINS };

            InternalChildren = new Drawable[]
            {
                defaultValueIndicatorContainer = new Container
                {
                    Width = SettingsPanel.CONTENT_MARGINS,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Left = SettingsPanel.CONTENT_MARGINS },
                    Child = FlowContent = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0, 5),
                        Child = Control = CreateControl(),
                    }
                }
            };

            // IMPORTANT: all bindable logic is in constructor intentionally to support "CreateSettingsControls" being used in a context it is
            // never loaded, but requires bindable storage.
            if (controlWithCurrent == null)
                throw new ArgumentException(@$"Control created via {nameof(CreateControl)} must implement {nameof(IHasCurrentValue<T>)}");

            controlWithCurrent.Current.ValueChanged += _ => SettingChanged?.Invoke();
            controlWithCurrent.Current.DisabledChanged += _ => updateDisabled();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // intentionally done before LoadComplete to avoid overhead.
            if (ShowsDefaultIndicator)
            {
                defaultValueIndicatorContainer.Add(new RevertToDefaultButton<T>
                {
                    Current = controlWithCurrent.Current,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                });
                updateLayout();
            }
        }

        private void updateLayout()
        {
            bool hasLabel = labelText != null && !string.IsNullOrEmpty(labelText.Text.ToString());

            // if the settings item is providing a label, the default value indicator should be centred vertically to the left of the label.
            // otherwise, it should be centred vertically to the left of the main control of the settings item.
            defaultValueIndicatorContainer.Height = hasLabel ? labelText.DrawHeight : Control.DrawHeight;
        }

        private void updateDisabled()
        {
            if (labelText != null)
                labelText.Alpha = controlWithCurrent.Current.Disabled ? 0.3f : 1;
        }
    }
}
