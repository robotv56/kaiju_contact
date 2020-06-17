using System.Collections;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public enum MissileState
    {
        IDLE,//in tube
        LAUNCHED,//just launched, not tracking
        HOMING,//tracking laser
        WAITING//waiting for trail to fade
    }

    //missile state
    public MissileState state { get; private set; } = MissileState.IDLE;//thanks visual studio :D

    private Rigidbody body;//storage for local rigidbody
    private Vector3 trackPoint;//point in space to track
    private GameObject missileBody;
    private GameObject missileWings;
    private GameObject missileTrail;
    private GameObject missileExplosion;
    private float trailLength = 0.8f;
    private float launchLength = 0.9f;

    private GameObject homeTube;
    private GameObject from;
    private bool fromLocal;

    // Start is called before the first frame update
    void Start()
    {
        body = this.GetComponent<Rigidbody>();
        missileBody = transform.Find("Body").gameObject;
        missileWings = transform.Find("Wings").gameObject;
        missileTrail = transform.Find("Trail").gameObject;
        missileTrail.GetComponent<TrailRenderer>().time = trailLength;
        missileExplosion = transform.Find("Explosion").gameObject;
        homeTube = transform.parent.gameObject;
        from = transform.root.gameObject;
        fromLocal = from.GetComponent<ClientMaster>().isLocalPlayer;
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
                transform.position += Vector3.up * Time.deltaTime * MissileLauncher.missileSpeed * (0.1f + 0.9f * (seconds / launchLength));
                transform.position += Quaternion.Euler(0f, from.GetComponent<ClientMaster>().GetShipCore().GetRotation(), 0f) * Vector3.forward * Time.deltaTime * from.GetComponent<ClientMaster>().GetShipCore().GetSpeed() * (1f - (seconds / launchLength));
                this.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                break;
            case MissileState.HOMING://track laser point and turn towards it
                direction = trackPoint - this.transform.position;
                heading = direction.normalized;// / direction.magnitude;//normalize
                look = Quaternion.LookRotation(heading);
                body.rotation = Quaternion.RotateTowards(body.rotation, look, MissileLauncher.missileTrackStrength * Time.deltaTime);
                body.position += transform.forward * Time.deltaTime * MissileLauncher.missileSpeed;
                break;
        }

        if(state != MissileState.IDLE && state != MissileState.WAITING)//flight time
        {
            seconds += Time.deltaTime;
            if(seconds >= 6)//repool after 6 seconds
            {
                Explode();
            }
            else if(state != MissileState.HOMING && seconds >= launchLength)//start homing after half a second
            {
                body.isKinematic = false;
                state = MissileState.HOMING;
            }
            if (transform.position.y < -5f)
            {
                Explode();
            }
        }

        if(state == MissileState.IDLE)
        {
            if (transform.localPosition != Vector3.zero)
            {
                transform.localPosition = Vector3.zero;
            }
            if (transform.parent == null)
            {
                transform.parent = homeTube.transform;
            }
        }

        // Missile Wing Animation
        if (seconds <= 0.25f && (state == MissileState.LAUNCHED || state == MissileState.HOMING) && missileWings.transform.localRotation.eulerAngles.y != 90f)
        {
            missileWings.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        }
        else if (seconds > 0.25f && seconds <= 0.55f && (state == MissileState.LAUNCHED || state == MissileState.HOMING))
        {
            missileWings.transform.localRotation = Quaternion.Euler(0f, 90f * (1f - ((seconds - 0.25f) / 0.3f)), 0f);
        }
        else if (seconds > 0.55f && (state == MissileState.LAUNCHED || state == MissileState.HOMING) && missileWings.transform.localRotation.eulerAngles.y != 0f)
        {
            missileWings.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((state == MissileState.LAUNCHED || state == MissileState.HOMING) && (collision.gameObject.layer == 9 || collision.gameObject.layer == 11))
        {
            if (collision.gameObject.layer == 11 && fromLocal)
            {
                collision.gameObject.GetComponent<Iceberg>().LocalDamage(10000f, from);
            }
            Explode();
        }
    }

    public void Launch()//launches the missile, starting the flight sequence
    {
        missileTrail.GetComponent<TrailRenderer>().enabled = true;
        state = MissileState.LAUNCHED;
        missileBody.SetActive(true);
        missileWings.SetActive(true);
        transform.parent = null;
    }

    public void UpdateTrackingData(Vector3 trackPoint)//called by MissileLauncher.cs to give missile a target. gets called a lot
    {
        this.trackPoint = trackPoint;
    }

    private void Explode()//puts missile back in object pool
    {
        body.isKinematic = true;
        state = MissileState.WAITING;
        StartCoroutine(TrailLinger());
        missileBody.SetActive(false);
        missileWings.SetActive(false);
        missileExplosion.GetComponent<ParticleSystem>().Play();
        seconds = 0;
    }

    IEnumerator TrailLinger()
    {
        yield return new WaitForSeconds(trailLength);
        state = MissileState.IDLE;
        missileTrail.GetComponent<TrailRenderer>().enabled = false;
        transform.parent = homeTube.transform;
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.Euler(-90, 0, 0);
    }
}
