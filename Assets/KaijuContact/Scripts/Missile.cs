using UnityEngine;

public class Missile : MonoBehaviour
{
    public enum MissileState
    {
        IDLE,//in tube
        LAUNCHED,//just launched, not tracking
        HOMING//tracking laser
    }

    //missile state
    public MissileState state { get; private set; } = MissileState.IDLE;//thanks visual studio :D

    private Rigidbody body;//storage for local rigidbody
    private Vector3 trackPoint;//point in space to track

    // Start is called before the first frame update
    void Start()
    {
        body = this.GetComponent<Rigidbody>();
    }

    float seconds = 0;//time, in seconds. similar to seconds in MissileLauncher
    Quaternion look;//look rotation

    //used for direction calculation
    Vector3 direction;
    Vector3 heading;

    // Update is called once per frame
    void Update()
    {
        //can i do this with functions? yes.
        //will I? fuck no.
        //why not? because fuck formatting (also it helps me keep track of whats happening)
        //I am chaos
        switch (state)
        {
            case MissileState.IDLE://do nothing
                break;
            case MissileState.LAUNCHED://fly up
                {
                    body.position += Vector3.up * Time.deltaTime * MissileLauncher.missileSpeed;
                }
                break;
            case MissileState.HOMING://track laser point and turn towards it
                {
                    direction = trackPoint - this.transform.position;
                    heading = direction / direction.magnitude;//normalize
                    look = Quaternion.LookRotation(heading);
                    body.rotation = Quaternion.RotateTowards(body.rotation, look, MissileLauncher.missileTrackStrength *Time.deltaTime);
                    body.position += transform.forward * Time.deltaTime * MissileLauncher.missileSpeed;
                }
                break;
        }

        if(state != MissileState.IDLE)//flight time
        {
            seconds += Time.deltaTime;
            if( seconds >= 5)//repool after 5 seconds
            {
                seconds = 0;
                Stow();
            }
            else if(state != MissileState.HOMING && seconds >= 1)//start homing after one second
            {
                state = MissileState.HOMING;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
<<<<<<< HEAD
        if ((state == MissileState.LAUNCHED || state == MissileState.HOMING) && (collision.gameObject.layer == 9 || collision.gameObject.layer == 11))
        {
            bool hitLocal = false;
            if (collision.transform.root.GetComponent<ClientMaster>())
            {
                hitLocal = collision.transform.root.GetComponent<ClientMaster>().GetIsLocalPlayer();
            }
            if (collision.transform.gameObject.layer == 9 && hitLocal && collision.transform.gameObject.name != "Slash")
            {
                if (collision.transform.gameObject.name == "Kaiju Weakspot")
                {
                    collision.transform.root.GetComponent<ClientMaster>().CmdDamageKaiju(4f);
                }
                else
                {
                    collision.transform.root.GetComponent<ClientMaster>().CmdDamageKaiju(1f);
                }
            }
            if (collision.gameObject.layer == 11 && fromLocal)
            {
                collision.gameObject.GetComponent<Iceberg>().LocalDamage(10000f, from);
            }
            Explode();
        }
=======
        //explode
        Stow();
>>>>>>> parent of edb97bc... Merge branch 'master' of https://github.com/robotv56/kaiju_contact
    }

    public void Launch()//launches the missile, starting the flight sequence
    {
        body.isKinematic = false;
        state = MissileState.LAUNCHED;
    }

    public void UpdateTrackingData(Vector3 trackPoint)//called by MissileLauncher.cs to give missile a target. gets called a lot
    {
        this.trackPoint = trackPoint;
    }

    private void Stow()//puts missile back in object pool
    {
        state = MissileState.IDLE;
        body.isKinematic = true;
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.Euler(-90,0,0);
        seconds = 0;
    }
}
