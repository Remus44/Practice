
using BatchLab.Console.Clients;
using BatchLab.Console.Clients.Interfaces;
using BatchLab.Console.Clients.Results;
using BatchLab.Contracts.Jobs;
using BatchLab.Contracts.Products;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Text.Json;

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

IJobsApiClient jobsClient = new JobsApiClient(httpClient);
BulkProductImportRequest? lastImport = null;
BulkPriceUpdateRequest? lastPriceUpdate = null;
BulkProductImportRequest? lastChunkedImport = null;
BulkPriceUpdateRequest? lastChunkedPriceUpdate = null;

bool running = true;

while (running)
{
    Console.Clear();
    Console.WriteLine("=== BatchLab Console ===");
    Console.WriteLine();
    Console.WriteLine("1. Termék lekérdezése");
    Console.WriteLine("2. Készlet módosítása");
    Console.WriteLine();
    Console.WriteLine("3. Bulk Insert - termékek bevitele");
    Console.WriteLine("4. Utolsó Bulk Insert megismétlése");
    Console.WriteLine();
    Console.WriteLine("5. Hangfire job állapota");
    Console.WriteLine();
    Console.WriteLine("6. Bulk Update – termékek árának módosítása");
    Console.WriteLine("7. Utolsó Bulk Update megismétlése");
    Console.WriteLine();
    Console.WriteLine("8. Chunkolt Bulk Insert");
    Console.WriteLine("9. Utolsó chunkolt import újraküldése");
    Console.WriteLine();
    Console.WriteLine("10. Chunkolt Bulk Update");
    Console.WriteLine("11. Utolsó chunkolt Bulk Update megismétlése");
    Console.WriteLine();
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

            case "3":
                {
                    System.Console.Write("Hány terméket szeretnél? (1-100): ");

                    if (!int.TryParse(System.Console.ReadLine(), out int count)
                        || count is < 1 or > 100)
                    {
                        System.Console.WriteLine("Érvénytelen darabszám.");
                        break;
                    }

                    var products = new List<ProductImportItem>();
                    bool valid = true;

                    for (int i = 0; i < count; i++)
                    {
                        System.Console.WriteLine($"\n{i + 1}. termék");

                        System.Console.Write("SKU: ");
                        string sku = System.Console.ReadLine()?.Trim() ?? "";

                        System.Console.Write("Név: ");
                        string name = System.Console.ReadLine()?.Trim() ?? "";

                        System.Console.Write("Ár: ");
                        bool priceValid = decimal.TryParse(
                            System.Console.ReadLine(), out decimal price);

                        System.Console.Write("Készlet: ");
                        bool stockValid = int.TryParse(
                            System.Console.ReadLine(), out int stock);

                        if (string.IsNullOrWhiteSpace(sku) ||
                            sku.Length > 64 ||
                            string.IsNullOrWhiteSpace(name) ||
                            name.Length > 200 ||
                            !priceValid || price < 0 ||
                            !stockValid || stock < 0)
                        {
                            System.Console.WriteLine("Hibás termékadat!");
                            valid = false;
                            break;
                        }

                        products.Add(new ProductImportItem(
                            sku, name, price, stock));
                    }

                    if (!valid)
                        break;

                    var request = new BulkProductImportRequest(
                        Guid.NewGuid(),
                        products);

                    var result = await jobsClient.StartBulkInsertAsync(request);

                    lastImport = request;

                    System.Console.WriteLine("\nJob elindítva!");
                    System.Console.WriteLine($"Job ID: {result.JobId}");
                    System.Console.WriteLine($"Operation ID: {result.OperationId}");

                    break;
                }

            case "4":
                {
                    if (lastImport is null)
                    {
                        System.Console.WriteLine("Még nincs korábbi import.");
                        break;
                    }

                    var result = await jobsClient.StartBulkInsertAsync(lastImport);

                    System.Console.WriteLine("Azonos import újra elküldve.");
                    System.Console.WriteLine($"Job ID: {result.JobId}");
                    System.Console.WriteLine($"Operation ID: {result.OperationId}");

                    break;
                }

            case "5":
                {
                    System.Console.Write("Job ID: ");
                    string jobId = System.Console.ReadLine()?.Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(jobId))
                        break;

                    var status = await jobsClient.GetStatusAsync(jobId);

                    if (status is null)
                    {
                        System.Console.WriteLine("Job nem található.");
                    }
                    else
                    {
                        System.Console.WriteLine(
                            $"Job {status.JobId}: {status.Status}");
                    }

                    break;
                }

            case "6":
                {
                    System.Console.Write("Termék ID-k (vesszővel elválasztva): ");
                    string input = System.Console.ReadLine() ?? "";

                    var parts = input.Split(
                        ',', StringSplitOptions.RemoveEmptyEntries
                            | StringSplitOptions.TrimEntries);

                    var ids = new List<long>();
                    bool valid = parts.Length is >= 1 and <= 100;

                    foreach (var part in parts)
                    {
                        if (!long.TryParse(part, out long id) || id <= 0)
                        {
                            valid = false;
                            break;
                        }

                        ids.Add(id);
                    }

                    if (!valid)
                    {
                        System.Console.WriteLine("Érvénytelen termékazonosítók.");
                        break;
                    }

                    System.Console.Write("Árváltozás (+/-): ");

                    if (!decimal.TryParse(
                            System.Console.ReadLine(), out decimal delta)
                        || delta == 0
                        || decimal.Round(delta, 2) != delta)
                    {
                        System.Console.WriteLine("Érvénytelen árváltozás.");
                        break;
                    }

                    var request = new BulkPriceUpdateRequest(
                        Guid.NewGuid(),
                        ids.Distinct().ToList(),
                        delta);

                    var result = await jobsClient.StartBulkPriceUpdateAsync(request);

                    lastPriceUpdate = request;

                    System.Console.WriteLine($"Job ID: {result.JobId}");
                    System.Console.WriteLine($"Operation ID: {result.OperationId}");
                    break;
                }

            case "7":
                {
                    if (lastPriceUpdate is null)
                    {
                        System.Console.WriteLine("Nincs korábbi Bulk Update.");
                        break;
                    }

                    var result = await jobsClient
                        .StartBulkPriceUpdateAsync(lastPriceUpdate);

                    System.Console.WriteLine(
                        $"Ismételt job indítva: {result.JobId}");
                    break;
                }

            case "8":
                {
                    var request = ReadChunkedImportRequest();

                    if (request is null)
                        break;

                    var result = await jobsClient.StartChunkedBulkInsertAsync(
                        request,
                        simulateFailure: true);

                    lastChunkedImport = request;

                    System.Console.WriteLine("Chunkolt import elindítva!");
                    System.Console.WriteLine($"Job ID: {result.JobId}");
                    System.Console.WriteLine($"Operation ID: {result.OperationId}");
                    break;
                }

            case "9":
                {
                    if (lastChunkedImport is null)
                    {
                        System.Console.WriteLine("Nincs korábbi chunkolt import.");
                        break;
                    }

                    var result = await jobsClient.StartChunkedBulkInsertAsync(
                        lastChunkedImport,
                        simulateFailure: false);

                    System.Console.WriteLine(
                        $"Import ismét elküldve: {result.JobId}");

                    break;
                }

            case "10":
                {
                    var request = ReadChunkedPriceUpdateRequest();

                    if (request is null)
                        break;

                    var result = await jobsClient.StartChunkedPriceUpdateAsync(
                        request,
                        simulateFailure: true);

                    lastChunkedPriceUpdate = request;

                    System.Console.WriteLine("Chunkolt Bulk Update elindítva!");
                    System.Console.WriteLine($"Job ID: {result.JobId}");
                    System.Console.WriteLine($"Operation ID: {result.OperationId}");

                    break;
                }

            case "11":
                {
                    if (lastChunkedPriceUpdate is null)
                    {
                        System.Console.WriteLine(
                            "Nincs korábbi chunkolt Bulk Update.");
                        break;
                    }

                    var result = await jobsClient.StartChunkedPriceUpdateAsync(
                        lastChunkedPriceUpdate,
                        simulateFailure: false);

                    System.Console.WriteLine(
                        $"Ismételt chunkolt update: {result.JobId}");

                    break;
                }

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

static BulkProductImportRequest? ReadChunkedImportRequest()
{
    System.Console.Write("Hány termék? (1-100): ");

    if (!int.TryParse(System.Console.ReadLine(), out int count)
        || count is < 1 or > 100)
    {
        System.Console.WriteLine("Érvénytelen darabszám.");
        return null;
    }

    var products = new List<ProductImportItem>();

    for (int i = 0; i < count; i++)
    {
        System.Console.WriteLine($"\n{i + 1}. termék");

        System.Console.Write("SKU: ");
        string sku = System.Console.ReadLine()?.Trim() ?? "";

        System.Console.Write("Név: ");
        string name = System.Console.ReadLine()?.Trim() ?? "";

        System.Console.Write("Ár: ");
        bool validPrice = decimal.TryParse(
            System.Console.ReadLine(), out decimal price);

        System.Console.Write("Készlet: ");
        bool validStock = int.TryParse(
            System.Console.ReadLine(), out int stock);

        if (string.IsNullOrWhiteSpace(sku) ||
            sku.Length > 64 ||
            string.IsNullOrWhiteSpace(name) ||
            name.Length > 200 ||
            !validPrice || price < 0 ||
            !validStock || stock < 0)
        {
            System.Console.WriteLine("Hibás termékadat.");
            return null;
        }

        products.Add(new ProductImportItem(
            sku, name, price, stock));
    }

    return new BulkProductImportRequest(
        Guid.NewGuid(), products);
}

static BulkPriceUpdateRequest? ReadChunkedPriceUpdateRequest()
{
    System.Console.Write(
        "Termékazonosítók (vesszővel elválasztva, min. 5 db): ");

    string input = System.Console.ReadLine() ?? "";

    var parts = input.Split(
        ',',
        StringSplitOptions.RemoveEmptyEntries |
        StringSplitOptions.TrimEntries);

    if (parts.Length is < 5 or > 100)
    {
        System.Console.WriteLine("5-100 azonosító szükséges.");
        return null;
    }

    var ids = new List<long>();

    foreach (string part in parts)
    {
        if (!long.TryParse(part, out long id) || id <= 0)
        {
            System.Console.WriteLine("Érvénytelen termékazonosító.");
            return null;
        }

        ids.Add(id);
    }

    ids = ids.Distinct().OrderBy(id => id).ToList();

    if (ids.Count < 5)
    {
        System.Console.WriteLine(
            "Legalább öt különböző azonosító szükséges.");
        return null;
    }

    System.Console.Write("Árváltozás (+/-): ");

    if (!decimal.TryParse(
            System.Console.ReadLine(), out decimal delta)
        || delta == 0
        || decimal.Round(delta, 2) != delta)
    {
        System.Console.WriteLine("Érvénytelen árváltozás.");
        return null;
    }

    return new BulkPriceUpdateRequest(
        Guid.NewGuid(),
        ids,
        delta);
}
