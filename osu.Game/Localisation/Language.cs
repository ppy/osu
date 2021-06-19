// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;

namespace osu.Game.Localisation
{
    public enum Language
    {
        [Description(@"English")]
        en,

        // TODO: Requires Arabic glyphs to be added to resources (and possibly also RTL support).
        // [Description(@"اَلْعَرَبِيَّةُ")]
        // ar,

        // TODO: Some accented glyphs are missing. Revisit when adding Inter.
        // [Description(@"Беларуская мова")]
        // be,

        [Description(@"Български")]
        bg,

        [Description(@"Česky")]
        cs,

        [Description(@"Dansk")]
        da,

        [Description(@"Deutsch")]
        de,

        // TODO: Some accented glyphs are missing. Revisit when adding Inter.
        // [Description(@"Ελληνικά")]
        // el,

        [Description(@"español")]
        es,

        [Description(@"Suomi")]
        fi,

        [Description(@"français")]
        fr,

        [Description(@"Magyar")]
        hu,

        [Description(@"Bahasa Indonesia")]
        id,

        [Description(@"Italiano")]
        it,

        [Description(@"日本語")]
        ja,

        [Description(@"한국어")]
        ko,

        [Description(@"Nederlands")]
        nl,

        [Description(@"Norsk")]
        no,

        [Description(@"polski")]
        pl,

        [Description(@"Português")]
        pt,

        [Description(@"Português (Brasil)")]
        pt_br,

        [Description(@"Română")]
        ro,

        [Description(@"Русский")]
        ru,

        [Description(@"Slovenčina")]
        sk,

        [Description(@"Svenska")]
        sv,

        [Description(@"ไทย")]
        th,

        [Description(@"Tagalog")]
        tl,

        [Description(@"Türkçe")]
        tr,

        // TODO: Some accented glyphs are missing. Revisit when adding Inter.
        // [Description(@"Українська мова")]
        // uk,

        [Description(@"Tiếng Việt")]
        vi,

        [Description(@"简体中文")]
        zh,

        [Description(@"繁體中文（香港）")]
        zh_hk,

        [Description(@"繁體中文（台灣）")]
        zh_tw
    }
}
