using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MarketStokAPI
{
    public class HttpTrigger1
    {
        private readonly ILogger<HttpTrigger1> _logger;

        public HttpTrigger1(ILogger<HttpTrigger1> logger)
        {
            _logger = logger;
        }

        [Function("HttpTrigger1")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.LogInformation("C# HTTP trigger function çalıştırıldı.");

            string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");

            // Bağlantı dizesi kontrolü
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Bağlantı dizesi bulunamadı. Lütfen Azure ortam değişkenlerini kontrol edin.");
                var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("Bağlantı dizesi bulunamadı.");
                return response;
            }

            List<Urun> urunler = new();

            try
            {
                await using (SqlConnection conn = new(connectionString))
                {
                    await conn.OpenAsync();
                    _logger.LogInformation("SQL bağlantısı başarılı.");

                    string query = "SELECT UrunID, UrunAdi, StokMiktari FROM dbo.Urunler";

                    await using (SqlCommand cmd = new(query, conn))
                    await using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            urunler.Add(new Urun
                            {
                                UrunID = reader.GetInt32(0),
                                UrunAdi = reader.GetString(1),
                                StokMiktari = reader.GetInt32(2)
                            });
                        }
                    }
                }

                _logger.LogInformation($"DB'den {urunler.Count} ürün çekildi.");

                string jsonResult = JsonConvert.SerializeObject(urunler, Formatting.Indented);

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(jsonResult);
                return response;
            }
            catch (SqlException sqlEx)
            {
                _logger.LogError($"SQL Hatası: {sqlEx.Message}");
                var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await response.WriteStringAsync($"SQL Hatası: {sqlEx.Message}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Genel Hata: {ex.Message}");
                var response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await response.WriteStringAsync($"Beklenmeyen hata: {ex.Message}");
                return response;
            }
        }
    }

    public class Urun
    {
        public int UrunID { get; set; }
        public string UrunAdi { get; set; }
        public int StokMiktari { get; set; }
    }
}
