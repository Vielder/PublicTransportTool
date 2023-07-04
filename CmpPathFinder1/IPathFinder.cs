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
        List<string> findRoute(string itemsLaht, string itemsSiht, int nudTransfersAmount, bool wheelChair, string curTime, NpgsqlConnection conn);
    }
}
