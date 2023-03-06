using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

var localClientApi = "http://localhost:7080";
var client = new HttpClient() { BaseAddress = new Uri(localClientApi) };
var tasks = new Task[10];
var ids = new ConcurrentBag<string>();

for (int i = 0; i < 10; i++)
{
    var orderNumber = i;
    tasks[i] = Task.Run(async () =>
    {
        for (int j = 0; j < 10; j++)
        {
            var initResponse = await client.PostAsJsonAsync("api/checkout", new
            {
                OrderId = $"{orderNumber}-{j}"
            });

            if (initResponse.StatusCode != HttpStatusCode.Created)
            {
                Console.WriteLine("Error: Order not created");
                return;
            }
            
            var checkoutId = JsonSerializer.Deserialize<InitResponse>(await initResponse.Content.ReadAsStringAsync())?.id;
            if (checkoutId != null)
            {
                ids.Add(checkoutId);
            }

            var location = initResponse.Headers.Location;
            for (int k = 0; k < 12; k++)
            {
                for (int l = 0; l < 10; l++)
                {
                    var response = await client.PostAsJsonAsync($"{location}/item", new
                    {
                        Name = $"{k}-{l}"
                    });
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Error adding item {k}-{l} to order {orderNumber}-{j}, status code {response.StatusCode}");
                        Process.GetCurrentProcess().Kill();
                    }
                }
            }
        }
    });
}

Task.WaitAll(tasks);

Console.WriteLine("Done sending waiting for enter to run verify");
Console.ReadLine();

 var verifyClient = new HttpClient() { BaseAddress = new Uri(localClientApi) };

 foreach (var checkoutId in ids)
 {
     Console.WriteLine($"Verify id {checkoutId}");
     var response = await verifyClient.GetAsync($"api/checkout/{checkoutId}");
     if (!response.IsSuccessStatusCode)
         Console.WriteLine($"No OK status code for id {checkoutId}");
         
     var countResponse = JsonSerializer.Deserialize<CountResponse>(await response.Content.ReadAsStringAsync());
     if (countResponse?.count != 120)
     {
         Console.WriteLine($"Wrong result for id {checkoutId}");
     }
}

Console.WriteLine("Done");

public record InitResponse(string id);
public record CountResponse(int count);
