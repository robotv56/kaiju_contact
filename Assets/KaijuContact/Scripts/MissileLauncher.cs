using UnityEngine;

public class MissileLauncher : MonoBehaviour
{
    //used for missiles, tells them how fast to move and how hard to turn
    public static float missileSpeed = 800;
    public static float missileTrackStrength = 200;

    [SerializeField] private GameObject aimCameraObject;//the camera to use for aiming
    private Camera aimCamera;
    private Vector3[] laserPoint = new Vector3[8];//the transform used for tracking the aimpoint. could also just use a vector3 if needed
    public LayerMask rayCastMask;//used to avoid hitting ships or other missiles with the raycast
    [SerializeField] private Missile[] missiles;//references to teh object pool

    RaycastHit hit;//storage for any raycast hits
    float seconds = 0;//used for time based proccess
    int launchTube = 0;//index for objext pool
    bool launching = false;//are we launching?
    bool cooldown = false;//are we cooling down?

    bool local = false;
    Vector3 cameraPos;
    Vector3 cameraDir;
    ClientMaster clientMaster;

    float sendUpdate = 0;

    private void Start()
    {
        clientMaster = transform.parent.parent.parent.parent.GetComponent<ClientMaster>();
        local = clientMaster.isLocalPlayer;
        aimCamera = aimCameraObject.GetComponent<Camera>();
    }
    
    void Update()
    {
        //TOW-similar Missile controls for each missile
        if (local)
        {
            cameraPos = aimCamera.transform.position;
            cameraDir = aimCamera.transform.forward;
            sendUpdate += Time.deltaTime;
            if (sendUpdate > 0.1f)
            {
                sendUpdate = 0f;
                clientMaster.CmdSyncMissile(cameraPos, cameraDir);
            }
        }
        float dist = 0;
        for (int i = 0; i < 8; i++)
        {
            dist = (missiles[i].transform.position - cameraPos).magnitude;
            Vector3 aimPoint = cameraPos + cameraDir * (dist + 50f);
            if (Physics.Linecast(cameraPos, aimPoint, out hit, rayCastMask.value))
            {
                laserPoint[i] = hit.point;
            }
            else
            {
                laserPoint[i] = aimPoint;
            }
        }

        if(launching)//main launcher logic
        {
            /*if(seconds > 0.125f * launchTube)//fire each missile an eighth of a second apart*/
            if (seconds > 0.33f * launchTube)//fire each missile a third of a second apart
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

            //cooldown is 15 seconds
            if(seconds >= 15)
            {
                cooldown = false;
                seconds = 0;
            }
        }

        //pass tracking data to missiles
        for (int i = 0; i < 8; i++)
        {
            if (missiles[i].state == Missile.MissileState.HOMING)
            {
                missiles[i].UpdateTrackingData(laserPoint[i]);
            }
        }
    }

    public bool Launch()
    {
        if (!cooldown && !launching)
        {
            launching = true;
            if (local)
            {
                clientMaster.CmdFireMissiles();
            }
            return true;
        }
        return false;
    }

    public void SetAimPoint(Vector3 cameraPos, Vector3 cameraDir)
    {
        if (!local)
        {
            this.cameraPos = cameraPos;
            this.cameraDir = cameraDir;
        }
    }
}
