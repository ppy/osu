// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Direct;
using osu.Game.Users;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Profile.Sections.Beatmaps
{
    public class PaginatedBeatmapContainer : PaginatedContainer
    {
        private const float panel_padding = 10f;
        private int count;
        private readonly Counter counterDrawable;
        private readonly BeatmapSetType type;
        private GetUserBeatmapsRequest request;

        public PaginatedBeatmapContainer(BeatmapSetType type, Bindable<User> user, string header, string missing = "None... yet.")
            : base(user, header, missing)
        {
            this.type = type;

            ItemsPerPage = 6;

            ItemsContainer.Spacing = new Vector2(panel_padding);

            HeaderContainer.Add(counterDrawable = new Counter());
        }

        protected override void ShowMore()
        {
            request = new GetUserBeatmapsRequest(User.Value.Id, type, VisiblePages++ * ItemsPerPage, ItemsPerPage);
            request.Success += sets => Schedule(() =>
            {
                MoreButton.IsLoading = false;

                if (!sets.Any() && VisiblePages == 1)
                {
                    MissingText.Show();
                    return;
                }

                foreach (var s in sets)
                {
                    if (!s.OnlineBeatmapSetID.HasValue)
                        continue;

                    var panel = new DirectGridPanel(s.ToBeatmapSet(Rulesets));
                    ItemsContainer.Add(panel);
                }

                MoreButton.FadeTo(ItemsContainer.Children.Count == count ? 0 : 1);
            });

            Api.Queue(request);
        }

        protected override void OnUserChanged(ValueChangedEvent<User> e)
        {
            base.OnUserChanged(e);

            switch (type)
            {
                case BeatmapSetType.Favourite:
                    count = User.Value.FavouriteBeatmapsetCount[0];
                    break;

                case BeatmapSetType.Graveyard:
                    count = User.Value.GraveyardBeatmapsetCount[0];
                    break;

                case BeatmapSetType.Loved:
                    count = User.Value.LovedBeatmapsetCount[0];
                    break;

                case BeatmapSetType.RankedAndApproved:
                    count = User.Value.RankedAndApprovedBeatmapsetCount[0];
                    break;

                case BeatmapSetType.Unranked:
                    count = User.Value.UnrankedBeatmapsetCount[0];
                    break;
            }

            counterDrawable.Value = count;
            counterDrawable.FadeTo(count > 0 ? 1 : 0);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            request?.Cancel();
        }

        private class Counter : CircularContainer
        {
            private readonly OsuSpriteText counterText;
            private int counter;

            public int Value
            {
                get => counter;
                set
                {
                    if (counter == value)
                        return;

                    counter = value;
                    counterText.Text = value.ToString();
                }
            }
            public Counter()
            {
                Masking = true;
                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                Size = new Vector2(30);
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    },
                    counterText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = OsuFont.GetFont(size: 20, weight: FontWeight.SemiBold)
                    }
                };
            }
        }
    }
}
