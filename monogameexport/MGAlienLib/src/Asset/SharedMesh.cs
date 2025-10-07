using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGAlienLib
{
    public class SharedMesh : SharedAsset<Mesh>
    {
        private SharedMesh(string address, object parameters) : base(address, parameters)
        {
        }

        public SharedMesh(string address, Mesh externalAsset) : base(address, null)
        {
            internal_SetExternalAsset(externalAsset);
        }

        public static Reference Get(string address)
        {
            var newMat = manager.Get(address, null, (a, p) => new SharedMesh(a, p));

            return newMat;
        }



        protected override Mesh CreateAsset(string address, object parameters)
        {
            return assetManager.GetMesh(address);
        }

    }
}
