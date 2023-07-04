using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CmpHelpers;
using System.Data;

namespace CmpPathFinder
{
    public class CPathFinder : IPathFinder
    {
        private void findRouteByTwoStops(string limitValue, string laht, string siht, List<string> itemsList, ref string time, bool wheelChair, NpgsqlConnection conn)
        {
            //query selects route number, trip long name etc. using tables:
            //routes, trips, stoptimes, stops, calendar.
            //query searches for a route number, that moves across both given stops
            //outputs routes with arrival time after current time
            //and only routes that are available in a current day
            string query = $@"SELECT DISTINCT r.route_short_name, t.trip_long_name, t.trip_headsign, 
                            st1.arrival_time, st2.arrival_time
                FROM routes r 
                JOIN trips t ON r.route_id = t.route_id 
                JOIN stoptimes st1 ON t.trip_id = st1.trip_id 
                JOIN stoptimes st2 ON t.trip_id = st2.trip_id 
                JOIN stops s1 ON st1.stop_id = s1.stop_id 
                JOIN stops s2 ON st2.stop_id = s2.stop_id 
                JOIN calendar c ON t.service_id = c.service_id
                WHERE s1.stop_name = {laht}
                    AND s2.stop_name = {siht}  
                    AND st1.arrival_time >= {time}
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

            query += wheelChair ? " AND t.wheelchair_accessible = true" : "";
            query += " ORDER BY st2.arrival_time ASC LIMIT " + limitValue;

            IHelpers helpers = new CHelpers();
            DateTime curTime = DateTime.Now;

            using (NpgsqlCommand comm = new NpgsqlCommand()) 
            {
                comm.Connection = conn;
                comm.CommandType = System.Data.CommandType.Text;
                comm.CommandText = query;

                using (NpgsqlDataReader reader = comm.ExecuteReader())
                {
                    TimeSpan timeA = TimeSpan.Parse(reader.GetString(3));
                    TimeSpan timeB = TimeSpan.Parse(reader.GetString(4));
                    TimeSpan timeResult = timeB - timeA;
                    string strRoute = reader.GetString(0) + " - " + reader.GetString(1);
                    string strText = strRoute + ": " + timeA.ToString("hh\\:mm") + " - " + timeB.ToString("hh\\:mm") + " (" + timeResult.TotalMinutes.ToString("0") + " min)" + "  " + helpers.timeHandler(reader.GetString(3), curTime.ToString("HH:mm:ss"));
                    itemsList.Add(strText);
                    time = timeB.ToString();
                }
            }
        }

        private void findUniqueRoutes(string laht, string siht, List<string> uniqueRoutesLaht, List<string> uniqueRoutesSiht, string time, bool wheelChair, NpgsqlConnection conn)
        {
            List<string> uniqueRoutesList = new List<string>();
            /*
             * query selects route number and route type using tables:
             * routes, trips, stoptimes, stops, calendar.
             * stop_order is used to divide route numbers considering its stop
             * query searches for all route numbers, that are unique for both given stops
             * outputs routes with arrival time after current time
             * and only routes that are available in a current day
            */
            string query = $@"SELECT r.route_short_name, 
                          CASE WHEN s.stop_name = '{laht}' THEN 1 
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
            using (NpgsqlCommand comm = new NpgsqlCommand())
            {
                comm.Connection = conn;
                comm.CommandType = CommandType.Text;
                comm.CommandText = query;
                using (NpgsqlDataReader reader = comm.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // tempReader reads stop_order column. Value 1 - route number belongs to the start stop, value 2 - final stop
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
                    reader.DisposeAsync();
                }

            };
        }

        private string findIntesection(string routeA, string routeB, int rTypeA, int rTypeB, NpgsqlConnection conn)
        {
            string intersectonStop = "";
            bool compareRouteTypes = false;

            // if is not a bus
            if (rTypeA != 3 || rTypeA != 3)
            {
                compareRouteTypes = true;
            }

            /*
             * query selects stop name using tables:
             * stops, stoptimes, trips, routes.
             * "HAVING COUNT = 2" makes query output only stop name which are in both (2) given routes             
            */
            string query = $@"SELECT s.stop_name
                                FROM stops s
                                INNER JOIN stoptimes st ON s.stop_id = st.stop_id
                                INNER JOIN trips t ON st.trip_id = t.trip_id
                                INNER JOIN routes r ON t.route_id = r.route_id
                                WHERE r.route_short_name IN (' {routeA}', ' {routeB} ')"

            // if one of the routes is not a bus - compares route numbers AND route types
            if (compareRouteTypes)
            {
                query += $" AND r.route_type IN (' {rTypeA} ', ' {rTypeB} ')";
            }
            query += @" GROUP BY s.stop_id
                        HAVING COUNT(DISTINCT r.route_id) = 2
                        LIMIT 1";

            using (NpgsqlCommand comm = new NpgsqlCommand())
            {
                comm.Connection = conn;
                comm.CommandType = CommandType.Text;
                comm.CommandText = query;
                using (NpgsqlDataReader reader = comm.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            intersectonStop = reader.GetString(0);
                        }
                    }
                    else
                    {
                        intersectonStop = "";
                    }
                }
            }
            return intersectonStop;
        }

        private List<string> findAllIntesections(string laht, string siht, List<string> uniqueRoutesLaht, List<string> uniqueRoutesSiht, NpgsqlConnection conn)
        {
            string tempStop;
            List<string> transferStops = new List<string>();
            // Iterate through each unique route in the starting location
            foreach (int i in Enumerable.Range(0, uniqueRoutesLaht.Count)) {
                string[] routePartsLaht = uniqueRoutesLaht[i].Split(' ');
                string tempLahtRouteName = routePartsLaht[0];
                int tempLahtRouteType = int.Parse(routePartsLaht[1]);

                // Iterate through each unique route in the destination location
                foreach (int j in Enumerable.Range(0, uniqueRoutesLaht.Count))
                {
                    // Split the route name and type from the current unique route in the destination location
                    string[] routePartsSiht = uniqueRoutesSiht[j].Split(' ');
                    string tempSihtRouteName = routePartsSiht[0];
                    int tempSihtRouteType = int.Parse((string)routePartsSiht[1]);

                    // Find the intersection stop between the current routes in the starting and destination locations
                    tempStop = findIntesection(tempLahtRouteName, tempSihtRouteName, tempLahtRouteType, tempSihtRouteType, conn);

                    // If an intersection stop is found and it has not already been added to the list of transfer stops, add it to the list
                    if (!string.IsNullOrEmpty(tempStop) && !transferStops.Contains(tempStop))
                    {
                        transferStops.Add(tempStop);
                    }
                }
            }
        }

        private void findRouteByThreeStops(string laht, string siht, List<string> itemsList, string curTime, bool totalTime, bool wheelChair, NpgsqlConnection conn)
        {
            List<string> uniqueRoutesLaht = new List<string>();
            List<string> uniqueRoutesSiht = new List<string>();
            List<string> ABList = new List<string>();
            List<string> CBList = new List<string>();
            findUniqueRoutes(laht, siht, uniqueRoutesLaht, uniqueRoutesSiht, curTime, wheelChair, conn);
            List<string> = findAllIntesections(laht, siht, uniqueRoutesLaht, uniqueRoutesSiht, conn);

        }

        private List<string> findRoute(string itemsLaht, string itemsSiht, int nudTransfersAmount, bool wheelChair, string curTime, NpgsqlConnection conn)
        {
            throw new NotImplementedException();
            List<string> itemsList = new List<string>();
            List<string> itemsTempList = new List<string>();

            // try to find a route with no transfers
            findRouteByTwoStops("3", itemsLaht, itemsSiht, itemsTempList, ref curTime, wheelChair, conn);
            if ( itemsTempList.Count > 0 )
            {
                itemsList.AddRange(itemsTempList);
                return itemsList;
            }
            // if transfers are not allowed
            if ( nudTransfersAmount == 0 )
            {
                return itemsList;
            }
            else
            {
                findRouteByThreeStops
            }

        }
    }
}
