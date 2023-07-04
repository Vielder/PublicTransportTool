using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CmpSearchData
{
    public class CSearchData : ISearchData
    {
        public NpgsqlDataReader searchInDb(string strName, string strRouteID, string strDir, string time, string find, NpgsqlConnection conn)
        {
            // choosing right query
            string query = string.Empty;
            NpgsqlDataReader reader;
            switch (find)
            {
                case "filterStops":
                    query = filterStopsQuery(strName);
                    break;
                case "filterRoutes":
                    query = filterRoutesQuery(strName);
                    break;
                case "relatedRoutes":
                    query = relatedRoutesQuery(strName);
                    break;
                case "relatedStops":
                    query = relatedStopsQuery(strRouteID, strDir);
                    break;
                case "closestTimes":
                    query = closestTimesQuery(strName, strRouteID, strDir, time);
                    break;
                case "allRoutesTimes":
                    query = allRoutesTimesQuery(strName);
                    break;
                case "allStopsTimes":
                    query = allStopsTimesQuery(strRouteID, strDir);
                    break;
            }

            // establishing connection and command
            using (NpgsqlCommand comm = new NpgsqlCommand())
            {
                comm.Connection = conn;
                comm.CommandType = CommandType.Text;
                comm.CommandText = query;
                try
                {
                    reader = comm.ExecuteReader();
                }
                catch (Exception)
                {
                    MessageBox.Show("Connection error. Do something and try again. Program may terminate in several seconds");
                    reader = comm.ExecuteReader();
                    // Stop
                }
            }

            return reader;
        }

        private string filterStopsQuery(string strStop)
        {
            // find all stops containing selected text
            return @"SELECT stop_name, stop_id, stop_lat, stop_lon
             FROM (SELECT DISTINCT stop_name, stop_id, stop_lat, stop_lon
                   FROM stops
                   WHERE authority = 'Tallinna TA'
                   AND (LOWER(stop_name) LIKE '%" + strStop + @"%' 
                   OR UPPER(stop_name) LIKE '%" + strStop + @"%'
                   OR stop_name LIKE '%" + strStop + @"%'))
             AS stops
             ORDER BY stop_name ASC";
        }

        private string filterRoutesQuery(string strRouteName)
        {
            // find all routes containing selected text
            return @"SELECT DISTINCT routes.route_short_name, trips.trip_headsign, routes.route_id, routes.route_type
             FROM routes
             INNER JOIN trips ON routes.route_id = trips.route_id
             WHERE routes.competent_authority = 'Tallinna TA'
             AND route_short_name LIKE '%" + strRouteName + @"%'
             AND routes.route_type IN (0,3,800)
             ORDER BY routes.route_short_name ASC, routes.route_type";
        }

        private string relatedRoutesQuery(string strStop)
        {
            // find routes related with selected stop
            return @"SELECT DISTINCT routes.route_short_name, trips.trip_headsign, routes.route_id, routes.route_type
             FROM routes
             INNER JOIN trips ON routes.route_id = trips.route_id
             INNER JOIN stoptimes ON trips.trip_id = stoptimes.trip_id
             INNER JOIN stops ON stoptimes.stop_id = stops.stop_id
             WHERE stops.stop_name = '" + strStop + @"'
             AND routes.competent_authority = 'Tallinna TA'
             ORDER BY routes.route_short_name ASC";
        }

        private string relatedStopsQuery(string strRouteID, string strDir)
        {
            // find all stops related to selected route in order
            return @"SELECT DISTINCT stops.stop_name, stops.stop_id, stop_lat, stop_lon, stoptimes.stop_sequence
             FROM stops
             INNER JOIN stoptimes ON stops.stop_id = stoptimes.stop_id
             INNER JOIN trips ON stoptimes.trip_id = trips.trip_id
             INNER JOIN routes ON trips.route_id = routes.route_id
             WHERE routes.route_id =  '" + strRouteID + @"'
             AND trips.trip_headsign = '" + strDir + @"'
             ORDER BY stoptimes.stop_sequence ASC";
        }

        private string closestTimesQuery(string strStop, string strRouteID, string strDir, string time)
        {
            // find 3 closest arrival times of selected route on selected stop
            return @"SELECT DISTINCT stoptimes.arrival_time
             FROM stoptimes
             INNER JOIN trips ON trips.trip_id = stoptimes.trip_id
             INNER JOIN routes ON routes.route_id = trips.route_id
             INNER JOIN stops ON stoptimes.stop_id = stops.stop_id
             INNER JOIN calendar ON trips.service_id = calendar.service_id
             WHERE stops.stop_name = '" + strStop + @"'
             AND stoptimes.arrival_time >=  '" + time + @"'
             AND routes.route_id =  '" + strRouteID + @"'
             AND trips.trip_headsign = '" + strDir + @"'
             AND (
                 CASE EXTRACT(DOW FROM current_date)
                 WHEN 0 THEN calendar.sunday
                 WHEN 1 THEN calendar.monday
                 WHEN 2 THEN calendar.tuesday
                 WHEN 3 THEN calendar.wednesday
                 WHEN 4 THEN calendar.thursday
                 WHEN 5 THEN calendar.friday
                 WHEN 6 THEN calendar.saturday
                 END
             ) = true
             ORDER BY stoptimes.arrival_time ASC LIMIT 3";
        }

        private string allRoutesTimesQuery(string strStop)
        {
            // find all times of all routes on selected stop
            return @"SELECT DISTINCT stoptimes.arrival_time, routes.route_short_name, trips.trip_headsign, routes.route_type
             FROM stoptimes
             INNER JOIN trips ON trips.trip_id = stoptimes.trip_id
             INNER JOIN routes ON routes.route_id = trips.route_id
             INNER JOIN stops ON stoptimes.stop_id = stops.stop_id
             INNER JOIN calendar ON trips.service_id = calendar.service_id
             WHERE stops.authority = 'Tallinna TA'
             AND stops.stop_name = '" + strStop + @"'
             AND (
                 CASE EXTRACT (DOW FROM current_date)
                 WHEN 0 THEN calendar.sunday
                 WHEN 1 THEN calendar.monday
                 WHEN 2 THEN calendar.tuesday
                 WHEN 3 THEN calendar.wednesday
                 WHEN 4 THEN calendar.thursday
                 WHEN 5 THEN calendar.friday
                 WHEN 6 THEN calendar.saturday
                 END
             ) = true
             ORDER BY stoptimes.arrival_time";
        }

        private string allStopsTimesQuery(string strRouteID, string strDir)
        {
            // find all times of selected route on each related stop
            return @"SELECT DISTINCT stoptimes.arrival_time, stops.stop_name, stoptimes.stop_sequence
             FROM stoptimes
             INNER JOIN trips ON trips.trip_id = stoptimes.trip_id
             INNER JOIN routes ON routes.route_id = trips.route_id
             INNER JOIN stops ON stoptimes.stop_id = stops.stop_id
             INNER JOIN calendar ON trips.service_id = calendar.service_id
             WHERE stops.authority = 'Tallinna TA'
             AND routes.route_id =  '" + strRouteID + @"'
             AND trips.trip_headsign = '" + strDir + @"'
             AND (
                 CASE EXTRACT(DOW FROM current_date)
                 WHEN 0 THEN calendar.sunday
                 WHEN 1 THEN calendar.monday
                 WHEN 2 THEN calendar.tuesday
                 WHEN 3 THEN calendar.wednesday
                 WHEN 4 THEN calendar.thursday
                 WHEN 5 THEN calendar.friday
                 WHEN 6 THEN calendar.saturday
                 END
             ) = true
             ORDER BY stoptimes.stop_sequence, stoptimes.arrival_time";
        }

        // This method implements the ISearchData.getGPS interface and is responsible for downloading GPS data
        // from a specified URL and saving it to a local file path.
        public void getGPS(string prjRoot)
        {
            // Set the URL for the GPS data.
            string url = "https://transport.tallinn.ee/gps.txt";
            // Set the local file path for saving the GPS data.
            string localFilePath = Path.Combine(prjRoot, "gps.csv");
            // Create a new WebClient object for downloading the GPS data.
            using (WebClient client = new WebClient())
            {
                try
                {
                    // Download the GPS data from the specified URL and save it to the local file path.
                    client.DownloadFile(url, localFilePath);
                }
                catch (Exception)
                {
                    // If an exception is thrown during the download, print an error message to the console.
                    Console.WriteLine("Error on gps download!");
                }
            }
        }
    }
}
