using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GuildWarsWalletFunctions.GuildWarsModels;
using GuildWarsWalletFunctions.TrackerModels;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GuildWarsWalletFunctions
{
    public class GetWalletInfo
    {
        private readonly HttpClient _client;

        public GetWalletInfo(IHttpClientFactory httpClientFactory)
        {
            this._client = httpClientFactory.CreateClient();
        }

        [FunctionName("GetWalletInfo")]
        public async Task Run([ServiceBusTrigger("devdailyqueue", Connection = "SbConnectString")]string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

            TrackedPerson messagePerson = JsonConvert.DeserializeObject<TrackedPerson>(myQueueItem);

            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {messagePerson.ApiKey}");
            string testString = await _client.GetStringAsync("https://api.guildwars2.com/v2/account/wallet");

            List<WalletValue> walletValues = JsonConvert.DeserializeObject<List<WalletValue>>(testString);
            Dictionary<int, long> walletIdValueDict = walletValues.ToDictionary(x => x.Id, x => x.Value);

            string connectString = System.Environment.GetEnvironmentVariable("DbConnectString");

            using (SqlConnection connection = new SqlConnection(connectString))
            {
                SqlCommand saveCmd = new SqlCommand(@"
                    INSERT INTO guild.Wallet
                    (
                        NickName,
                        EntryDate,
                        Coin,
                        Karma,
                        Laurel,
                        Gem,
                        FractalRelic,
                        BadgeOfHonor,
                        GuildCommendation,
                        TransmutationCharge,
                        AirshipPart,
                        LeyLineCrystal,
                        LumpOfAurillium,
                        SpiritShard,
                        PristineFractalRelic,
                        Geode,
                        WvWSkirmishClaimTicket,
                        BanditCrest,
                        MagnetiteShard,
                        ProvisionerToken,
                        PvPLeagueTicket,
                        ProofOfHeroics,
                        UnboundMagic,
                        AscendedShardsOfGlory,
                        TradeContract,
                        ElegyMosaic,
                        TestimonyOfDesertHeroics,
                        ExaltedKey,
                        Machete,
                        GaetingCrystal,
                        BanditSkeletonKey,
                        PactCrowbar,
                        VialOfChakAcid,
                        ZephyriteLockpick,
                        TradersKey,
                        VolatileMagic,
                        PvPTournamentVoucher,
                        RacingMedallion,
                        MistbornKey,
                        FestivalToken,
                        CacheKey,
                        RedProphetShard,
                        GreenProphetShard,
                        BlueProphetCrystal,
                        GreenProphetCrystal,
                        RedProphetCrystal,
                        BlueProphetShard,
                        WarSupplies,
                        UnstableFractalEssence,
                        TyrianDefenseSeal,
                        ResearchNote,
                        UnusualCoin,
                        JadeSliver,
                        TestimonyOfJadeHeroics,
                        CanachCoins,
                        ImperialFavor,
                        TalesOfDungeonDelving,
                        LegendaryInsight
                    )
                    VALUES
                    (   @NickName,
                        @EntryDate,
                        @Coin,
                        @Karma,
                        @Laurel,
                        @Gem,
                        @FractalRelic,
                        @BadgeOfHonor,
                        @GuildCommendation,
                        @TransmutationCharge,
                        @AirshipPart,
                        @LeyLineCrystal,
                        @LumpOfAurillium,
                        @SpiritShard,
                        @PristineFractalRelic,
                        @Geode,
                        @WvWSkirmishClaimTicket,
                        @BanditCrest,
                        @MagnetiteShard,
                        @ProvisionerToken,
                        @PvPLeagueTicket,
                        @ProofOfHeroics,
                        @UnboundMagic,
                        @AscendedShardsOfGlory,
                        @TradeContract,
                        @ElegyMosaic,
                        @TestimonyOfDesertHeroics,
                        @ExaltedKey,
                        @Machete,
                        @GaetingCrystal,
                        @BanditSkeletonKey,
                        @PactCrowbar,
                        @VialOfChakAcid,
                        @ZephyriteLockpick,
                        @TradersKey,
                        @VolatileMagic,
                        @PvPTournamentVoucher,
                        @RacingMedallion,
                        @MistbornKey,
                        @FestivalToken,
                        @CacheKey,
                        @RedProphetShard,
                        @GreenProphetShard,
                        @BlueProphetCrystal,
                        @GreenProphetCrystal,
                        @RedProphetCrystal,
                        @BlueProphetShard,
                        @WarSupplies,
                        @UnstableFractalEssence,
                        @TyrianDefenseSeal,
                        @ResearchNote,
                        @UnusualCoin,
                        @JadeSliver,
                        @TestimonyOfJadeHeroics,
                        @CanachCoins,
                        @ImperialFavor,
                        @TalesOfDungeonDelving,
                        @LegendaryInsight
                        )", connection);

                saveCmd.Parameters.AddWithValue("@NickName", messagePerson.NickName);
                saveCmd.Parameters.AddWithValue("@EntryDate", DateTime.UtcNow.Date);
                saveCmd.Parameters.AddWithValue("@Coin", walletIdValueDict.ContainsKey(1) ? walletIdValueDict[1] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@Karma", walletIdValueDict.ContainsKey(2) ? walletIdValueDict[2] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@Laurel", walletIdValueDict.ContainsKey(3) ? walletIdValueDict[3] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@Gem", walletIdValueDict.ContainsKey(4) ? walletIdValueDict[4] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@FractalRelic", walletIdValueDict.ContainsKey(7) ? walletIdValueDict[7] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@BadgeOfHonor", walletIdValueDict.ContainsKey(15) ? walletIdValueDict[15] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@GuildCommendation", walletIdValueDict.ContainsKey(16) ? walletIdValueDict[16] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@TransmutationCharge", walletIdValueDict.ContainsKey(18) ? walletIdValueDict[18] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@AirshipPart", walletIdValueDict.ContainsKey(19) ? walletIdValueDict[19] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@LeyLineCrystal", walletIdValueDict.ContainsKey(20) ? walletIdValueDict[20] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@LumpOfAurillium", walletIdValueDict.ContainsKey(22) ? walletIdValueDict[22] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@SpiritShard", walletIdValueDict.ContainsKey(23) ? walletIdValueDict[23] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@PristineFractalRelic", walletIdValueDict.ContainsKey(24) ? walletIdValueDict[24] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@Geode", walletIdValueDict.ContainsKey(25) ? walletIdValueDict[25] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@WvWSkirmishClaimTicket", walletIdValueDict.ContainsKey(26) ? walletIdValueDict[26] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@BanditCrest", walletIdValueDict.ContainsKey(27) ? walletIdValueDict[27] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@MagnetiteShard", walletIdValueDict.ContainsKey(28) ? walletIdValueDict[28] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@ProvisionerToken", walletIdValueDict.ContainsKey(29) ? walletIdValueDict[29] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@PvPLeagueTicket", walletIdValueDict.ContainsKey(30) ? walletIdValueDict[30] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@ProofOfHeroics", walletIdValueDict.ContainsKey(31) ? walletIdValueDict[31] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@UnboundMagic", walletIdValueDict.ContainsKey(32) ? walletIdValueDict[32] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@AscendedShardsOfGlory", walletIdValueDict.ContainsKey(33) ? walletIdValueDict[33] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@TradeContract", walletIdValueDict.ContainsKey(34) ? walletIdValueDict[34] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@ElegyMosaic", walletIdValueDict.ContainsKey(35) ? walletIdValueDict[35] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@TestimonyOfDesertHeroics", walletIdValueDict.ContainsKey(36) ? walletIdValueDict[36] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@ExaltedKey", walletIdValueDict.ContainsKey(37) ? walletIdValueDict[37] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@Machete", walletIdValueDict.ContainsKey(38) ? walletIdValueDict[38] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@GaetingCrystal", walletIdValueDict.ContainsKey(39) ? walletIdValueDict[39] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@BanditSkeletonKey", walletIdValueDict.ContainsKey(40) ? walletIdValueDict[40] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@PactCrowbar", walletIdValueDict.ContainsKey(41) ? walletIdValueDict[41] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@VialOfChakAcid", walletIdValueDict.ContainsKey(42) ? walletIdValueDict[42] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@ZephyriteLockpick", walletIdValueDict.ContainsKey(43) ? walletIdValueDict[43] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@TradersKey", walletIdValueDict.ContainsKey(44) ? walletIdValueDict[44] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@VolatileMagic", walletIdValueDict.ContainsKey(45) ? walletIdValueDict[45] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@PvPTournamentVoucher", walletIdValueDict.ContainsKey(46) ? walletIdValueDict[46] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@RacingMedallion", walletIdValueDict.ContainsKey(47) ? walletIdValueDict[47] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@MistbornKey", walletIdValueDict.ContainsKey(49) ? walletIdValueDict[49] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@FestivalToken", walletIdValueDict.ContainsKey(50) ? walletIdValueDict[50] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@CacheKey", walletIdValueDict.ContainsKey(51) ? walletIdValueDict[51] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@RedProphetShard", walletIdValueDict.ContainsKey(52) ? walletIdValueDict[52] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@GreenProphetShard", walletIdValueDict.ContainsKey(53) ? walletIdValueDict[53] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@BlueProphetCrystal", walletIdValueDict.ContainsKey(54) ? walletIdValueDict[54] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@GreenProphetCrystal", walletIdValueDict.ContainsKey(55) ? walletIdValueDict[55] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@RedProphetCrystal", walletIdValueDict.ContainsKey(56) ? walletIdValueDict[56] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@BlueProphetShard", walletIdValueDict.ContainsKey(57) ? walletIdValueDict[57] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@WarSupplies", walletIdValueDict.ContainsKey(58) ? walletIdValueDict[58] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@UnstableFractalEssence", walletIdValueDict.ContainsKey(59) ? walletIdValueDict[59] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@TyrianDefenseSeal", walletIdValueDict.ContainsKey(60) ? walletIdValueDict[60] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@ResearchNote", walletIdValueDict.ContainsKey(61) ? walletIdValueDict[61] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@UnusualCoin", walletIdValueDict.ContainsKey(62) ? walletIdValueDict[62] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@JadeSliver", walletIdValueDict.ContainsKey(64) ? walletIdValueDict[64] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@TestimonyOfJadeHeroics", walletIdValueDict.ContainsKey(65) ? walletIdValueDict[65] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@CanachCoins", walletIdValueDict.ContainsKey(67) ? walletIdValueDict[67] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@ImperialFavor", walletIdValueDict.ContainsKey(68) ? walletIdValueDict[68] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@TalesOfDungeonDelving", walletIdValueDict.ContainsKey(69) ? walletIdValueDict[69] : DBNull.Value);
                saveCmd.Parameters.AddWithValue("@LegendaryInsight", walletIdValueDict.ContainsKey(70) ? walletIdValueDict[70] : DBNull.Value);

                connection.Open();

                saveCmd.ExecuteNonQuery();

                connection.Close();
            }
        }
    }
}
