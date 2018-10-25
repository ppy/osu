using osu.Core.Wiki.Sections;
using osu.Framework.Graphics.Textures;

namespace osu.Core.Wiki.Included.Home
{
    public sealed class HomeWikiSet : WikiSet
    {
        public override string Name => "Home";

        public override string Description => "Welcome to osu!lazer! We hope you enjoy all the new features lazer has to offer!";

        public override Texture HeaderBackground => SymcolOsuModSet.LazerTextures.Get("Backgrounds/bg2");

        public override WikiSection[] GetSections() => new WikiSection[]
        {
            new WhatIsTheWiki(),
            new Access()
        };
    }
}
