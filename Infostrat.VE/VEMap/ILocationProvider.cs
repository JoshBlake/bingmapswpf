using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace InfoStrat.VE
{
    public interface ILocationProvider
    {
        Point? UpdatePosition(VEMap map);
        int GetZIndex();
    }
}
