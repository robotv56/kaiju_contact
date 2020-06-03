using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRotation3D : MonoBehaviour
{
    public float speed = 30;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.eulerAngles += Vector3.up * Time.deltaTime * speed;
    }
}
