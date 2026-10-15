using System;
using System.IO;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Newtonsoft.Json; 

namespace MarketStokAPI
{
    public static class HttpTrigger1
    {
        [FunctionName("HttpTrigger1")]
        public static async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req,
    ILogger log)
{
    log.LogInformation("C# HTTP trigger function çalıştırıldı.");
    string connectionString = "Server=localhost;Database=MarketStokkontrol;User Id=SA;Password=Password123;";
    
    List<Urun> urunler = new List<Urun>();

    try
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            await conn.OpenAsync();
            string query = "SELECT urun_id, ad, stok_adedi FROM dbo.Urunler";
            
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        urunler.Add(new Urun
                        {
                            urun_id = reader.GetInt32(0),
                            ad = reader.GetString(1),
                            stok_adedi = reader.GetInt32(2)
                        });
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        log.LogError($"Hata oluştu: {ex.Message}");
        return new ObjectResult($"Hata: {ex.Message}") { StatusCode = 500 }; 
    }

    return new OkObjectResult(urunler); 
}

    }

    public class Urun
    {
        public int urun_id { get; set; }
        public string ad { get; set; }
        public int stok_adedi { get; set; }
    }
}
