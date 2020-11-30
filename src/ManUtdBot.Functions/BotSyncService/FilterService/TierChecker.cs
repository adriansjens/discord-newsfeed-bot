using System.Linq;

namespace ManUtdBot.Functions.BotSyncService.FilterService
{
    public static class TierChecker
    {
        public static int CheckTier(string displayName)
        {
            var tier1 = new[]
            {
                "FabrizioRomano", "sistoney67", "TelegraphDucker", "SimonPeach", "CraigNorwood", "henrywinter",
                "mohamedbouhafsi", "_pauljoyce", "HLNinEngeland", "SamLee", "JPercyTelegraph", "BBCSport"
            };
            var tier2 = new[]
            {
                "BILD", "BILD_English", "kicker_ENG", "telegraaf", "COPE", "honigstein", "VI_nl", "Matt_Law_DT",
                "La_SER", "RMCsport", "telefoot_TF1", "gerardromero", "lee_ryder", "hirstclass", "CMVM_pt",
                "David_Ornstein", "SPORT1", "Telegraph", "mcgrathmike", "cfbayern"
            };
            var tier3 = new[]
            {
                "rtppt", "PierreMenes", "JamieJackson___", "Ian_Ladyman_DM", "guardian", "thetimes", "MarkOgden_",
                "DTathletic", "RNBVB", "RN_S04", "le_Parisien", "ianherbs", "JBurtTelegraph", "JimWhite",
                "RobDawsonESPN", "ojogo", "lauriewhitwell", "AdrianJKajumba", "DanielHarris", "AlfredoPedulla",
                "MikeKeegan_DM", "AndyMitten", "SkyKaveh", "JulienMaynard"
            };

            return tier1.Contains(displayName) ? 1 :
                   tier2.Contains(displayName) ? 2 :
                   tier3.Contains(displayName) ? 3 : 0;
        }
    }
}
