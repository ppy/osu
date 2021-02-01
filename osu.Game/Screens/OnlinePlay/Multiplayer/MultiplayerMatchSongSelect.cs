// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerMatchSongSelect : SongSelect, IOnlinePlaySubScreen
    {
        public string ShortTitle => "song selection";

        public override string Title => ShortTitle.Humanize();

        [Resolved(typeof(Room), nameof(Room.Playlist))]
        private BindableList<PlaylistItem> playlist { get; set; }

        [Resolved]
        private StatefulMultiplayerClient client { get; set; }

        private readonly Bindable<IReadOnlyList<Mod>> freeMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        private readonly FreeModSelectOverlay freeModSelectOverlay;
        private LoadingLayer loadingLayer;

        private WorkingBeatmap initialBeatmap;
        private RulesetInfo initialRuleset;
        private IReadOnlyList<Mod> initialMods;

        private bool itemSelected;

        public MultiplayerMatchSongSelect()
        {
            Padding = new MarginPadding { Horizontal = HORIZONTAL_OVERFLOW_PADDING };

            freeModSelectOverlay = new FreeModSelectOverlay
            {
                SelectedMods = { BindTarget = freeMods },
                IsValidMod = isValidFreeMod,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(loadingLayer = new LoadingLayer(true));
            initialBeatmap = Beatmap.Value;
            initialRuleset = Ruleset.Value;
            initialMods = Mods.Value.ToList();

            freeMods.Value = playlist.FirstOrDefault()?.AllowedMods.Select(m => m.CreateCopy()).ToArray() ?? Array.Empty<Mod>();

            FooterPanels.Add(freeModSelectOverlay);
        }

        protected override bool OnStart()
        {
            itemSelected = true;
            var item = new PlaylistItem();

            item.Beatmap.Value = Beatmap.Value.BeatmapInfo;
            item.Ruleset.Value = Ruleset.Value;

            item.RequiredMods.Clear();
            item.RequiredMods.AddRange(Mods.Value.Select(m => m.CreateCopy()));

            item.AllowedMods.Clear();
            item.AllowedMods.AddRange(freeMods.Value.Select(m => m.CreateCopy()));

            // If the client is already in a room, update via the client.
            // Otherwise, update the playlist directly in preparation for it to be submitted to the API on match creation.
            if (client.Room != null)
            {
                loadingLayer.Show();

                client.ChangeSettings(item: item).ContinueWith(t =>
                {
                    Schedule(() =>
                    {
                        loadingLayer.Hide();

                        if (t.IsCompletedSuccessfully)
                            this.Exit();
                        else
                        {
                            Logger.Log($"Could not use current beatmap ({t.Exception?.Message})", level: LogLevel.Important);
                            Carousel.AllowSelection = true;
                        }
                    });
                });
            }
            else
            {
                playlist.Clear();
                playlist.Add(item);
                this.Exit();
            }

            return true;
        }

        public override bool OnExiting(IScreen next)
        {
            if (!itemSelected)
            {
                Beatmap.Value = initialBeatmap;
                Ruleset.Value = initialRuleset;
                Mods.Value = initialMods;
            }

            return base.OnExiting(next);
        }

        protected override BeatmapDetailArea CreateBeatmapDetailArea() => new PlayBeatmapDetailArea();

        protected override ModSelectOverlay CreateModSelectOverlay() => new SoloModSelectOverlay
        {
            IsValidMod = isValidMod
        };

        protected override IEnumerable<(FooterButton, OverlayContainer)> CreateFooterButtons()
        {
            var buttons = base.CreateFooterButtons().ToList();
            buttons.Insert(buttons.FindIndex(b => b.Item1 is FooterButtonMods) + 1, (new FooterButtonFreeMods { Current = freeMods }, freeModSelectOverlay));
            return buttons;
        }

        private bool isValidMod(Mod mod) => !(mod is ModAutoplay) && (mod as MultiMod)?.Mods.Any(mm => mm is ModAutoplay) != true;

        private bool isValidFreeMod(Mod mod) => isValidMod(mod) && !(mod is ModRateAdjust) && !(mod is ModTimeRamp);
    }

    public class FooterButtonFreeMods : FooterButton, IHasCurrentValue<IReadOnlyList<Mod>>
    {
        public Bindable<IReadOnlyList<Mod>> Current
        {
            get => modDisplay.Current;
            set => modDisplay.Current = value;
        }

        private readonly ModDisplay modDisplay;

        public FooterButtonFreeMods()
        {
            ButtonContentContainer.Add(modDisplay = new ModDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                DisplayUnrankedText = false,
                Scale = new Vector2(0.8f),
                ExpansionMode = ExpansionMode.AlwaysContracted,
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            SelectedColour = colours.Yellow;
            DeselectedColour = SelectedColour.Opacity(0.5f);
            Text = @"freemods";
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(_ => updateModDisplay(), true);
        }

        private void updateModDisplay()
        {
            if (Current.Value?.Count > 0)
                modDisplay.FadeIn();
            else
                modDisplay.FadeOut();
        }
    }
}
