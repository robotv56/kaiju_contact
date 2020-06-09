using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    public Camera aimCamera;
    public Transform laserPoint;
    public LayerMask rayCastMask;
    public Transform[] tubes;
    public Missile[] missiles;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    RaycastHit hit;
    float seconds = 0;
    float oldSeconds = 1;
    int launchTube = 0;

    // Update is called once per frame
    void Update()
    {
        if(Physics.Raycast(aimCamera.transform.position, aimCamera.transform.forward, out hit, 1000,rayCastMask.value))
        {
            
            if(!laserPoint.gameObject.activeSelf)
            {
                laserPoint.gameObject.SetActive(true);
            }
            Debug.Log(hit.point);
            laserPoint.position = hit.point;
        }
        else
        {
            if (laserPoint.gameObject.activeSelf)
            {
                laserPoint.gameObject.SetActive(false);
            }
        }
    }
}
