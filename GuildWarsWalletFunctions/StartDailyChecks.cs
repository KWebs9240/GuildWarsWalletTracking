using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Xml.Linq;
using Azure.Messaging.ServiceBus;
using GuildWarsWalletFunctions.TrackerModels;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GuildWarsWalletFunctions
{
    public class StartDailyChecks
    {
        [FunctionName("StartDailyChecks")]
        public async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            string dbConnectString = System.Environment.GetEnvironmentVariable("DbConnectString");
            ServiceBusClient client = new ServiceBusClient(System.Environment.GetEnvironmentVariable("SbConnectString"));
            ServiceBusSender sender = client.CreateSender("devdailyqueue");

            using (SqlConnection connection = new SqlConnection(dbConnectString))
            {
                SqlCommand cmd = new SqlCommand("SELECT NickName, ApiKey FROM guild.TrackedPeople", connection);

                using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

                connection.Open();

                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        TrackedPerson newPerson = new TrackedPerson();
                        newPerson.NickName = reader["NickName"].ToString();
                        newPerson.ApiKey = reader["ApiKey"].ToString();


                        messageBatch.TryAddMessage(new ServiceBusMessage(JsonConvert.SerializeObject(newPerson)));
                    }
                }

                await sender.SendMessagesAsync(messageBatch);
            }
        }
    }
}
