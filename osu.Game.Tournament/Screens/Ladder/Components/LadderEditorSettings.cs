// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class LadderEditorSettings : PlayerSettingsGroup
    {
        private const int padding = 10;

        protected override string Title => @"ladder";

        private SettingsDropdown<TournamentRound> roundDropdown;
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
                roundDropdown = new SettingsRoundDropdown(ladderInfo.Rounds) { LabelText = "Round" },
                losersCheckbox = new PlayerCheckbox { LabelText = "Losers Bracket" },
                dateTimeBox = new DateTextBox { LabelText = "Match Time" },
            };

            editorInfo.Selected.ValueChanged += selection =>
            {
                roundDropdown.Bindable = selection.NewValue?.Round;
                losersCheckbox.Current = selection.NewValue?.Losers;
                dateTimeBox.Bindable = selection.NewValue?.Date;

                team1Dropdown.Bindable = selection.NewValue?.Team1;
                team2Dropdown.Bindable = selection.NewValue?.Team2;
            };

            roundDropdown.Bindable.ValueChanged += round =>
            {
                if (editorInfo.Selected.Value?.Date.Value < round.NewValue?.StartDate.Value)
                {
                    editorInfo.Selected.Value.Date.Value = round.NewValue.StartDate.Value;
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

        private class SettingsRoundDropdown : LadderSettingsDropdown<TournamentRound>
        {
            public SettingsRoundDropdown(BindableList<TournamentRound> rounds)
            {
                Bindable = new Bindable<TournamentRound>();

                foreach (var r in rounds.Prepend(new TournamentRound()))
                    add(r);

                rounds.ItemsRemoved += items => items.ForEach(i => Control.RemoveDropdownItem(i));
                rounds.ItemsAdded += items => items.ForEach(add);
            }

            private readonly List<IUnbindable> refBindables = new List<IUnbindable>();

            private T boundReference<T>(T obj)
                where T : IBindable
            {
                obj = (T)obj.GetBoundCopy();
                refBindables.Add(obj);
                return obj;
            }

            private void add(TournamentRound round)
            {
                Control.AddDropdownItem(round);
                boundReference(round.Name).BindValueChanged(_ =>
                {
                    Control.RemoveDropdownItem(round);
                    Control.AddDropdownItem(round);
                });
            }
        }

        private class SettingsTeamDropdown : LadderSettingsDropdown<TournamentTeam>
        {
            public SettingsTeamDropdown(BindableList<TournamentTeam> teams)
            {
                foreach (var t in teams.Prepend(new TournamentTeam()))
                    add(t);

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

        private class LadderSettingsDropdown<T> : SettingsDropdown<T>
        {
            protected override OsuDropdown<T> CreateDropdown() => new DropdownControl();

            private new class DropdownControl : SettingsDropdown<T>.DropdownControl
            {
                protected override DropdownMenu CreateMenu() => new Menu();

                private new class Menu : OsuDropdownMenu
                {
                    public Menu()
                    {
                        MaxHeight = 200;
                    }
                }
            }
        }
    }
}
