using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetOutline : MonoBehaviour
{
    public new Camera camera;
    public GameObject target;

    public Vector2 offset;

    public Vector2 sizeMultiplier = new Vector2(1,1);

    private RectTransform targetRect;

    // Start is called before the first frame update
    void Start()
    {
        targetRect = GetComponent<RectTransform>();
    }
    
    // Update is called once per frame
    void Update()
    {
        Rect visualRect = UIHelpers.RendererBoundsInScreenSpace(target.GetComponent<MeshRenderer>(), camera);

        targetRect.position = new Vector2(visualRect.xMin, visualRect.yMin) + offset;

        targetRect.sizeDelta = new Vector2(visualRect.width, visualRect.height) * sizeMultiplier;
    }
}
