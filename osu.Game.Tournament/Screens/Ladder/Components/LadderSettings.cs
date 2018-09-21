// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Play.PlayerSettings;
using osu.Game.Tournament.Components;

namespace osu.Game.Tournament.Screens.Ladder.Components
{
    public class LadderEditorSettings : PlayerSettingsGroup
    {
        private const int padding = 10;

        protected override string Title => @"ladder";

        private PlayerSliderBar<double> sliderBestOf;

        private SettingsDropdown<TournamentTeam> dropdownTeam1;
        private SettingsDropdown<TournamentTeam> dropdownTeam2;

        [Resolved]
        private LadderEditorInfo editorInfo { get; set; } = null;

        [BackgroundDependencyLoader]
        private void load()
        {
            var teamEntries = editorInfo.Teams.Select(t => new KeyValuePair<string, TournamentTeam>(t.ToString(), t)).Prepend(new KeyValuePair<string, TournamentTeam>("Empty", new TournamentTeam()));

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
                dropdownTeam1 = new SettingsDropdown<TournamentTeam>
                {
                    Items = teamEntries,
                    Bindable = new Bindable<TournamentTeam>
                    {
                        Value = teamEntries.First().Value,
                        Default = teamEntries.First().Value
                    }
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
                            Text = "Team2",
                        },
                    },
                },
                dropdownTeam2 = new SettingsDropdown<TournamentTeam>
                {
                    Items = teamEntries,
                    Bindable = new Bindable<TournamentTeam>
                    {
                        Value = teamEntries.First().Value,
                        Default = teamEntries.First().Value
                    }
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
                            Text = "Best of",
                        },
                    },
                },
                sliderBestOf = new PlayerSliderBar<double>
                {
                    Bindable = new BindableDouble
                    {
                        Default = 5,
                        Value = 5,
                        MinValue = 1,
                        MaxValue = 20,
                        Precision = 1,
                    },
                }
            };

            editorInfo.Selected.ValueChanged += selection =>
            {
                dropdownTeam1.Bindable.Value = dropdownTeam1.Items.FirstOrDefault(i => i.Value.Acronym == selection?.Team1.Value?.Acronym).Value;
                dropdownTeam2.Bindable.Value = dropdownTeam1.Items.FirstOrDefault(i => i.Value.Acronym == selection?.Team2.Value?.Acronym).Value;
                sliderBestOf.Bindable.Value = selection?.BestOf ?? sliderBestOf.Bindable.Default;
            };

            dropdownTeam1.Bindable.ValueChanged += val =>
            {
                if (editorInfo.Selected.Value != null) editorInfo.Selected.Value.Team1.Value = val.Acronym == null ? null : val;
            };

            dropdownTeam2.Bindable.ValueChanged += val =>
            {
                if (editorInfo.Selected.Value != null) editorInfo.Selected.Value.Team2.Value = val.Acronym == null ? null : val;
            };

            sliderBestOf.Bindable.ValueChanged += val =>
            {
                if (editorInfo.Selected.Value != null) editorInfo.Selected.Value.BestOf.Value = (int)val;
            };

            editorInfo.EditingEnabled.ValueChanged += enabled =>
            {
                if (!enabled) editorInfo.Selected.Value = null;
            };
        }
    }
}
