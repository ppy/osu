// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using WebCommonStrings = osu.Game.Resources.Localisation.Web.CommonStrings;

namespace osu.Game.Screens.Select.FooterV2
{
    public partial class BeatmapOptionsPopover : OsuPopover
    {
        private FillFlowContainer buttonFlow = null!;
        private readonly FooterButtonOptionsV2 footerButton;

        private WorkingBeatmap beatmapWhenOpening = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        public BeatmapOptionsPopover(FooterButtonOptionsV2 footerButton)
        {
            this.footerButton = footerButton;
        }

        [BackgroundDependencyLoader]
        private void load(ManageCollectionsDialog? manageCollectionsDialog, SongSelect? songSelect, OsuColour colours, BeatmapManager? beatmapManager)
        {
            Content.Padding = new MarginPadding(5);

            Child = buttonFlow = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(3),
            };

            beatmapWhenOpening = beatmap.Value;

            addHeader(CommonStrings.General);
            addButton(SongSelectStrings.ManageCollections, FontAwesome.Solid.Book, () => manageCollectionsDialog?.Show());

            addHeader(SongSelectStrings.ForAllDifficulties, beatmapWhenOpening.BeatmapSetInfo.ToString());
            addButton(SongSelectStrings.DeleteBeatmap, FontAwesome.Solid.Trash, () => songSelect?.DeleteBeatmap(beatmapWhenOpening.BeatmapSetInfo), colours.Red1);

            addHeader(SongSelectStrings.ForSelectedDifficulty, beatmapWhenOpening.BeatmapInfo.DifficultyName);
            // TODO: make work, and make show "unplayed" or "played" based on status.
            addButton(SongSelectStrings.MarkAsPlayed, FontAwesome.Regular.TimesCircle, null);
            addButton(SongSelectStrings.ClearAllLocalScores, FontAwesome.Solid.Eraser, () => songSelect?.ClearScores(beatmapWhenOpening.BeatmapInfo), colours.Red1);

            if (songSelect != null && songSelect.AllowEditing)
                addButton(SongSelectStrings.EditBeatmap, FontAwesome.Solid.PencilAlt, () => songSelect.Edit(beatmapWhenOpening.BeatmapInfo));

            addButton(WebCommonStrings.ButtonsHide.ToSentence(), FontAwesome.Solid.Magic, () => beatmapManager?.Hide(beatmapWhenOpening.BeatmapInfo));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ScheduleAfterChildren(() => GetContainingInputManager().ChangeFocus(this));

            beatmap.BindValueChanged(_ => Hide());
        }

        [Resolved]
        private OverlayColourProvider overlayColourProvider { get; set; } = null!;

        private void addHeader(LocalisableString text, string? context = null)
        {
            var textFlow = new OsuTextFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Padding = new MarginPadding(10),
            };

            textFlow.AddText(text, t => t.Font = OsuFont.Default.With(weight: FontWeight.SemiBold));

            if (context != null)
            {
                textFlow.NewLine();
                textFlow.AddText(context, t =>
                {
                    t.Colour = overlayColourProvider.Content2;
                    t.Font = t.Font.With(size: 13);
                });
            }

            buttonFlow.Add(textFlow);
        }

        private void addButton(LocalisableString text, IconUsage icon, Action? action, Color4? colour = null)
        {
            var button = new OptionButton
            {
                Text = text,
                Icon = icon,
                TextColour = colour,
                Action = () =>
                {
                    Scheduler.AddDelayed(Hide, 50);
                    action?.Invoke();
                },
            };

            buttonFlow.Add(button);
        }

        private partial class OptionButton : OsuButton
        {
            public IconUsage Icon { get; init; }
            public Color4? TextColour { get; init; }

            public OptionButton()
            {
                Size = new Vector2(265, 50);
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                BackgroundColour = colourProvider.Background3;

                SpriteText.Colour = TextColour ?? Color4.White;
                Content.CornerRadius = 10;

                Add(new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Size = new Vector2(17),
                    X = 15,
                    Icon = Icon,
                    Colour = TextColour ?? Color4.White,
                });
            }

            protected override SpriteText CreateText() => new OsuSpriteText
            {
                Depth = -1,
                Origin = Anchor.CentreLeft,
                Anchor = Anchor.CentreLeft,
                X = 40
            };
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            // don't absorb control as ToolbarRulesetSelector uses control + number to navigate
            if (e.ControlPressed) return false;

            if (!e.Repeat && e.Key >= Key.Number1 && e.Key <= Key.Number9)
            {
                int requested = e.Key - Key.Number1;

                OptionButton? found = buttonFlow.Children.OfType<OptionButton>().ElementAtOrDefault(requested);

                if (found != null)
                {
                    found.TriggerClick();
                    return true;
                }
            }

            return base.OnKeyDown(e);
        }

        protected override void UpdateState(ValueChangedEvent<Visibility> state)
        {
            base.UpdateState(state);

            if (state.NewValue == Visibility.Hidden)
                footerButton.IsActive.Value = false;
        }
    }
}
