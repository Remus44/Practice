
using System.Net;
using System.Text.Json;
using BatchLab.Console.Clients;
using BatchLab.Console.Data;
using BatchLab.Console.DTOs;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables()
    .Build();

string apiBaseUrl = config["Api:BaseUrl"]
    ?? throw new InvalidOperationException(
        "Missing Api:BaseUrl configuration.");

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl),
    Timeout = TimeSpan.FromSeconds(15)
};

IProductsApiClient productsClient =
    new ProductsApiClient(httpClient);

bool running = true;

while (running)
{
    Console.Clear();
    Console.WriteLine("=== BatchLab Console ===");
    Console.WriteLine();
    Console.WriteLine("1. Termék lekérdezése");
    Console.WriteLine("2. Készlet módosítása");
    Console.WriteLine("0. Kilépés");
    Console.WriteLine();
    Console.Write("Választás: ");

    string? choice = Console.ReadLine()?.Trim();

    try
    {
        switch (choice)
        {
            case "1":
                await ShowProductAsync(productsClient);
                break;

            case "2":
                await AdjustStockAsync(productsClient);
                break;

            case "0":
                running = false;
                break;

            default:
                Console.WriteLine("Ismeretlen menüpont.");
                break;
        }
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"HTTP hiba: {ex.Message}");
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("A kérés megszakadt vagy időtúllépés történt.");
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"JSON feldolgozási hiba: {ex.Message}");
    }

    if (running)
    {
        Console.WriteLine();
        Console.Write("ENTER a folytatáshoz...");
        Console.ReadLine();
    }
}

static async Task ShowProductAsync(IProductsApiClient client)
{
    long? id = ReadProductId();
    if (id is null) return;

    ProductResponse? product =
        await client.GetByIdAsync(id.Value);

    if (product is null)
    {
        Console.WriteLine("A termék nem található.");
        return;
    }

    PrintProduct(product);
}

static async Task AdjustStockAsync(IProductsApiClient client)
{
    long? id = ReadProductId();
    if (id is null) return;

    Console.Write("Készletváltozás (+/-): ");

    if (!int.TryParse(Console.ReadLine(), out int delta)
        || delta == 0)
    {
        Console.WriteLine("Érvénytelen mennyiség.");
        return;
    }

    ApiCallResult result =
        await client.AdjustStockAsync(id.Value, delta);

    if (!result.IsSuccess)
    {
        Console.WriteLine(result.StatusCode switch
        {
            HttpStatusCode.BadRequest => "Hibás kérés (400).",
            HttpStatusCode.NotFound => "Nem található (404).",
            HttpStatusCode.Conflict => "Ütközés (409).",
            _ => $"API hiba: {(int)result.StatusCode}"
        });

        return;
    }

    Console.WriteLine("Sikeres készletmódosítás!");

    ProductResponse? updated =
        await client.GetByIdAsync(id.Value);

    if (updated is not null)
        PrintProduct(updated);
}

static long? ReadProductId()
{
    Console.Write("Termék ID: ");

    if (long.TryParse(Console.ReadLine(), out long id)
        && id > 0)
        return id;

    Console.WriteLine("Érvénytelen termékazonosító.");
    return null;
}

static void PrintProduct(ProductResponse product)
{
    Console.WriteLine();
    Console.WriteLine($"ID: {product.Id}");
    Console.WriteLine($"SKU: {product.Sku}");
    Console.WriteLine($"Név: {product.Name}");
    Console.WriteLine($"Ár: {product.Price:N2}");
    Console.WriteLine($"Készlet: {product.StockQuantity}");
}
