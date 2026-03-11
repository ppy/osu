// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public abstract partial class RankedPlaySubScreen : Container
    {
        public const float CENTERED_CARD_SCALE = 1.2f;

        public readonly Bindable<Visibility> CornerPieceVisibility = new Bindable<Visibility>(Visibility.Visible);
        protected readonly Bindable<Visibility> CountdownVisibility = new Bindable<Visibility>(Visibility.Visible);

        public virtual bool ShowBeatmapBackground => false;

        /// <summary>
        /// Heading text to be displayed indicating the purpose of the current stage.
        /// </summary>
        protected abstract LocalisableString StageHeading { get; }

        /// <summary>
        /// Subtitle text to be displayed indicating the action a user should take in the current stage.
        /// </summary>
        protected abstract LocalisableString StageCaption { get; }

        /// <summary>
        /// The colour scheme commonly used for components of this screen.
        /// </summary>
        protected virtual RankedPlayColourScheme ColourScheme => RankedPlayColourScheme.Blue;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        protected MultiplayerClient Client => client;

        protected override Container<Drawable> Content { get; }
        protected readonly Container CenterColumn;
        protected readonly FillFlowContainer ButtonsContainer;
        protected readonly RankedPlayStageDisplay StageDisplay;

        protected RankedPlaySubScreen()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren =
            [
                CenterColumn = new Container
                {
                    Name = "Center Column",
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding(20),
                },
                Content = new Container
                {
                    Name = "Content",
                    RelativeSizeAxes = Axes.Both,
                },
                ButtonsContainer = new FillFlowContainer
                {
                    Name = "Buttons",
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    X = 30,
                    Y = -110,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(8)
                },
                StageDisplay = new RankedPlayStageDisplay(ColourScheme)
                {
                    Heading = StageHeading,
                    Caption = StageCaption,
                    Margin = new MarginPadding { Top = 60 },
                    State = { BindTarget = CountdownVisibility }
                },
            ];
        }

        protected override void Update()
        {
            base.Update();

            CenterColumn.Width = DrawWidth - RankedPlayCornerPiece.WidthFor(DrawWidth) * 2;
        }

        public virtual void OnEntering(RankedPlaySubScreen? previous)
        {
        }

        public virtual void OnExiting(RankedPlaySubScreen? next)
        {
            Hide();
        }

        protected static string FormatRoundIndex(int roundNumber)
        {
            return roundNumber >= 10 ? roundNumber.Ordinalize(CultureInfo.InvariantCulture) : roundNumber.ToOrdinalWords(CultureInfo.InvariantCulture);
        }
    }
}
