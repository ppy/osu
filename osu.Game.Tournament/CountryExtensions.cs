// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Users;

namespace osu.Game.Tournament
{
    public static class CountryExtensions
    {
        public static string GetAcronym(this CountryCode country)
        {
            switch (country)
            {
                case CountryCode.BD:
                    return "BGD";

                case CountryCode.BE:
                    return "BEL";

                case CountryCode.BF:
                    return "BFA";

                case CountryCode.BG:
                    return "BGR";

                case CountryCode.BA:
                    return "BIH";

                case CountryCode.BB:
                    return "BRB";

                case CountryCode.WF:
                    return "WLF";

                case CountryCode.BL:
                    return "BLM";

                case CountryCode.BM:
                    return "BMU";

                case CountryCode.BN:
                    return "BRN";

                case CountryCode.BO:
                    return "BOL";

                case CountryCode.BH:
                    return "BHR";

                case CountryCode.BI:
                    return "BDI";

                case CountryCode.BJ:
                    return "BEN";

                case CountryCode.BT:
                    return "BTN";

                case CountryCode.JM:
                    return "JAM";

                case CountryCode.BV:
                    return "BVT";

                case CountryCode.BW:
                    return "BWA";

                case CountryCode.WS:
                    return "WSM";

                case CountryCode.BQ:
                    return "BES";

                case CountryCode.BR:
                    return "BRA";

                case CountryCode.BS:
                    return "BHS";

                case CountryCode.JE:
                    return "JEY";

                case CountryCode.BY:
                    return "BLR";

                case CountryCode.BZ:
                    return "BLZ";

                case CountryCode.RU:
                    return "RUS";

                case CountryCode.RW:
                    return "RWA";

                case CountryCode.RS:
                    return "SRB";

                case CountryCode.TL:
                    return "TLS";

                case CountryCode.RE:
                    return "REU";

                case CountryCode.TM:
                    return "TKM";

                case CountryCode.TJ:
                    return "TJK";

                case CountryCode.RO:
                    return "ROU";

                case CountryCode.TK:
                    return "TKL";

                case CountryCode.GW:
                    return "GNB";

                case CountryCode.GU:
                    return "GUM";

                case CountryCode.GT:
                    return "GTM";

                case CountryCode.GS:
                    return "SGS";

                case CountryCode.GR:
                    return "GRC";

                case CountryCode.GQ:
                    return "GNQ";

                case CountryCode.GP:
                    return "GLP";

                case CountryCode.JP:
                    return "JPN";

                case CountryCode.GY:
                    return "GUY";

                case CountryCode.GG:
                    return "GGY";

                case CountryCode.GF:
                    return "GUF";

                case CountryCode.GE:
                    return "GEO";

                case CountryCode.GD:
                    return "GRD";

                case CountryCode.GB:
                    return "GBR";

                case CountryCode.GA:
                    return "GAB";

                case CountryCode.SV:
                    return "SLV";

                case CountryCode.GN:
                    return "GIN";

                case CountryCode.GM:
                    return "GMB";

                case CountryCode.GL:
                    return "GRL";

                case CountryCode.GI:
                    return "GIB";

                case CountryCode.GH:
                    return "GHA";

                case CountryCode.OM:
                    return "OMN";

                case CountryCode.TN:
                    return "TUN";

                case CountryCode.JO:
                    return "JOR";

                case CountryCode.HR:
                    return "HRV";

                case CountryCode.HT:
                    return "HTI";

                case CountryCode.HU:
                    return "HUN";

                case CountryCode.HK:
                    return "HKG";

                case CountryCode.HN:
                    return "HND";

                case CountryCode.HM:
                    return "HMD";

                case CountryCode.VE:
                    return "VEN";

                case CountryCode.PR:
                    return "PRI";

                case CountryCode.PS:
                    return "PSE";

                case CountryCode.PW:
                    return "PLW";

                case CountryCode.PT:
                    return "PRT";

                case CountryCode.SJ:
                    return "SJM";

                case CountryCode.PY:
                    return "PRY";

                case CountryCode.IQ:
                    return "IRQ";

                case CountryCode.PA:
                    return "PAN";

                case CountryCode.PF:
                    return "PYF";

                case CountryCode.PG:
                    return "PNG";

                case CountryCode.PE:
                    return "PER";

                case CountryCode.PK:
                    return "PAK";

                case CountryCode.PH:
                    return "PHL";

                case CountryCode.PN:
                    return "PCN";

                case CountryCode.PL:
                    return "POL";

                case CountryCode.PM:
                    return "SPM";

                case CountryCode.ZM:
                    return "ZMB";

                case CountryCode.EH:
                    return "ESH";

                case CountryCode.EE:
                    return "EST";

                case CountryCode.EG:
                    return "EGY";

                case CountryCode.ZA:
                    return "ZAF";

                case CountryCode.EC:
                    return "ECU";

                case CountryCode.IT:
                    return "ITA";

                case CountryCode.VN:
                    return "VNM";

                case CountryCode.SB:
                    return "SLB";

                case CountryCode.ET:
                    return "ETH";

                case CountryCode.SO:
                    return "SOM";

                case CountryCode.ZW:
                    return "ZWE";

                case CountryCode.SA:
                    return "SAU";

                case CountryCode.ES:
                    return "ESP";

                case CountryCode.ER:
                    return "ERI";

                case CountryCode.ME:
                    return "MNE";

                case CountryCode.MD:
                    return "MDA";

                case CountryCode.MG:
                    return "MDG";

                case CountryCode.MF:
                    return "MAF";

                case CountryCode.MA:
                    return "MAR";

                case CountryCode.MC:
                    return "MCO";

                case CountryCode.UZ:
                    return "UZB";

                case CountryCode.MM:
                    return "MMR";

                case CountryCode.ML:
                    return "MLI";

                case CountryCode.MO:
                    return "MAC";

                case CountryCode.MN:
                    return "MNG";

                case CountryCode.MH:
                    return "MHL";

                case CountryCode.MK:
                    return "MKD";

                case CountryCode.MU:
                    return "MUS";

                case CountryCode.MT:
                    return "MLT";

                case CountryCode.MW:
                    return "MWI";

                case CountryCode.MV:
                    return "MDV";

                case CountryCode.MQ:
                    return "MTQ";

                case CountryCode.MP:
                    return "MNP";

                case CountryCode.MS:
                    return "MSR";

                case CountryCode.MR:
                    return "MRT";

                case CountryCode.IM:
                    return "IMN";

                case CountryCode.UG:
                    return "UGA";

                case CountryCode.TZ:
                    return "TZA";

                case CountryCode.MY:
                    return "MYS";

                case CountryCode.MX:
                    return "MEX";

                case CountryCode.IL:
                    return "ISR";

                case CountryCode.FR:
                    return "FRA";

                case CountryCode.IO:
                    return "IOT";

                case CountryCode.SH:
                    return "SHN";

                case CountryCode.FI:
                    return "FIN";

                case CountryCode.FJ:
                    return "FJI";

                case CountryCode.FK:
                    return "FLK";

                case CountryCode.FM:
                    return "FSM";

                case CountryCode.FO:
                    return "FRO";

                case CountryCode.NI:
                    return "NIC";

                case CountryCode.NL:
                    return "NLD";

                case CountryCode.NO:
                    return "NOR";

                case CountryCode.NA:
                    return "NAM";

                case CountryCode.VU:
                    return "VUT";

                case CountryCode.NC:
                    return "NCL";

                case CountryCode.NE:
                    return "NER";

                case CountryCode.NF:
                    return "NFK";

                case CountryCode.NG:
                    return "NGA";

                case CountryCode.NZ:
                    return "NZL";

                case CountryCode.NP:
                    return "NPL";

                case CountryCode.NR:
                    return "NRU";

                case CountryCode.NU:
                    return "NIU";

                case CountryCode.CK:
                    return "COK";

                case CountryCode.XK:
                    return "XKX";

                case CountryCode.CI:
                    return "CIV";

                case CountryCode.CH:
                    return "CHE";

                case CountryCode.CO:
                    return "COL";

                case CountryCode.CN:
                    return "CHN";

                case CountryCode.CM:
                    return "CMR";

                case CountryCode.CL:
                    return "CHL";

                case CountryCode.CC:
                    return "CCK";

                case CountryCode.CA:
                    return "CAN";

                case CountryCode.CG:
                    return "COG";

                case CountryCode.CF:
                    return "CAF";

                case CountryCode.CD:
                    return "COD";

                case CountryCode.CZ:
                    return "CZE";

                case CountryCode.CY:
                    return "CYP";

                case CountryCode.CX:
                    return "CXR";

                case CountryCode.CR:
                    return "CRI";

                case CountryCode.CW:
                    return "CUW";

                case CountryCode.CV:
                    return "CPV";

                case CountryCode.CU:
                    return "CUB";

                case CountryCode.SZ:
                    return "SWZ";

                case CountryCode.SY:
                    return "SYR";

                case CountryCode.SX:
                    return "SXM";

                case CountryCode.KG:
                    return "KGZ";

                case CountryCode.KE:
                    return "KEN";

                case CountryCode.SR:
                    return "SUR";

                case CountryCode.KI:
                    return "KIR";

                case CountryCode.KH:
                    return "KHM";

                case CountryCode.KN:
                    return "KNA";

                case CountryCode.KM:
                    return "COM";

                case CountryCode.ST:
                    return "STP";

                case CountryCode.SK:
                    return "SVK";

                case CountryCode.KR:
                    return "KOR";

                case CountryCode.SI:
                    return "SVN";

                case CountryCode.KP:
                    return "PRK";

                case CountryCode.KW:
                    return "KWT";

                case CountryCode.SN:
                    return "SEN";

                case CountryCode.SM:
                    return "SMR";

                case CountryCode.SL:
                    return "SLE";

                case CountryCode.SC:
                    return "SYC";

                case CountryCode.KZ:
                    return "KAZ";

                case CountryCode.KY:
                    return "CYM";

                case CountryCode.SG:
                    return "SGP";

                case CountryCode.SE:
                    return "SWE";

                case CountryCode.SD:
                    return "SDN";

                case CountryCode.DO:
                    return "DOM";

                case CountryCode.DM:
                    return "DMA";

                case CountryCode.DJ:
                    return "DJI";

                case CountryCode.DK:
                    return "DNK";

                case CountryCode.VG:
                    return "VGB";

                case CountryCode.DE:
                    return "DEU";

                case CountryCode.YE:
                    return "YEM";

                case CountryCode.DZ:
                    return "DZA";

                case CountryCode.US:
                    return "USA";

                case CountryCode.UY:
                    return "URY";

                case CountryCode.YT:
                    return "MYT";

                case CountryCode.UM:
                    return "UMI";

                case CountryCode.LB:
                    return "LBN";

                case CountryCode.LC:
                    return "LCA";

                case CountryCode.LA:
                    return "LAO";

                case CountryCode.TV:
                    return "TUV";

                case CountryCode.TW:
                    return "TWN";

                case CountryCode.TT:
                    return "TTO";

                case CountryCode.TR:
                    return "TUR";

                case CountryCode.LK:
                    return "LKA";

                case CountryCode.LI:
                    return "LIE";

                case CountryCode.LV:
                    return "LVA";

                case CountryCode.TO:
                    return "TON";

                case CountryCode.LT:
                    return "LTU";

                case CountryCode.LU:
                    return "LUX";

                case CountryCode.LR:
                    return "LBR";

                case CountryCode.LS:
                    return "LSO";

                case CountryCode.TH:
                    return "THA";

                case CountryCode.TF:
                    return "ATF";

                case CountryCode.TG:
                    return "TGO";

                case CountryCode.TD:
                    return "TCD";

                case CountryCode.TC:
                    return "TCA";

                case CountryCode.LY:
                    return "LBY";

                case CountryCode.VA:
                    return "VAT";

                case CountryCode.VC:
                    return "VCT";

                case CountryCode.AE:
                    return "ARE";

                case CountryCode.AD:
                    return "AND";

                case CountryCode.AG:
                    return "ATG";

                case CountryCode.AF:
                    return "AFG";

                case CountryCode.AI:
                    return "AIA";

                case CountryCode.VI:
                    return "VIR";

                case CountryCode.IS:
                    return "ISL";

                case CountryCode.IR:
                    return "IRN";

                case CountryCode.AM:
                    return "ARM";

                case CountryCode.AL:
                    return "ALB";

                case CountryCode.AO:
                    return "AGO";

                case CountryCode.AQ:
                    return "ATA";

                case CountryCode.AS:
                    return "ASM";

                case CountryCode.AR:
                    return "ARG";

                case CountryCode.AU:
                    return "AUS";

                case CountryCode.AT:
                    return "AUT";

                case CountryCode.AW:
                    return "ABW";

                case CountryCode.IN:
                    return "IND";

                case CountryCode.AX:
                    return "ALA";

                case CountryCode.AZ:
                    return "AZE";

                case CountryCode.IE:
                    return "IRL";

                case CountryCode.ID:
                    return "IDN";

                case CountryCode.UA:
                    return "UKR";

                case CountryCode.QA:
                    return "QAT";

                case CountryCode.MZ:
                    return "MOZ";

                default:
                    return country.ToString();
            }
        }
    }
}
