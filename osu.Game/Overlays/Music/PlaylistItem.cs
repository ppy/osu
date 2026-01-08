// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK.Graphics;

namespace osu.Game.Overlays.Music
{
    public partial class PlaylistItem : PoolableDrawable, IHasCurrentValue<Live<BeatmapSetInfo>>
    {
        public Bindable<Live<BeatmapSetInfo>> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableWithCurrent<Live<BeatmapSetInfo>> current = new BindableWithCurrent<Live<BeatmapSetInfo>>();

        private readonly Bindable<Live<BeatmapSetInfo>?> selectedSet = new Bindable<Live<BeatmapSetInfo>?>();
        private Action<Live<BeatmapSetInfo>>? requestSelection;

        private MarqueeContainer text = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(PlaylistOverlay playlistOverlay)
        {
            RelativeSizeAxes = Axes.X;
            Height = 20;

            InternalChild = text = new MarqueeContainer
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.X,
                InitialMoveDelay = 0,
                AllowScrolling = false,
                Padding = new MarginPadding { Horizontal = 15 },
            };

            selectedSet.BindTo(playlistOverlay.SelectedSet);
            requestSelection = playlistOverlay.ItemSelected;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Current.BindValueChanged(_ => onItemChanged(), true);
            selectedSet.BindValueChanged(updateSelectionState, true);
        }

        private void onItemChanged() => Current.Value.PerformRead(m =>
        {
            var metadata = m.Metadata;

            var title = new RomanisableString(metadata.TitleUnicode, metadata.Title);
            var artist = new RomanisableString(metadata.ArtistUnicode, metadata.Artist);

            text.CreateContent = () =>
            {
                var flow = new OsuTextFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                };

                flow.AddText(title, sprite => sprite.Font = OsuFont.GetFont(weight: FontWeight.Regular));
                flow.AddText(@"  "); // to separate the title from the artist.
                flow.AddText(artist, sprite =>
                {
                    sprite.Font = OsuFont.GetFont(size: 14, weight: FontWeight.Bold);
                    sprite.Colour = colours.Gray9;
                });
                return flow;
            };

            selectedSet.TriggerChange();
            FinishTransforms(true);
        });

        private bool? selected;

        private void updateSelectionState(ValueChangedEvent<Live<BeatmapSetInfo>?> selected)
        {
            bool? wasSelected = this.selected;
            this.selected = selected.NewValue?.Equals(Current.Value) == true;

            if (wasSelected == this.selected)
                return;

            text.FadeColour(this.selected == true ? colours.Yellow : Color4.White, 100);
        }

        protected override bool OnClick(ClickEvent e)
        {
            requestSelection?.Invoke(Current.Value);
            return true;
        }

        protected override bool OnHover(HoverEvent e)
        {
            text.AllowScrolling = true;
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            text.AllowScrolling = false;
            base.OnHoverLost(e);
        }
    }
}
