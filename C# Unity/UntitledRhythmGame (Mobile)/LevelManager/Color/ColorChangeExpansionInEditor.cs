using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColorChangeExpansionInEditor : MonoBehaviour
{
#if UNITY_EDITOR
    void Update()
    {
        GetComponent<SpriteRenderer>().color = GetComponent<ColorChange>().colors[0];
    }

#endif  
}
