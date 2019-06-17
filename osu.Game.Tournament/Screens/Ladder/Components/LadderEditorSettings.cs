// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class LadderEditorSettings : PlayerSettingsGroup
    {
        private const int padding = 10;

        protected override string Title => @"ladder";

        private SettingsDropdown<TournamentGrouping> groupingDropdown;
        private PlayerCheckbox losersCheckbox;
        private DateTextBox dateTimeBox;
        private SettingsTeamDropdown team1Dropdown;
        private SettingsTeamDropdown team2Dropdown;

        [Resolved]
        private LadderEditorInfo editorInfo { get; set; }

        [Resolved]
        private LadderInfo ladderInfo { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                team1Dropdown = new SettingsTeamDropdown(ladderInfo.Teams) { LabelText = "Team 1" },
                team2Dropdown = new SettingsTeamDropdown(ladderInfo.Teams) { LabelText = "Team 2" },
                groupingDropdown = new SettingsGroupingDropdown(ladderInfo.Groupings) { LabelText = "Grouping" },
                losersCheckbox = new PlayerCheckbox { LabelText = "Losers Bracket" },
                dateTimeBox = new DateTextBox { LabelText = "Match Time" },
            };

            editorInfo.Selected.ValueChanged += selection =>
            {
                groupingDropdown.Bindable = selection.NewValue?.Grouping;
                losersCheckbox.Current = selection.NewValue?.Losers;
                dateTimeBox.Bindable = selection.NewValue?.Date;

                team1Dropdown.Bindable = selection.NewValue?.Team1;
                team2Dropdown.Bindable = selection.NewValue?.Team2;
            };

            groupingDropdown.Bindable.ValueChanged += grouping =>
            {
                if (editorInfo.Selected.Value?.Date.Value < grouping.NewValue?.StartDate.Value)
                {
                    editorInfo.Selected.Value.Date.Value = grouping.NewValue.StartDate.Value;
                    editorInfo.Selected.TriggerChange();
                }
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

        private class SettingsTeamDropdown : SettingsDropdown<TournamentTeam>
        {
            public SettingsTeamDropdown(BindableList<TournamentTeam> teams)
            {
                foreach (var g in teams.Prepend(new TournamentTeam()))
                    add(g);

                teams.ItemsRemoved += items => items.ForEach(i => Control.RemoveDropdownItem(i));
                teams.ItemsAdded += items => items.ForEach(add);
            }

            private readonly List<IUnbindable> refBindables = new List<IUnbindable>();

            private T boundReference<T>(T obj)
                where T : IBindable
            {
                obj = (T)obj.GetBoundCopy();
                refBindables.Add(obj);
                return obj;
            }

            private void add(TournamentTeam team)
            {
                Control.AddDropdownItem(team);
                boundReference(team.FullName).BindValueChanged(_ =>
                {
                    Control.RemoveDropdownItem(team);
                    Control.AddDropdownItem(team);
                });
            }
        }
    }
}
