using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotationTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    Vector3 cameraRot = new Vector3();
    // Update is called once per frame
    void Update()
    {
        this.transform.eulerAngles += new Vector3(0, Input.GetAxis("Mouse X"),0);
        this.transform.localEulerAngles += new Vector3(-Input.GetAxis("Mouse Y"), 0,0);
    }
}
