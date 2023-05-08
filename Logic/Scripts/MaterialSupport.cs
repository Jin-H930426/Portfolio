using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSupport : MonoBehaviour
{
    public Material material;
    
    [ContextMenu("Set material in child SpriteRenderer")]
    void SetMaterialChildSpriteRenderer()
    {
        foreach (var componentsInChild in GetComponentsInChildren<SpriteRenderer>())
        {
            componentsInChild.material = material;
        }
    }
}
