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

    private MissileState state = MissileState.IDLE;
    private Rigidbody body;
    private Vector3 trackPoint;

    // Start is called before the first frame update
    void Start()
    {
        body = this.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Stow()
    {
        state = MissileState.IDLE;
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
    }

    public void Launch()
    {
        state = MissileState.LAUNCHED;
    }

    public void StartTrack()
    {
        state = MissileState.HOMING;
    }

    public void UpdateTrackingData(Vector3 trackPoint)
    {
        this.trackPoint = trackPoint;
    }
}
