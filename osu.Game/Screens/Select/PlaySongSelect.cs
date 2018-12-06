// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osuTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Skinning;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelect : SongSelect
    {
        private OsuScreen player;
        protected readonly BeatmapDetailArea BeatmapDetails;
        private bool removeAutoModOnResume;

        public PlaySongSelect()
        {
            LeftContent.Add(BeatmapDetails = new BeatmapDetailArea
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Top = 10, Right = 5 },
            });

            BeatmapDetails.Leaderboard.ScoreSelected += s => Push(new Results(s));
        }

        private SampleChannel sampleConfirm;

        [Cached]
        [Cached(Type = typeof(IBindable<IEnumerable<Mod>>))]
        private readonly Bindable<IEnumerable<Mod>> selectedMods = new Bindable<IEnumerable<Mod>>(new Mod[] { });

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, AudioManager audio, BeatmapManager beatmaps, SkinManager skins, DialogOverlay dialogOverlay, Bindable<IEnumerable<Mod>> selectedMods)
        {
            if (selectedMods != null) this.selectedMods.BindTo(selectedMods);

            sampleConfirm = audio.Sample.Get(@"SongSelect/confirm-selection");

            BeatmapOptions.AddButton(@"Remove", @"from unplayed", FontAwesome.fa_times_circle_o, colours.Purple, null, Key.Number1);
            BeatmapOptions.AddButton(@"Clear", @"local scores", FontAwesome.fa_eraser, colours.Purple, null, Key.Number2);
            BeatmapOptions.AddButton(@"Edit", @"beatmap", FontAwesome.fa_pencil, colours.Yellow, () =>
            {
                ValidForResume = false;
                Edit();
            }, Key.Number3);

            if (dialogOverlay != null)
            {
                Schedule(() =>
                {
                    // if we have no beatmaps but osu-stable is found, let's prompt the user to import.
                    if (!beatmaps.GetAllUsableBeatmapSets().Any() && beatmaps.StableInstallationAvailable)
                        dialogOverlay.Push(new ImportFromStablePopup(() =>
                        {
                            beatmaps.ImportFromStableAsync();
                            skins.ImportFromStableAsync();
                        }));
                });
            }
        }

        protected override void ExitFromBack()
        {
            if (modSelect.State == Visibility.Visible)
            {
                modSelect.Hide();
                return;
            }

            base.ExitFromBack();
        }

        protected override void UpdateBeatmap(WorkingBeatmap beatmap)
        {
            beatmap.Mods.BindTo(selectedMods);

            base.UpdateBeatmap(beatmap);

            BeatmapDetails.Beatmap = beatmap;

            if (beatmap.Track != null)
                beatmap.Track.Looping = true;
        }

        protected override void OnResuming(Screen last)
        {
            player = null;

            if (removeAutoModOnResume)
            {
                var autoType = Ruleset.Value.CreateInstance().GetAutoplayMod().GetType();
                ModSelect.DeselectTypes(new[] { autoType }, true);
                removeAutoModOnResume = false;
            }

            BeatmapDetails.Leaderboard.RefreshScores();

            Beatmap.Value.Track.Looping = true;

            base.OnResuming(last);
        }

        protected override bool OnExiting(Screen next)
        {
            if (base.OnExiting(next))
                return true;

            if (Beatmap.Value.Track != null)
                Beatmap.Value.Track.Looping = false;

            selectedMods.UnbindAll();
            Beatmap.Value.Mods.Value = new Mod[] { };

            return false;
        }

        protected override bool OnStart()
        {
            if (player != null) return false;

            // Ctrl+Enter should start map with autoplay enabled.
            if (GetContainingInputManager().CurrentState?.Keyboard.ControlPressed == true)
            {
                var auto = Ruleset.Value.CreateInstance().GetAutoplayMod();
                var autoType = auto.GetType();

                var mods = selectedMods.Value;
                if (mods.All(m => m.GetType() != autoType))
                {
                    selectedMods.Value = mods.Append(auto);
                    removeAutoModOnResume = true;
                }
            }

            Beatmap.Value.Track.Looping = false;
            Beatmap.Disabled = true;

            sampleConfirm?.Play();

            LoadComponentAsync(player = new PlayerLoader(new Player()), l =>
            {
                if (IsCurrentScreen) Push(player);
            });

            return true;
        }
    }
}
