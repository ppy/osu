// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Users;

namespace osu.Game.Tournament
{
    public static class CountryExtensions
    {
        public static string GetAcronym(this Country country)
        {
            switch (country)
            {
                case Country.BD:
                    return "BGD";

                case Country.BE:
                    return "BEL";

                case Country.BF:
                    return "BFA";

                case Country.BG:
                    return "BGR";

                case Country.BA:
                    return "BIH";

                case Country.BB:
                    return "BRB";

                case Country.WF:
                    return "WLF";

                case Country.BL:
                    return "BLM";

                case Country.BM:
                    return "BMU";

                case Country.BN:
                    return "BRN";

                case Country.BO:
                    return "BOL";

                case Country.BH:
                    return "BHR";

                case Country.BI:
                    return "BDI";

                case Country.BJ:
                    return "BEN";

                case Country.BT:
                    return "BTN";

                case Country.JM:
                    return "JAM";

                case Country.BV:
                    return "BVT";

                case Country.BW:
                    return "BWA";

                case Country.WS:
                    return "WSM";

                case Country.BQ:
                    return "BES";

                case Country.BR:
                    return "BRA";

                case Country.BS:
                    return "BHS";

                case Country.JE:
                    return "JEY";

                case Country.BY:
                    return "BLR";

                case Country.BZ:
                    return "BLZ";

                case Country.RU:
                    return "RUS";

                case Country.RW:
                    return "RWA";

                case Country.RS:
                    return "SRB";

                case Country.TL:
                    return "TLS";

                case Country.RE:
                    return "REU";

                case Country.TM:
                    return "TKM";

                case Country.TJ:
                    return "TJK";

                case Country.RO:
                    return "ROU";

                case Country.TK:
                    return "TKL";

                case Country.GW:
                    return "GNB";

                case Country.GU:
                    return "GUM";

                case Country.GT:
                    return "GTM";

                case Country.GS:
                    return "SGS";

                case Country.GR:
                    return "GRC";

                case Country.GQ:
                    return "GNQ";

                case Country.GP:
                    return "GLP";

                case Country.JP:
                    return "JPN";

                case Country.GY:
                    return "GUY";

                case Country.GG:
                    return "GGY";

                case Country.GF:
                    return "GUF";

                case Country.GE:
                    return "GEO";

                case Country.GD:
                    return "GRD";

                case Country.GB:
                    return "GBR";

                case Country.GA:
                    return "GAB";

                case Country.SV:
                    return "SLV";

                case Country.GN:
                    return "GIN";

                case Country.GM:
                    return "GMB";

                case Country.GL:
                    return "GRL";

                case Country.GI:
                    return "GIB";

                case Country.GH:
                    return "GHA";

                case Country.OM:
                    return "OMN";

                case Country.TN:
                    return "TUN";

                case Country.JO:
                    return "JOR";

                case Country.HR:
                    return "HRV";

                case Country.HT:
                    return "HTI";

                case Country.HU:
                    return "HUN";

                case Country.HK:
                    return "HKG";

                case Country.HN:
                    return "HND";

                case Country.HM:
                    return "HMD";

                case Country.VE:
                    return "VEN";

                case Country.PR:
                    return "PRI";

                case Country.PS:
                    return "PSE";

                case Country.PW:
                    return "PLW";

                case Country.PT:
                    return "PRT";

                case Country.SJ:
                    return "SJM";

                case Country.PY:
                    return "PRY";

                case Country.IQ:
                    return "IRQ";

                case Country.PA:
                    return "PAN";

                case Country.PF:
                    return "PYF";

                case Country.PG:
                    return "PNG";

                case Country.PE:
                    return "PER";

                case Country.PK:
                    return "PAK";

                case Country.PH:
                    return "PHL";

                case Country.PN:
                    return "PCN";

                case Country.PL:
                    return "POL";

                case Country.PM:
                    return "SPM";

                case Country.ZM:
                    return "ZMB";

                case Country.EH:
                    return "ESH";

                case Country.EE:
                    return "EST";

                case Country.EG:
                    return "EGY";

                case Country.ZA:
                    return "ZAF";

                case Country.EC:
                    return "ECU";

                case Country.IT:
                    return "ITA";

                case Country.VN:
                    return "VNM";

                case Country.SB:
                    return "SLB";

                case Country.ET:
                    return "ETH";

                case Country.SO:
                    return "SOM";

                case Country.ZW:
                    return "ZWE";

                case Country.SA:
                    return "SAU";

                case Country.ES:
                    return "ESP";

                case Country.ER:
                    return "ERI";

                case Country.ME:
                    return "MNE";

                case Country.MD:
                    return "MDA";

                case Country.MG:
                    return "MDG";

                case Country.MF:
                    return "MAF";

                case Country.MA:
                    return "MAR";

                case Country.MC:
                    return "MCO";

                case Country.UZ:
                    return "UZB";

                case Country.MM:
                    return "MMR";

                case Country.ML:
                    return "MLI";

                case Country.MO:
                    return "MAC";

                case Country.MN:
                    return "MNG";

                case Country.MH:
                    return "MHL";

                case Country.MK:
                    return "MKD";

                case Country.MU:
                    return "MUS";

                case Country.MT:
                    return "MLT";

                case Country.MW:
                    return "MWI";

                case Country.MV:
                    return "MDV";

                case Country.MQ:
                    return "MTQ";

                case Country.MP:
                    return "MNP";

                case Country.MS:
                    return "MSR";

                case Country.MR:
                    return "MRT";

                case Country.IM:
                    return "IMN";

                case Country.UG:
                    return "UGA";

                case Country.TZ:
                    return "TZA";

                case Country.MY:
                    return "MYS";

                case Country.MX:
                    return "MEX";

                case Country.IL:
                    return "ISR";

                case Country.FR:
                    return "FRA";

                case Country.IO:
                    return "IOT";

                case Country.SH:
                    return "SHN";

                case Country.FI:
                    return "FIN";

                case Country.FJ:
                    return "FJI";

                case Country.FK:
                    return "FLK";

                case Country.FM:
                    return "FSM";

                case Country.FO:
                    return "FRO";

                case Country.NI:
                    return "NIC";

                case Country.NL:
                    return "NLD";

                case Country.NO:
                    return "NOR";

                case Country.NA:
                    return "NAM";

                case Country.VU:
                    return "VUT";

                case Country.NC:
                    return "NCL";

                case Country.NE:
                    return "NER";

                case Country.NF:
                    return "NFK";

                case Country.NG:
                    return "NGA";

                case Country.NZ:
                    return "NZL";

                case Country.NP:
                    return "NPL";

                case Country.NR:
                    return "NRU";

                case Country.NU:
                    return "NIU";

                case Country.CK:
                    return "COK";

                case Country.XK:
                    return "XKX";

                case Country.CI:
                    return "CIV";

                case Country.CH:
                    return "CHE";

                case Country.CO:
                    return "COL";

                case Country.CN:
                    return "CHN";

                case Country.CM:
                    return "CMR";

                case Country.CL:
                    return "CHL";

                case Country.CC:
                    return "CCK";

                case Country.CA:
                    return "CAN";

                case Country.CG:
                    return "COG";

                case Country.CF:
                    return "CAF";

                case Country.CD:
                    return "COD";

                case Country.CZ:
                    return "CZE";

                case Country.CY:
                    return "CYP";

                case Country.CX:
                    return "CXR";

                case Country.CR:
                    return "CRI";

                case Country.CW:
                    return "CUW";

                case Country.CV:
                    return "CPV";

                case Country.CU:
                    return "CUB";

                case Country.SZ:
                    return "SWZ";

                case Country.SY:
                    return "SYR";

                case Country.SX:
                    return "SXM";

                case Country.KG:
                    return "KGZ";

                case Country.KE:
                    return "KEN";

                case Country.SS:
                    return "SSD";

                case Country.SR:
                    return "SUR";

                case Country.KI:
                    return "KIR";

                case Country.KH:
                    return "KHM";

                case Country.KN:
                    return "KNA";

                case Country.KM:
                    return "COM";

                case Country.ST:
                    return "STP";

                case Country.SK:
                    return "SVK";

                case Country.KR:
                    return "KOR";

                case Country.SI:
                    return "SVN";

                case Country.KP:
                    return "PRK";

                case Country.KW:
                    return "KWT";

                case Country.SN:
                    return "SEN";

                case Country.SM:
                    return "SMR";

                case Country.SL:
                    return "SLE";

                case Country.SC:
                    return "SYC";

                case Country.KZ:
                    return "KAZ";

                case Country.KY:
                    return "CYM";

                case Country.SG:
                    return "SGP";

                case Country.SE:
                    return "SWE";

                case Country.SD:
                    return "SDN";

                case Country.DO:
                    return "DOM";

                case Country.DM:
                    return "DMA";

                case Country.DJ:
                    return "DJI";

                case Country.DK:
                    return "DNK";

                case Country.VG:
                    return "VGB";

                case Country.DE:
                    return "DEU";

                case Country.YE:
                    return "YEM";

                case Country.DZ:
                    return "DZA";

                case Country.US:
                    return "USA";

                case Country.UY:
                    return "URY";

                case Country.YT:
                    return "MYT";

                case Country.UM:
                    return "UMI";

                case Country.LB:
                    return "LBN";

                case Country.LC:
                    return "LCA";

                case Country.LA:
                    return "LAO";

                case Country.TV:
                    return "TUV";

                case Country.TW:
                    return "TWN";

                case Country.TT:
                    return "TTO";

                case Country.TR:
                    return "TUR";

                case Country.LK:
                    return "LKA";

                case Country.LI:
                    return "LIE";

                case Country.LV:
                    return "LVA";

                case Country.TO:
                    return "TON";

                case Country.LT:
                    return "LTU";

                case Country.LU:
                    return "LUX";

                case Country.LR:
                    return "LBR";

                case Country.LS:
                    return "LSO";

                case Country.TH:
                    return "THA";

                case Country.TF:
                    return "ATF";

                case Country.TG:
                    return "TGO";

                case Country.TD:
                    return "TCD";

                case Country.TC:
                    return "TCA";

                case Country.LY:
                    return "LBY";

                case Country.VA:
                    return "VAT";

                case Country.VC:
                    return "VCT";

                case Country.AE:
                    return "ARE";

                case Country.AD:
                    return "AND";

                case Country.AG:
                    return "ATG";

                case Country.AF:
                    return "AFG";

                case Country.AI:
                    return "AIA";

                case Country.VI:
                    return "VIR";

                case Country.IS:
                    return "ISL";

                case Country.IR:
                    return "IRN";

                case Country.AM:
                    return "ARM";

                case Country.AL:
                    return "ALB";

                case Country.AO:
                    return "AGO";

                case Country.AQ:
                    return "ATA";

                case Country.AS:
                    return "ASM";

                case Country.AR:
                    return "ARG";

                case Country.AU:
                    return "AUS";

                case Country.AT:
                    return "AUT";

                case Country.AW:
                    return "ABW";

                case Country.IN:
                    return "IND";

                case Country.AX:
                    return "ALA";

                case Country.AZ:
                    return "AZE";

                case Country.IE:
                    return "IRL";

                case Country.ID:
                    return "IDN";

                case Country.UA:
                    return "UKR";

                case Country.QA:
                    return "QAT";

                case Country.MZ:
                    return "MOZ";

                default:
                    throw new ArgumentOutOfRangeException(nameof(country));
            }
        }
    }
}
