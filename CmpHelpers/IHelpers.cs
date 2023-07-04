using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmpHelpers
{
    public interface IHelpers
    {
        string timeHandler(string time, string timeToSubtract);
        float parseCoordinates(string cordinates);
    }
}
