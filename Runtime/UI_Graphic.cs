using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace InventoryEngine
{
    public class UI_Graphic : Graphic
    {
        public override void SetMaterialDirty() { return; }
        public override void SetVerticesDirty() { return; }
        
        /// <summary>
        /// Probably not necessary since the chain of calls 
        /// "Rebuild()->UpdateGeometry()->DoMeshGeneration()->OnPopulateMesh()" won't happen;
        /// so here really just as a fail-safe.
        /// </summary>
        /// <param name="vh">vertex helper</param>
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            return;
        }

    }
}
