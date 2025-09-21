using MGAlienLib;
using System.Collections.Generic;

namespace Project1
{
    public class testTOML : ComponentBase
    {
        public static readonly bool IsAddableFromInspector = true;

        [SerializeField] protected List<string> data = new();
    }
}
