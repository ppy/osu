// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osuTK;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.SelectV2
{
    public partial class CircleSizeSelector : CompositeDrawable
    {
        private readonly Bindable<string> keyModeId = new Bindable<string>("All");
        private readonly BindableBool isMultiSelectMode = new BindableBool();
        private readonly Dictionary<int, HashSet<string>> modeSelections = new Dictionary<int, HashSet<string>>();

        private ShearedCsModeTabControl tabControl = null!;
        private ShearedToggleButton multiSelectButton = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        public IBindable<string> Current => tabControl.Current;

        public CircleSizeFilter CircleSizeFilter { get; } = new CircleSizeFilter();

        public CircleSizeSelector()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            CornerRadius = 8;
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Shear = -OsuGame.SHEAR,
                    RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute),
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new[]
                        {
                            tabControl = new ShearedCsModeTabControl
                            {
                                RelativeSizeAxes = Axes.X,
                            },
                            Empty(),
                            multiSelectButton = new ShearedToggleButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Multi",
                                Height = 30f,
                            }
                        }
                    }
                }
            };

            multiSelectButton.Active.BindTo(isMultiSelectMode);

            keyModeId.BindTo(config.GetBindable<string>(OsuSetting.SongSelectCircleSizeMode));
            keyModeId.BindValueChanged(onSelectorChanged, true);

            isMultiSelectMode.BindValueChanged(_ => updateValue(), true);
            ruleset.BindValueChanged(onRulesetChanged, true);
            CircleSizeFilter.SelectionChanged += updateValue;

            tabControl.Current.BindTarget = keyModeId;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            CircleSizeFilter.SelectionChanged -= updateValue;
        }

        private void onRulesetChanged(ValueChangedEvent<RulesetInfo> e)
        {
            tabControl.UpdateForRuleset(e.NewValue.OnlineID);
            updateValue();
        }

        private void onSelectorChanged(ValueChangedEvent<string> e)
        {
            var modes = parseModeIds(e.NewValue);
            CircleSizeFilter.SetSelection(modes);
            tabControl.UpdateTabItemUI(modes);
        }

        private void updateValue()
        {
            int currentRulesetId = ruleset.Value.OnlineID;

            if (!modeSelections.ContainsKey(currentRulesetId))
                modeSelections[currentRulesetId] = new HashSet<string> { "All" };

            HashSet<string> selectedModes;

            if (isMultiSelectMode.Value)
            {
                selectedModes = CircleSizeFilter.SelectedModeIds;
                keyModeId.Value = string.Join(",", selectedModes.OrderBy(x => x));
            }
            else
            {
                selectedModes = CircleSizeFilter.SelectedModeIds;
                keyModeId.Value = selectedModes.Count > 0 ? selectedModes.First() : "All";
            }

            modeSelections[currentRulesetId] = selectedModes;
            tabControl.UpdateForRuleset(currentRulesetId);
            tabControl.UpdateTabItemUI(selectedModes);
            tabControl.IsMultiSelectMode = isMultiSelectMode.Value;
        }

        private HashSet<string> parseModeIds(string value)
        {
            if (string.IsNullOrEmpty(value))
                return new HashSet<string> { "All" };

            return new HashSet<string>(value.Split(','));
        }

        public partial class ShearedCsModeTabControl : OsuTabControl<string>
        {
            private readonly Box labelBox;
            private readonly OsuSpriteText labelText;
            private HashSet<string> currentSelection = new HashSet<string> { "All" };
            private int currentRulesetId = -1;

            public Container LabelContainer;
            public bool IsMultiSelectMode { get; set; }
            public float TabHeight { get; set; } = 30f;

            public Action<HashSet<string>>? SetCurrentSelections;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public ShearedCsModeTabControl()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Shear = OsuGame.SHEAR;
                CornerRadius = ShearedButton.CORNER_RADIUS;
                Masking = true;
                LabelContainer = new Container
                {
                    Depth = float.MaxValue,
                    CornerRadius = ShearedButton.CORNER_RADIUS,
                    Masking = true,
                    AutoSizeAxes = Axes.Y,
                    Width = 50,
                    Children = new Drawable[]
                    {
                        labelBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        labelText = new OsuSpriteText
                        {
                            Text = "Keys",
                            Margin = new MarginPadding
                                { Horizontal = 8f, Vertical = 7f },
                            Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                            Shear = -OsuGame.SHEAR,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                };
                AddInternal(LabelContainer);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                labelBox.Colour = colourProvider.Background3;

                TabContainer.Anchor = Anchor.CentreLeft;
                TabContainer.Origin = Anchor.CentreLeft;
                TabContainer.Shear = -OsuGame.SHEAR;
                TabContainer.RelativeSizeAxes = Axes.X;
                TabContainer.AutoSizeAxes = Axes.Y;
                TabContainer.Spacing = new Vector2(0f);
                TabContainer.Margin = new MarginPadding
                {
                    Left = LabelContainer.DrawWidth + 8,
                };
            }

            public void UpdateForRuleset(int rulesetId)
            {
                if (currentRulesetId == rulesetId && Items.Any())
                    return;

                currentRulesetId = rulesetId;

                var keyModes = CsItemIds.GetModesForRuleset(rulesetId)
                                        .OrderBy(m => m.Id == "All" ? -1 : m.CsValue ?? 0)
                                        .Select(m => m.Id)
                                        .ToList();

                TabContainer.Clear();
                Items = keyModes;
                labelText.Text = rulesetId == 3 ? "Keys" : "CS";

                UpdateTabItemUI(currentSelection);
            }

            public void UpdateTabItemUI(HashSet<string> selectedModes)
            {
                currentSelection = new HashSet<string>(selectedModes);

                foreach (var tabItem in TabContainer.Children.Cast<ShearedCsModeTabItem>())
                {
                    bool isSelected = selectedModes.Contains(tabItem.Value);
                    tabItem.UpdateButton(isSelected);
                }
            }

            protected override Dropdown<string> CreateDropdown() => null!;
            // protected override bool AddEnumEntriesAutomatically => false;

            protected override TabItem<string> CreateTabItem(string value)
            {
                var tabItem = new ShearedCsModeTabItem(value);
                tabItem.Clicked += onTabItemClicked;
                return tabItem;
            }

            private void onTabItemClicked(string mode)
            {
                var newSelection = new HashSet<string>(currentSelection);

                if (mode == "All" || newSelection.Count == 0)
                {
                    newSelection.Clear();
                    newSelection.Add("All");
                }
                else if (newSelection.Contains("All"))
                {
                    newSelection.Clear();
                    newSelection.Add(mode);
                }
                else if (!newSelection.Add(mode))
                {
                    newSelection.Remove(mode);
                }
                else
                {
                    if (IsMultiSelectMode)
                    {
                        newSelection.Add(mode);
                    }
                    else
                    {
                        newSelection.Clear();
                        newSelection.Add(mode);
                    }
                }

                currentSelection = newSelection;
                Current.Value = string.Join(",", newSelection.OrderBy(x => x));
                UpdateTabItemUI(newSelection);

                SetCurrentSelections?.Invoke(newSelection);
            }

            public partial class ShearedCsModeTabItem : TabItem<string>
            {
                private readonly OsuSpriteText text;
                private readonly Box background;
                private OverlayColourProvider colourProvider = null!;

                public event Action<string>? Clicked;

                public ShearedCsModeTabItem(string value)
                    : base(value)
                {
                    Shear = OsuGame.SHEAR;
                    CornerRadius = ShearedButton.CORNER_RADIUS;
                    Masking = true;
                    AutoSizeAxes = Axes.Both;
                    Margin = new MarginPadding { Left = 4 };

                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    };

                    var modeInfo = CsItemIds.GetById(value);
                    text = new OsuSpriteText
                    {
                        Text = modeInfo?.DisplayName ?? value,
                        Margin = new MarginPadding
                            { Horizontal = 10f, Vertical = 7f },
                        Font = OsuFont.Style.Body.With(weight: FontWeight.SemiBold),
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Shear = -OsuGame.SHEAR,
                        Colour = Colour4.White,
                    };

                    AddInternal(background);
                    AddInternal(text);
                }

                [BackgroundDependencyLoader]
                private void load(OverlayColourProvider colourProvider)
                {
                    this.colourProvider = colourProvider;
                    background.Colour = colourProvider.Background5;
                }

                public void UpdateButton(bool isSelected)
                {
                    if (Active.Value != isSelected)
                    {
                        Active.Value = isSelected;
                        Schedule(updateColours);
                    }
                }

                private void updateColours()
                {
                    using (BeginDelayedSequence(0))
                    {
                        if (Active.Value)
                        {
                            background.FadeColour(colourProvider.Light4, 150, Easing.OutQuint);
                            text.FadeColour(Colour4.Black, 150, Easing.OutQuint);
                        }
                        else if (IsHovered)
                        {
                            background.FadeColour(colourProvider.Background4, 150, Easing.OutQuint);
                            text.FadeColour(Colour4.White, 150, Easing.OutQuint);
                        }
                        else
                        {
                            background.FadeColour(colourProvider.Background5, 150, Easing.OutQuint);
                            text.FadeColour(Colour4.White, 150, Easing.OutQuint);
                        }
                    }
                }

                protected override void OnActivated() => Schedule(updateColours);
                protected override void OnDeactivated() => Schedule(updateColours);

                protected override bool OnHover(HoverEvent e)
                {
                    Schedule(updateColours);
                    return base.OnHover(e);
                }

                protected override void OnHoverLost(HoverLostEvent e)
                {
                    Schedule(updateColours);
                    base.OnHoverLost(e);
                }

                protected override bool OnClick(ClickEvent e)
                {
                    Clicked?.Invoke(Value);
                    return true;
                }
            }
        }
    }
}
