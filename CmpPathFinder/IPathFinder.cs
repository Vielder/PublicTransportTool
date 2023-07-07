using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace CmpPathFinder
{
    public interface IPathFinder
    {
        Task<List<string>> FindRouteAsync(string itemsLaht, string itemsSiht, int transfersAmount, bool wheelChair, string curTime, NpgsqlConnection conn);
    }
}
