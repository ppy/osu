// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Screens;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Overlays.Dashboard.CurrentlyOnline
{
    internal partial class OnlineUserGridPanel : CompositeDrawable, IFilterable
    {
        public readonly APIUser User;

        private PurpleRoundedButton spectateButton = null!;

        public IEnumerable<LocalisableString> FilterTerms { get; }

        [Resolved]
        private IPerformFromScreenRunner? performer { get; set; }

        [Resolved]
        private MetadataClient? metadataClient { get; set; }

        public bool FilteringActive { set; get; }

        public bool MatchingFilter
        {
            set
            {
                if (value)
                    Show();
                else
                    Hide();
            }
        }

        public OnlineUserGridPanel(APIUser user)
        {
            User = user;

            FilterTerms = new LocalisableString[] { User.Username };

            AutoSizeAxes = Axes.Both;
        }

        protected override void Update()
        {
            base.Update();

            // TODO: we probably don't want to do this every frame.
            var activity = metadataClient?.GetPresence(User.Id)?.Activity;

            switch (activity)
            {
                default:
                    spectateButton.Enabled.Value = false;
                    break;

                case UserActivity.InSoloGame:
                case UserActivity.InMultiplayerGame:
                case UserActivity.InPlaylistGame:
                    spectateButton.Enabled.Value = true;
                    break;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(2),
                    Width = 290,
                    Children = new Drawable[]
                    {
                        new UserGridPanel(User)
                        {
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        },
                        spectateButton = new PurpleRoundedButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Spectate",
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Action = () => performer?.PerformFromScreen(s => s.Push(new SoloSpectatorScreen(User))),
                        }
                    }
                },
            };
        }
    }
}
