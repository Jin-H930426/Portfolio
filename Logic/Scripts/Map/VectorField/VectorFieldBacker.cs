using System;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace JH.Portfolio.Map
{
    public class VectorFieldBacker : MonoBehaviour
    {
        [Header("Scanning Properties")] public LayerMask scanningMask;
        public SerializedDictionary<Tile, int> costs
            = new SerializedDictionary<Tile, int>(Enum.GetNames(typeof(Tile)).Select(
                x => (Tile)Enum.Parse(typeof(Tile), x)).ToArray());

        public int2 gridSize = new int2(10, 10);
        public float cellRadius = .5f;
        public VectorField VectorField;

        private void BakeMap()
        {
            VectorField = new VectorField(cellRadius, gridSize, transform.position + transform.up);
        }
        
        public void CreateIntegrationField(float3 position)
        {
            BakeMap();
            VectorField.CreateCostField(scanningMask, costs);
            VectorField.CreateIntegrationField(VectorField.GetCellFromWorldPos(position));
        }

        #region Editor
#if UNITY_EDITOR 
        private void OnValidate()
        {
            BakeMap();
        }
        private void OnDrawGizmosSelected()
        {
            if (VectorField == null) return;
            
            Gizmos.color = Color.black;
            Vector3 size = new float3(cellRadius, 0, cellRadius) * 2;
            for (int x = 0; x < VectorField.gridSize.x; x++)
            {
                for (int y = 0; y < VectorField.gridSize.y; y++)
                {
                    Gizmos.DrawWireCube(VectorField.grid[x, y].worldPosition, size);
                }
            }
            Gizmos.color = Color.white;
            
            if (!Application.isPlaying) DisplayCostField();
            else DisplayIngrationField();
        }
        private void DisplayCostField()
        {
            Handles.color = Color.gray;
            foreach (var cell in VectorField.grid)
            {
                Handles.Label(cell.worldPosition, cell.cost.ToString());
            }
            Handles.color = Color.white;
        }
        private void DisplayIngrationField()
        {
            Handles.color = Color.gray;
            foreach (var cell in VectorField.grid)
            {
                Handles.Label(cell.worldPosition, cell.bestCost.ToString());
            }
            Handles.color = Color.white;
        } 
#endif
        #endregion
        
    }
}