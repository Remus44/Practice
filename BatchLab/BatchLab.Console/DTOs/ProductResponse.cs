using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchLab.Console.DTOs
{
    // A Web API DTO-jának megfelelő lokális szerződés
    public record ProductResponse(
        long Id,
        string Sku,
        string Name,
        decimal Price,
        int StockQuantity,
        DateTime UpdatedAt);
}
