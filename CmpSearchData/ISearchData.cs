using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmpSearchData
{
    public interface ISearchData
    {
        NpgsqlDataReader searchInDb(string strName, string strRouteId, string strDir, string time, string find, NpgsqlConnection conn);
        void getGPS(string prjRoot);
    }
}