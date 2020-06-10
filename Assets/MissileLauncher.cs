using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    public static float missileSpeed = 10;
    public static float missileTrackStrength = 100;

    public Camera aimCamera;
    public Transform laserPoint;
    public LayerMask rayCastMask;
    public Transform[] tubes;//not needed?
    public Missile[] missiles;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    RaycastHit hit;
    float seconds = 0;
    int launchTube = 0;
    bool launching = false;
    bool cooldown = false;
    // Update is called once per frame
    void Update()
    {
        if(Physics.Raycast(aimCamera.transform.position, aimCamera.transform.forward, out hit, 1000,rayCastMask.value))
        {
            
            if(!laserPoint.gameObject.activeSelf)
            {
                laserPoint.gameObject.SetActive(true);
            }
            //Debug.Log(hit.point);
            laserPoint.position = hit.point;
        }
        else
        {
            if (laserPoint.gameObject.activeSelf)
            {
                laserPoint.gameObject.SetActive(false);
            }
        }

        if(Input.GetKeyDown(KeyCode.Space) && !cooldown && !launching)
        {
            launching = true;
            Debug.Log("launch initiated");
        }

        if(launching)
        {
            if(seconds > 0.125f * launchTube)
            {
                missiles[launchTube].Launch();
                
                Debug.Log("launched " + launchTube);
                launchTube++;
            }

            if(launchTube == 8)
            {
                Debug.Log("stopping launch");
                launching = false;
                cooldown = true;
                seconds = 0;
                launchTube = 0;
            }
            //Debug.Log(seconds);
            seconds += Time.deltaTime;
        }

        if(!launching && cooldown)
        {
            seconds += Time.deltaTime;
            if(seconds >= 10)
            {
                cooldown = false;
                seconds = 0;
            }
        }

        //pass tracking data to missiles
        foreach(Missile m in missiles)
        {
            if(m.state == Missile.MissileState.HOMING)
            {
                m.UpdateTrackingData(laserPoint.position);
            }
        }
    }
}
