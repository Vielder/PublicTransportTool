using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CmpHelpers;
using System.Data;
using CmpSearchData;
using CmpPathFinder;
using System.Text.RegularExpressions;
using Npgsql.Internal.TypeHandlers.DateTimeHandlers;

public class CPathFinder : IPathFinder
{
    private async Task<List<string>> FindRoutesByTwoStopsAsync(string limitValue, string laht, string siht, string curTime, bool wheelChair, NpgsqlConnection conn)
    {
        List<string> itemsList = new List<string>();
        string time = curTime;
        curTime = DateTime.Now.ToString("HH:mm:ss");


        string query = $@"SELECT DISTINCT r.route_short_name, t.trip_long_name, t.trip_headsign, 
                            st1.arrival_time, st2.arrival_time
                FROM routes r 
                JOIN trips t ON r.route_id = t.route_id 
                JOIN stoptimes st1 ON t.trip_id = st1.trip_id 
                JOIN stoptimes st2 ON t.trip_id = st2.trip_id 
                JOIN stops s1 ON st1.stop_id = s1.stop_id 
                JOIN stops s2 ON st2.stop_id = s2.stop_id 
                JOIN calendar c ON t.service_id = c.service_id
                WHERE s1.stop_name = '{laht}'
                    AND s2.stop_name = '{siht}'
                    AND st1.arrival_time >= '{time}'
                    AND st1.stop_sequence < st2.stop_sequence 
                    AND 
                    (    
                        CASE EXTRACT(DOW FROM current_date)
                        WHEN 0 THEN c.sunday
                        WHEN 1 THEN c.monday
                        WHEN 2 THEN c.tuesday
                        WHEN 3 THEN c.wednesday
                        WHEN 4 THEN c.thursday
                        WHEN 5 THEN c.friday
                        WHEN 6 THEN c.saturday
                        END
                    ) = true";

        if (wheelChair)
        {
            query += " AND t.wheelchair_accessible = true";
        }

        query += $@" ORDER BY st2.arrival_time ASC LIMIT {limitValue}";

        IHelpers helpers = new CHelpers();

        using (NpgsqlCommand comm = new NpgsqlCommand(query, conn))
        {
            using (NpgsqlDataReader reader = await comm.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    TimeSpan timeA = TimeSpan.Parse(reader.GetString(3));
                    TimeSpan timeB = TimeSpan.Parse(reader.GetString(4));
                    TimeSpan timeResult = timeB - timeA;
                    string strRoute = reader.GetString(0) + " - " + reader.GetString(1);
                    string strText = strRoute + ": " + timeA.ToString("hh\\:mm") + " - " + timeB.ToString("hh\\:mm") + " (" + timeResult.TotalMinutes.ToString("0") + " min)" + "  " + helpers.timeHandler(reader.GetString(3), curTime);
                    itemsList.Add(strText);
                    time = timeB.ToString();
                }
            }
        }

        return itemsList;
    }

    private async Task FindUniqueRoutesAsync(string laht, string siht, List<string> uniqueRoutesLaht, List<string> uniqueRoutesSiht, string time, bool wheelChair, NpgsqlConnection conn)
    {
        string query = $@"SELECT r.route_short_name, 
                          CASE WHEN s.stop_name = '{laht}'THEN 1 
                               WHEN s.stop_name = '{siht}' THEN 2 
                          END AS stop_order, r.route_type
                  FROM routes r
                  JOIN trips t ON r.route_id = t.route_id
                  JOIN stoptimes st ON t.trip_id = st.trip_id
                  JOIN stops s ON st.stop_id = s.stop_id
                  JOIN calendar c ON t.service_id = c.service_id
                  WHERE s.stop_name IN ('{laht}', '{siht}')
                      AND st.arrival_time >= '{time}'
                      AND 
                      (    
                          CASE EXTRACT(DOW FROM current_date)
                          WHEN 0 THEN c.sunday
                          WHEN 1 THEN c.monday
                          WHEN 2 THEN c.tuesday
                          WHEN 3 THEN c.wednesday
                          WHEN 4 THEN c.thursday
                          WHEN 5 THEN c.friday
                          WHEN 6 THEN c.saturday
                          END
                      ) = true";

        if (wheelChair)
        {
            query += " AND t.wheelchair_accessible = true";
        }

        query += @" GROUP BY r.route_short_name, r.route_type, s.stop_name
                    ORDER BY stop_order, route_type DESC, route_short_name ASC";

        using (NpgsqlCommand comm = new NpgsqlCommand(query, conn))
        {
            using (NpgsqlDataReader reader = await comm.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int tempReader = reader.GetInt32(1);
                    if (tempReader == 1)
                    {
                        uniqueRoutesLaht.Add(reader.GetString(0) + " " + reader.GetInt32(2));
                    }
                    else
                    {
                        uniqueRoutesSiht.Add(reader.GetString(0) + " " + reader.GetInt32(2));
                    }
                }
            }
        }
    }

    private async Task<string> FindIntersectionAsync(string routeA, string routeB, int rTypeA, int rTypeB, NpgsqlConnection conn)
    {
        string intersectionStop = string.Empty;
        bool compareRouteTypes = false;

        if (rTypeA != 3 || rTypeA != 3)
        {
            compareRouteTypes = true;
        }

        string query = $@"SELECT s.stop_name
                            FROM stops s
                            INNER JOIN stoptimes st ON s.stop_id = st.stop_id
                            INNER JOIN trips t ON st.trip_id = t.trip_id
                            INNER JOIN routes r ON t.route_id = r.route_id
                            WHERE r.route_short_name IN ('{routeA}', '{routeB}')";

        if (compareRouteTypes)
        {
            query += $" AND r.route_type IN ('{rTypeA}', '{rTypeB}')";
        }

        query += @" GROUP BY s.stop_id
                    HAVING COUNT(DISTINCT r.route_id) = 2
                    LIMIT 1";

        using (NpgsqlCommand comm = new NpgsqlCommand(query, conn))
        {
            using (NpgsqlDataReader reader = await comm.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while (await reader.ReadAsync())
                    {
                        intersectionStop = reader.GetString(0);
                    }
                }
            }
        }

        return intersectionStop;
    }

    private async Task<List<string>> FindAllIntersectionsAsync(string laht, string siht, List<string> uniqueRoutesLaht, List<string> uniqueRoutesSiht, NpgsqlConnection conn)
    {
        List<string> transferStops = new List<string>();

        foreach (var routeLaht in uniqueRoutesLaht)
        {
            string[] routePartsLaht = routeLaht.Split(' ');
            string tempLahtRouteName = routePartsLaht[0];
            int tempLahtRouteType = int.Parse(routePartsLaht[1]);

            foreach (var routeSiht in uniqueRoutesSiht)
            {
                string[] routePartsSiht = routeSiht.Split(' ');
                string tempSihtRouteName = routePartsSiht[0];
                int tempSihtRouteType = int.Parse(routePartsSiht[1]);

                string tempStop = await FindIntersectionAsync(tempLahtRouteName, tempSihtRouteName, tempLahtRouteType, tempSihtRouteType, conn);

                if (!string.IsNullOrEmpty(tempStop) && !transferStops.Contains(tempStop))
                {
                    transferStops.Add(tempStop);
                }
            }
        }

        return transferStops;
    }

    private async Task<List<string>> FindRoutesByThreeStopsAsync(string laht, string siht, List<string> uniqueRoutesLaht, List<string> uniqueRoutesSiht, string time, bool wheelChair, NpgsqlConnection conn)
    {
        List<string> itemsList = new List<string>();

        IHelpers helpers = new CHelpers();

        // TODO: optimize search and display multiple routes with transfers
        //foreach (var transferStop in await FindAllIntersectionsAsync(laht, siht, uniqueRoutesLaht, uniqueRoutesSiht, conn))
        //{
        List<string> transferStops = await FindAllIntersectionsAsync(laht, siht, uniqueRoutesLaht, uniqueRoutesSiht, conn);

        string[] routePartsLaht = uniqueRoutesLaht[0].Split(' ');
        string tempLahtRouteName = routePartsLaht[0];
        int tempLahtRouteType = int.Parse(routePartsLaht[1]);

        string[] routePartsSiht = uniqueRoutesSiht[0].Split(' ');
        string tempSihtRouteName = routePartsSiht[0];
        int tempSihtRouteType = int.Parse(routePartsSiht[1]);

        List<string> transferRoutes = await FindRoutesByTwoStopsAsync("1", laht, transferStops[0], time, wheelChair, conn);
        Regex regex = new Regex(@"(?<=\d{2}:\d{2} - )\d{2}:\d{2}");
        Match match = regex.Match(transferRoutes[0]);
        string nextTime = match.Value + ":00";
        List<string> transferRoutes2 = await FindRoutesByTwoStopsAsync("1", transferStops[0], siht, nextTime, wheelChair, conn);

        foreach (var route in transferRoutes)
        {
            itemsList.Add(route);
        }
        
        itemsList.Add(transferStops[0]);

        foreach (var route in transferRoutes2)
        {
            itemsList.Add(route);
        }
        
        regex = new Regex(@"(?<=\d{2}:\d{2} - )\d{2}:\d{2}");
        match = regex.Match(transferRoutes2[0]);
        string endTime = match.Value + ":00";
        regex = new Regex(@"(?=\d{2}:\d{2} - )\d{2}:\d{2}");
        match = regex.Match(transferRoutes[0]);
        string startTime = match.Value + ":00";

        itemsList.Add("Time for the route: " + helpers.timeHandler(endTime, startTime));
        //}

        return itemsList;
    }

    public async Task<List<string>> FindRouteAsync(string itemsLaht, string itemsSiht, int transfersAmount, bool wheelChair, string curTime, NpgsqlConnection conn)
    {
        List<string> itemsList = new List<string>();

        string limitValue = transfersAmount.ToString();

        List<string> uniqueRoutesLaht = new List<string>();
        List<string> uniqueRoutesSiht = new List<string>();

        await FindUniqueRoutesAsync(itemsLaht, itemsSiht, uniqueRoutesLaht, uniqueRoutesSiht, curTime, wheelChair, conn);

        List<string> itemsByTwoStops = await FindRoutesByTwoStopsAsync("3", itemsLaht, itemsSiht, curTime, wheelChair, conn);

        if (itemsByTwoStops.Count == 0 && transfersAmount > 0)
        {
            List<string> itemsByThreeStops = await FindRoutesByThreeStopsAsync(itemsLaht, itemsSiht, uniqueRoutesLaht, uniqueRoutesSiht, curTime, wheelChair, conn);
            itemsList.AddRange(itemsByThreeStops);
        } 
        else
        {
            itemsList.AddRange(itemsByTwoStops);
        }


        return itemsList;
    }
}
