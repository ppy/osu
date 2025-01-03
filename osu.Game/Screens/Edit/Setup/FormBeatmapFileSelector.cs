// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osuTK;
using osu.Game.Localisation;

namespace osu.Game.Screens.Edit.Setup
{
    /// <summary>
    /// A type of <see cref="FormFileSelector"/> dedicated to beatmap resources.
    /// </summary>
    /// <remarks>
    /// This expands on <see cref="FormFileSelector"/> by adding an intermediate step before finalisation
    /// to choose whether the selected file should be applied to the current difficulty or all difficulties in the set,
    /// the user's choice is saved in <see cref="ApplyToAllDifficulties"/> before the file selection is finalised and propagated to <see cref="FormFileSelector.Current"/>.
    /// </remarks>
    public partial class FormBeatmapFileSelector : FormFileSelector
    {
        private readonly bool beatmapHasMultipleDifficulties;

        public readonly Bindable<bool> ApplyToAllDifficulties = new Bindable<bool>(true);

        private SelectionScopePopoverTarget selectionScopeTarget = null!;

        protected override bool IsPopoverVisible => base.IsPopoverVisible || selectionScopeTarget.PopoverState.Value == Visibility.Visible;

        public FormBeatmapFileSelector(bool beatmapHasMultipleDifficulties, params string[] handledExtensions)
            : base(handledExtensions)
        {
            this.beatmapHasMultipleDifficulties = beatmapHasMultipleDifficulties;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(selectionScopeTarget = new SelectionScopePopoverTarget
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            selectionScopeTarget.PopoverState.BindValueChanged(_ => UpdateState(), true);
        }

        protected override void OnFileSelected()
        {
            if (InternalSelection.Value == null)
                return;

            if (!beatmapHasMultipleDifficulties)
            {
                base.OnFileSelected();
                return;
            }

            selectionScopeTarget.ShowPopover();
            selectionScopeTarget.OnSelected = v =>
            {
                ApplyToAllDifficulties.Value = v;
                base.OnFileSelected();
            };
        }

        public partial class SelectionScopePopoverTarget : Drawable, IHasPopover
        {
            public Action<bool>? OnSelected;

            public readonly Bindable<Visibility> PopoverState = new Bindable<Visibility>();

            public Popover GetPopover()
            {
                var popover = new SelectionScopePopover(v => OnSelected?.Invoke(v));
                PopoverState.UnbindBindings();
                PopoverState.BindTo(popover.State);
                return popover;
            }
        }

        private partial class SelectionScopePopover : OsuPopover
        {
            private readonly Action<bool> onSelected;

            public SelectionScopePopover(Action<bool> onSelected)
            {
                this.onSelected = onSelected;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, OverlayColourProvider colourProvider)
            {
                AutoSizeAxes = Axes.Both;

                Body.BorderColour = colourProvider.Highlight1;
                Body.BorderThickness = 2;

                Child = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = EditorSetupStrings.ApplicationScopeSelectionTitle,
                            Margin = new MarginPadding { Bottom = 20f },
                        },
                        new RoundedButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Width = 300f,
                            Text = EditorSetupStrings.ApplyToAllDifficulties,
                            Action = () => onSelected(true),
                            BackgroundColour = colours.Red2,
                        },
                        new RoundedButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Width = 300f,
                            Text = EditorSetupStrings.ApplyToThisDifficulty,
                            Action = () => onSelected(false),
                        },
                    }
                };
            }
        }
    }
}
