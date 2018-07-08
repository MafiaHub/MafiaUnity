using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MafiaUnity
{

    /// <summary>
    /// This interface is used by mods as an entry point once the mods get loaded.
    /// </summary>
    public interface IModScript
    {
        void Start();
    }
}
