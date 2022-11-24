using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GuildWarsWalletFunctions.GuildWarsModels;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GuildWarsWalletFunctions
{
    public class GetNumCoins
    {
        private readonly HttpClient _client;

        public GetNumCoins(IHttpClientFactory httpClientFactory)
        {
            this._client = httpClientFactory.CreateClient();
        }

        [FunctionName("GetNumCoins")]
        public async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {System.Environment.GetEnvironmentVariable("ApiToken")}");
            string testString = await _client.GetStringAsync("https://api.guildwars2.com/v2/account/wallet");

            List<WalletValue> walletValues = JsonConvert.DeserializeObject<List<WalletValue>>(testString);

            string connectString = System.Environment.GetEnvironmentVariable("DbConnectString");

            using(SqlConnection connection = new SqlConnection(connectString))
            {
                SqlCommand saveCmd = new SqlCommand("INSERT INTO guild.Wallet (NickName, EntryDate, Coins) VALUES (@NickName, @EntryDate, @Coins)", connection);

                saveCmd.Parameters.AddWithValue("@NickName", "KWebs");
                saveCmd.Parameters.AddWithValue("@EntryDate", DateTime.UtcNow.Date);
                saveCmd.Parameters.AddWithValue("@Coins", walletValues.First(x => x.Id.Equals(1)).Value);

                connection.Open();

                saveCmd.ExecuteNonQuery();

                connection.Close();
            }
        }
    }
}
