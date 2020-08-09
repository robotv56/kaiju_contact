using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    //used for missiles, tells them how fast to move and how hard to turn
    public static float missileSpeed = 10;
    public static float missileTrackStrength = 100;

    public Camera aimCamera;//the camera to use for aiming
    public Transform laserPoint;//the transform used for tracking the aimpoint. could also just use a vector3 if needed
    public LayerMask rayCastMask;//used to avoid hitting ships or other missiles with the raycast
    public Missile[] missiles;//references to teh object pool

    RaycastHit hit;//storage for any raycast hits
    float seconds = 0;//used for time based proccess
    int launchTube = 0;//index for objext pool
    bool launching = false;//are we launching?
    bool cooldown = false;//are we cooling down?

    // Update is called once per frame
    void Update()
    {
        if(Physics.Raycast(aimCamera.transform.position, aimCamera.transform.forward, out hit, 1000,rayCastMask.value))
        {
            // if you end up using an empty or a vec3, you can remove this bit
            if(!laserPoint.gameObject.activeSelf)
            {
                laserPoint.gameObject.SetActive(true);
            }

            //store impact point of cast
            laserPoint.position = hit.point;
        }
        else
        {
            //more gameobject bs
            if (laserPoint.gameObject.activeSelf)
            {
                laserPoint.gameObject.SetActive(false);
            }
        }

        //trigger launch
        if(Input.GetKeyDown(KeyCode.Space) && !cooldown && !launching)
        {
            launching = true;
        }

        if(launching)//main launcher logic
        {
            if(seconds > 0.125f * launchTube)//fire each missile an eighth of a second apart
            {
                missiles[launchTube].Launch();
                launchTube++;
            }

            if(launchTube == 8)//stop if all tubes fired
            {
                launching = false;
                cooldown = true;
                seconds = 0;
                launchTube = 0;
            }

            seconds += Time.deltaTime;//increment time
        }

        if(!launching && cooldown)//cooldown
        {
            seconds += Time.deltaTime;//increment time for cooldown

            //cooldown is 10 seconds
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
