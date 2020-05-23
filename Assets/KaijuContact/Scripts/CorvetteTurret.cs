using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorvetteTurret : MonoBehaviour {

    private Vector3 aimPoint;
    private Vector3 aim;
    private CorvetteCore core;
    private GameObject shipPivot;
    private GameObject cannonPivot;
    private GameObject[] cannons = { null, null };
    private float turretRotation = 0f;
    private float turretRotationSpeed = 0f;
    [SerializeField] GameObject shellPrefab;
    [SerializeField] private float turretRotationSpeedMax = 75f;
    [SerializeField] float turretRotationAcceleration = 360f;
    [SerializeField] private float turretRotationMax = 120f;
    private float cannonRotation = 0f;
    private float cannonRotationSpeed = 0f;
    [SerializeField] private float cannonRotationSpeedMax = 25f;
    [SerializeField] private float cannonRotationAcceleration = 360f;
    [SerializeField] private float cannonRotationMax_up = 40f;
    [SerializeField] private float cannonRotationMax_down = 10f;
    [SerializeField] private float cannonVelocity = 60f;
    [SerializeField] private float cannonAccuracy = 3f;
    [SerializeField] private float cannonGravity = 1.25f;
    private float cannonReload = 0f;
    [SerializeField] private float cannonReloadLength = 3.8f;
    [SerializeField] private float cannonDamage = 1f;
    private bool onTarget = false;

    void Start() {
        core = transform.root.GetComponent<CorvetteCore>();
        shipPivot = transform.parent.parent.gameObject;
        cannonPivot = transform.Find("Corvette Cannons").gameObject;
        cannons[0] = cannonPivot.transform.Find("Corvette Cannon Right").gameObject;
        cannons[1] = cannonPivot.transform.Find("Corvette Cannon Left").gameObject;
    }
    
    void Update() {
        //Account for gravity
        Vector3 aimDir1 = (aimPoint - transform.position).normalized;
        Vector3 aimDir2 = Quaternion.Euler(0f, -Mathf.Atan2(aimDir1.x, aimDir1.z) * Mathf.Rad2Deg, 0f) * aimDir1;
        float aimCompensation = 0.5f * Mathf.Asin(cannonGravity * (aimPoint - transform.position).magnitude / Mathf.Pow(cannonVelocity, 2f)) * Mathf.Rad2Deg;
        aimDir2 = Quaternion.Euler(-aimCompensation, 0f, 0f) * aimDir2;
        aimDir2 = Quaternion.Euler(0f, Mathf.Atan2(aimDir1.x, aimDir1.z) * Mathf.Rad2Deg, 0f) * aimDir2;
        //Account for ship angle and heading
        aimDir2 = Quaternion.Euler(0f, -shipPivot.transform.eulerAngles.y, 0f) * aimDir2;
        aimDir2 = Quaternion.Euler(0f, 0f, -shipPivot.transform.eulerAngles.z) * aimDir2;
        //Aim Turret
        Vector2 rot1 = RotateTo(turretRotation, Mathf.Clamp(Mathf.Atan2(aimDir2.x, aimDir2.z) * Mathf.Rad2Deg, -turretRotationMax, turretRotationMax), turretRotationSpeed, turretRotationSpeedMax, turretRotationAcceleration, 0.5f);
        turretRotation = rot1.x;
        turretRotationSpeed = rot1.y;
        transform.localEulerAngles = new Vector3(0f, turretRotation, 0f);
        //Aim Cannons
        Vector2 rot2 = RotateTo(cannonRotation, Mathf.Clamp(-Mathf.Atan2(aimDir2.y, new Vector2(aimDir2.x,aimDir2.z).magnitude) * Mathf.Rad2Deg, -cannonRotationMax_down, cannonRotationMax_up), cannonRotationSpeed, cannonRotationSpeedMax, cannonRotationAcceleration, 0.5f);
        cannonRotation = rot2.x;
        cannonRotationSpeed = rot2.y;
        cannonPivot.transform.localEulerAngles = new Vector3(cannonRotation, 0f, 0f);
        //Check if On Target
        onTarget = Mathf.Abs(RotateDifference(turretRotation, Mathf.Atan2(aimDir2.x, aimDir2.z) * Mathf.Rad2Deg)) < 2.5f;
        //Reload
        cannonReload = Mathf.Clamp(cannonReload - Time.deltaTime, 0f, cannonReloadLength);
    }

    public void FireCannons(int playerMask) {
        if (cannonReload == 0f) {
            cannonReload = cannonReloadLength;
            for (int i = 0; i < 2; i++) {
                GameObject shell = Instantiate(shellPrefab, cannons[i].transform.position, cannons[i].transform.rotation);
                Quaternion randomRotation = Quaternion.Euler(Random.Range(0f, cannonAccuracy), Random.Range(0f, cannonAccuracy), Random.Range(0f, cannonAccuracy));
                shell.GetComponent<ShellController>().Setup(randomRotation * cannonPivot.transform.forward * cannonVelocity, cannonGravity, playerMask, cannonDamage);
            }
        }
    }

    public Vector3 GetAimPoint() {
        return aimPoint;
    }

    public bool IfOnTarget() {
        return onTarget;
    }

    public void UpdateAim(Vector3 targetPoint, Vector3 targetVelocity) {
        Vector3 targDir = targetPoint - transform.position;
        float a = Vector3.Dot(targetVelocity, targetVelocity) - (cannonVelocity * cannonVelocity);
        float b = 2f * Vector3.Dot(targetVelocity, targDir);
        float c = Vector3.Dot(targDir, targDir);
        float p = -b / (2 * a);
        float q = Mathf.Sqrt((b * b) - 4 * a * c) / (2 * a);
        float ttt1 = p - q;
        float ttt2 = p + q;
        float timeToTarget;
        if (ttt1 > ttt2 && ttt2 > 0) {
            timeToTarget = ttt2;
        } else {
            timeToTarget = ttt1;
        }
        aimPoint = targetPoint + targetVelocity * timeToTarget;
    }

    //Returns a Vector2 containing the changed value and the changed speed;
    private Vector2 RotateTo(float value, float target, float speed, float speedMax, float acceleration, float snap) {
        //float v = RotateDifference(value, target);
        float v = target - value;
        if (v >= 0f) {
            if ((speed / acceleration) * speedMax < v) {
                speed = Mathf.Clamp(speed + acceleration * Time.deltaTime, -speedMax, speedMax);
            } else {
                speed = Mathf.Clamp(speed - acceleration * Time.deltaTime, -speedMax, speedMax);
            }
            if (v + speed * Time.deltaTime <= 0f || v < snap) {
                value = target;
                speed = 0f;
            } else {
                value += speed * Time.deltaTime;
            }
        } else {
            if ((speed / acceleration) * speedMax > v) {
                speed = Mathf.Clamp(speed - acceleration * Time.deltaTime, -speedMax, speedMax);
            } else {
                speed = Mathf.Clamp(speed + acceleration * Time.deltaTime, -speedMax, speedMax);
            }
            if (v + speed * Time.deltaTime >= 0f || v > snap) {
                value = target;
                speed = 0f;
            } else {
                value += speed * Time.deltaTime;
            }
        }
        return new Vector2(value, speed);
    }

    private float RotateDifference(float value, float target) {
        Vector3 dir = Quaternion.Euler(0f, 0f, target) * Vector3.up;
        dir = Quaternion.Euler(0f, 0f, -value) * dir;
        float v = -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        if (v > 180f || v < -180f) {
            v -= 360f * Mathf.Sign(v);
        }
        return v;
    }
}
