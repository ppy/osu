// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Play.PlayerSettings;

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

        [Resolved]
        private LadderEditorInfo editorInfo { get; set; } = null;

        [BackgroundDependencyLoader]
        private void load()
        {
            var teamEntries = editorInfo.Teams;

            var groupingOptions = editorInfo.Groupings.Select(g => new KeyValuePair<string, TournamentGrouping>(g.Name, g))
                                            .Prepend(new KeyValuePair<string, TournamentGrouping>("None", new TournamentGrouping()));

            Children = new Drawable[]
            {
                new PlayerCheckbox
                {
                    Bindable = editorInfo.EditingEnabled,
                    LabelText = "Enable editing"
                },
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
                groupingDropdown = new SettingsDropdown<TournamentGrouping>
                {
                    Bindable = new Bindable<TournamentGrouping> { Default = groupingOptions.First().Value },
                    Items = groupingOptions
                },
                losersCheckbox = new PlayerCheckbox
                {
                    LabelText = "Losers Bracket",
                    Bindable = new Bindable<bool>()
                }
            };

            editorInfo.Selected.ValueChanged += selection =>
            {
                textboxTeam1.Text = selection?.Team1.Value?.Acronym;
                textboxTeam2.Text = selection?.Team2.Value?.Acronym;
                groupingDropdown.Bindable.Value = selection?.Grouping.Value ?? groupingOptions.First().Value;
                losersCheckbox.Current.Value = selection?.Losers.Value ?? false;
            };

            textboxTeam1.OnCommit = (val, newText) =>
            {
                if (newText && editorInfo.Selected.Value != null)
                    editorInfo.Selected.Value.Team1.Value = teamEntries.FirstOrDefault(t => t.Acronym == val.Text);
            };

            textboxTeam2.OnCommit = (val, newText) =>
            {
                if (newText && editorInfo.Selected.Value != null)
                    editorInfo.Selected.Value.Team2.Value = teamEntries.FirstOrDefault(t => t.Acronym == val.Text);
            };

            groupingDropdown.Bindable.ValueChanged += grouping =>
            {
                if (editorInfo.Selected.Value != null)
                    editorInfo.Selected.Value.Grouping.Value = grouping;
            };

            losersCheckbox.Current.ValueChanged += losers =>
            {
                if (editorInfo.Selected.Value != null)
                    editorInfo.Selected.Value.Losers.Value = losers;
            };

            // sliderBestOf.Bindable.ValueChanged += val =>
            // {
            //     if (editorInfo.Selected.Value != null) editorInfo.Selected.Value.BestOf.Value = (int)val;
            // };

            editorInfo.EditingEnabled.ValueChanged += enabled =>
            {
                if (!enabled) editorInfo.Selected.Value = null;
            };
        }
    }
}
