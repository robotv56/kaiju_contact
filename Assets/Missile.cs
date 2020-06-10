using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    public enum MissileState
    {
        IDLE,//in tube
        LAUNCHED,//just launched, not tracking
        HOMING//tracking laser
    }

    public MissileState state { get; private set; } = MissileState.IDLE;//thanks visual studio :D

    private Rigidbody body;
    private Vector3 trackPoint;

    // Start is called before the first frame update
    void Start()
    {
        body = this.GetComponent<Rigidbody>();
    }

    float t = 0;
    Quaternion look;
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
            case MissileState.IDLE:
                break;
            case MissileState.LAUNCHED:
                {
                    body.position += Vector3.up * Time.deltaTime * MissileLauncher.missileSpeed;
                }
                break;
            case MissileState.HOMING:
                {
                    direction = trackPoint - this.transform.position;
                    heading = direction / direction.magnitude;//normalize
                    look = Quaternion.LookRotation(heading);

                    //body.rotation = look;
                    body.rotation = Quaternion.RotateTowards(body.rotation, look, MissileLauncher.missileTrackStrength *Time.deltaTime);
                    body.position += transform.forward * Time.deltaTime * MissileLauncher.missileSpeed;
                }
                break;
        }

        if(state != MissileState.IDLE)
        {
            t += Time.deltaTime;
            if( t >= 5)
            {
                t = 0;
                Stow();
            }
            else if(state != MissileState.HOMING && t >= 1)
            {
                state = MissileState.HOMING;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //explode
        Debug.Log("impact registered");
        Stow();
    }

    public void Launch()
    {
        state = MissileState.LAUNCHED;
    }

    public void UpdateTrackingData(Vector3 trackPoint)
    {
        this.trackPoint = trackPoint;
    }

    private void Stow()
    {
        state = MissileState.IDLE;
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.Euler(-90,0,0);
        t = 0;
    }
}
