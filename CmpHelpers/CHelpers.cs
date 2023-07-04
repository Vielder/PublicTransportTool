using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmpHelpers
{
    public class CHelpers : IHelpers
    {
        
        // This function takes two strings representing times and returns a string representing the result of subtracting the second time from the first time.
        public string timeHandler(string time, string timeToSubtract)
        {
            // Split the time string into hours, minutes, and seconds
            string[] times = time.Split(':');
            // Declare an array to keep track of whether to subtract one from the hours or minutes or not
            int[] k = { 0, 0 };
            // If there is no time to subtract, simply return the original time
            if (timeToSubtract == null)
            {
                // Check if the hours are greater than 24 and adjust accordingly
                if (int.Parse(times[0]) > 24)
                {
                    time = (int.Parse(times[0]) - 24).ToString() + ":" + times[1] + ":" + times[2];
                }
                else
                {
                    time = times[0] + ":" + times[1] + ":" + times[2];
                }
                return time;
            }
            else
            {
                // Split the time to subtract string into hours, minutes, and seconds
                string[] timeSubst = timeToSubtract.Split(':');
                // Check if we need to subtract one from the minutes or not
                if (int.Parse(times[2]) < int.Parse(timeSubst[2]))
                {
                    k[1] = 1;
                }
                // Check if we need to subtract one from the hours or not
                if (int.Parse(times[1]) - k[1] < int.Parse(timeSubst[1]))
                {
                    k[0] = 1;
                }

                // Check if we need to add 24 to the hours, then subtract the timeToSubtract from the original time and return the result
                if (int.Parse(times[0]) < int.Parse(timeSubst[0]))
                {
                    time = (int.Parse(times[0]) + 24 - int.Parse(timeSubst[0]) - k[0]).ToString() + ":" +
                            (int.Parse(times[1]) - int.Parse(timeSubst[1]) - k[1]).ToString() + ":" +
                            (int.Parse(times[2]) - int.Parse(timeSubst[2])).ToString();
                    return time;
                }
                // Check if we need to add 60 to the minutes and seconds, then subtract the timeToSubtract from the original time and return the result
                else if (k[0] == 1 && k[1] == 1)
                {
                    time = (int.Parse(times[0]) - int.Parse(timeSubst[0]) - k[0]).ToString() + ":" +
                            (int.Parse(times[1]) - int.Parse(timeSubst[1]) - k[1] + 60).ToString() + ":" +
                            (int.Parse(times[2]) - int.Parse(timeSubst[2]) + 60).ToString();
                }
                // Check if we need to add 60 to the seconds, then subtract the timeToSubtract from the original time and return the result
                else if (k[1] == 1)
                {
                    time = (int.Parse(times[0]) - int.Parse(timeSubst[0]) - k[0]).ToString() + ":" +
                            (int.Parse(times[1]) - int.Parse(timeSubst[1]) - k[1]).ToString() + ":" +
                            (int.Parse(times[2]) - int.Parse(timeSubst[2]) + 60).ToString();
                }
                // Subtract the timeToSubtract from the original time and return the result
                else
                {
                    time = (int.Parse(times[0]) - int.Parse(timeSubst[0])).ToString() + ":" +
                            (int.Parse(times[1]) - int.Parse(timeSubst[1])).ToString() + ":" +
                            (int.Parse(times[2]) - int.Parse(timeSubst[2])).ToString();
                }
                return time;
            }
        }

        // This function takes a string of coordinates as input and returns a single precision floating point value.
        public float parseCoordinates(string coordinates)
        {
            string result = ""; // Declare a variable to store the resulting string
            for (int i = 0; i < 2; i++) // Loop over the first two characters of the input string
            {
                result += coordinates[i]; // Append each character to the result string
            }
            result += ","; // Add a comma after the first two characters
            for (int i = 2; i < coordinates.Length; i++) // Loop over the remaining characters in the input string
            {
                result += coordinates[i]; // Append each character to the result string
            }
            return float.Parse(result); // Convert the result string to a single precision floating point value and return it
        }

    }
}
