// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelect : SongSelect
    {
        private OsuScreen player;
        private readonly ModSelectOverlay modSelect;
        protected readonly BeatmapDetailArea BeatmapDetails;
        private bool removeAutoModOnResume;

        public PlaySongSelect()
        {
            FooterPanels.Add(modSelect = new ModSelectOverlay
            {
                RelativeSizeAxes = Axes.X,
                Origin = Anchor.BottomCentre,
                Anchor = Anchor.BottomCentre,
            });

            LeftContent.Add(BeatmapDetails = new BeatmapDetailArea
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding { Top = 10, Right = 5 },
            });

            BeatmapDetails.Leaderboard.ScoreSelected += s => Push(new Results(s));
        }

        private SampleChannel sampleConfirm;

        public readonly Bindable<IEnumerable<Mod>> SelectedMods = new Bindable<IEnumerable<Mod>>(new List<Mod>());

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colours, AudioManager audio, BeatmapManager beatmaps, DialogOverlay dialogOverlay, OsuGame osu)
        {
            if (osu != null) SelectedMods.BindTo(osu.SelectedMods);
            modSelect.SelectedMods.BindTo(SelectedMods);

            sampleConfirm = audio.Sample.Get(@"SongSelect/confirm-selection");

            Footer.AddButton(@"mods", colours.Yellow, modSelect, Key.F1, float.MaxValue);

            BeatmapOptions.AddButton(@"Remove", @"from unplayed", FontAwesome.fa_times_circle_o, colours.Purple, null, Key.Number1);
            BeatmapOptions.AddButton(@"Clear", @"local scores", FontAwesome.fa_eraser, colours.Purple, null, Key.Number2);
            BeatmapOptions.AddButton(@"Edit", @"Beatmap", FontAwesome.fa_pencil, colours.Yellow, () =>
            {
                ValidForResume = false;
                Push(new Editor());
            }, Key.Number3);

            if (dialogOverlay != null)
            {
                Schedule(() =>
                {
                    // if we have no beatmaps but osu-stable is found, let's prompt the user to import.
                    if (!beatmaps.GetAllUsableBeatmapSets().Any() && beatmaps.StableInstallationAvailable)
                        dialogOverlay.Push(new ImportFromStablePopup(() =>
                            Task.Factory.StartNew(beatmaps.ImportFromStable, TaskCreationOptions.LongRunning)));
                });
            }
        }

        protected override void UpdateBeatmap(WorkingBeatmap beatmap)
        {
            base.UpdateBeatmap(beatmap);

            beatmap.Mods.BindTo(SelectedMods);

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
                modSelect.DeselectTypes(new[] { autoType }, true);
                removeAutoModOnResume = false;
            }

            Beatmap.Value.Track.Looping = true;

            base.OnResuming(last);
        }

        protected override void OnSuspending(Screen next)
        {
            modSelect.Hide();

            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            if (modSelect.State == Visibility.Visible)
            {
                modSelect.Hide();
                return true;
            }

            if (base.OnExiting(next))
                return true;

            if (Beatmap.Value.Track != null)
                Beatmap.Value.Track.Looping = false;

            SelectedMods.UnbindAll();
            Beatmap.Value.Mods.Value = new Mod[] { };

            return false;
        }

        protected override bool OnSelectionFinalised()
        {
            if (player != null) return false;

            // Ctrl+Enter should start map with autoplay enabled.
            if (GetContainingInputManager().CurrentState?.Keyboard.ControlPressed == true)
            {
                var auto = Ruleset.Value.CreateInstance().GetAutoplayMod();
                var autoType = auto.GetType();

                var mods = modSelect.SelectedMods.Value;
                if (mods.All(m => m.GetType() != autoType))
                {
                    modSelect.SelectedMods.Value = mods.Append(auto);
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
