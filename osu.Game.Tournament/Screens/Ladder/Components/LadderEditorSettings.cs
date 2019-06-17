// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class LadderEditorSettings : PlayerSettingsGroup
    {
        private const int padding = 10;

        protected override string Title => @"ladder";

        private OsuTextBox textboxTeam1;
        private OsuTextBox textboxTeam2;
        private SettingsDropdown<TournamentGrouping> groupingDropdown;
        private PlayerCheckbox losersCheckbox;
        private DateTextBox dateTimeBox;

        [Resolved]
        private LadderEditorInfo editorInfo { get; set; }

        [Resolved]
        private LadderInfo ladderInfo { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            var teamEntries = ladderInfo.Teams;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = padding },
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = "Team1",
                        },
                    },
                },
                textboxTeam1 = new OsuTextBox { RelativeSizeAxes = Axes.X, Height = 20 },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Horizontal = padding },
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Text = "Team2",
                        },
                    },
                },
                textboxTeam2 = new OsuTextBox { RelativeSizeAxes = Axes.X, Height = 20 },
                groupingDropdown = new SettingsGroupingDropdown(ladderInfo.Groupings),
                losersCheckbox = new PlayerCheckbox
                {
                    LabelText = "Losers Bracket",
                    Bindable = new Bindable<bool>()
                },
                dateTimeBox = new DateTextBox
                {
                    Bindable = new Bindable<DateTimeOffset>()
                }
            };

            editorInfo.Selected.ValueChanged += selection =>
            {
                textboxTeam1.Text = selection.NewValue?.Team1.Value?.Acronym.Value;
                textboxTeam2.Text = selection.NewValue?.Team2.Value?.Acronym.Value;
                groupingDropdown.Bindable.Value = selection.NewValue?.Grouping.Value;
                losersCheckbox.Current.Value = selection.NewValue?.Losers.Value ?? false;
                dateTimeBox.Bindable.Value = selection.NewValue?.Date.Value ?? DateTimeOffset.UtcNow;
            };

            textboxTeam1.OnCommit = (val, newText) =>
            {
                if (newText && editorInfo.Selected.Value != null)
                    editorInfo.Selected.Value.Team1.Value = teamEntries.FirstOrDefault(t => t.Acronym.Value == val.Text);
            };

            textboxTeam2.OnCommit = (val, newText) =>
            {
                if (newText && editorInfo.Selected.Value != null)
                    editorInfo.Selected.Value.Team2.Value = teamEntries.FirstOrDefault(t => t.Acronym.Value == val.Text);
            };

            groupingDropdown.Bindable.ValueChanged += grouping =>
            {
                if (editorInfo.Selected.Value != null)
                {
                    editorInfo.Selected.Value.Grouping.Value = grouping.NewValue;

                    if (editorInfo.Selected.Value.Date.Value < grouping.NewValue.StartDate.Value)
                    {
                        editorInfo.Selected.Value.Date.Value = grouping.NewValue.StartDate.Value;
                        editorInfo.Selected.TriggerChange();
                    }
                }
            };

            losersCheckbox.Current.ValueChanged += losers =>
            {
                if (editorInfo.Selected.Value != null)
                    editorInfo.Selected.Value.Losers.Value = losers.NewValue;
            };

            dateTimeBox.Bindable.ValueChanged += date =>
            {
                if (editorInfo.Selected.Value != null)
                    editorInfo.Selected.Value.Date.Value = date.NewValue;
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            this.FadeIn();
        }

        protected override bool OnHover(HoverEvent e)
        {
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
        }

        private class SettingsGroupingDropdown : SettingsDropdown<TournamentGrouping>
        {
            public SettingsGroupingDropdown(BindableList<TournamentGrouping> groupings)
            {
                Bindable = new Bindable<TournamentGrouping>();

                foreach (var g in groupings.Prepend(new TournamentGrouping()))
                    add(g);

                groupings.ItemsRemoved += items => items.ForEach(i => Control.RemoveDropdownItem(i));
                groupings.ItemsAdded += items => items.ForEach(add);
            }

            private readonly List<IUnbindable> refBindables = new List<IUnbindable>();

            private T boundReference<T>(T obj)
                where T : IBindable
            {
                obj = (T)obj.GetBoundCopy();
                refBindables.Add(obj);
                return obj;
            }

            private void add(TournamentGrouping grouping)
            {
                Control.AddDropdownItem(grouping);
                boundReference(grouping.Name).BindValueChanged(_ =>
                {
                    Control.RemoveDropdownItem(grouping);
                    Control.AddDropdownItem(grouping);
                });
            }
        }
    }
}
