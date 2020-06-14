using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaijuCore : MonoBehaviour
{
    private GameObject kaijuPivot;
    private Vector3 kaijuDesiredMovement;
    private Vector3 kaijuMovement;
    [SerializeField] private float kaijuSurfAcc = 65f;
    [SerializeField] private float kaijuSurfSpeedMax = 100f;
    [SerializeField] private float kaijuSubAcc = 240f;
    [SerializeField] private float kaijuSubSpeedMax = 800f;
    private float kaijuDesiredRotation = 0f;
    private float kaijuRotation = 0f;
    private float kaijuRotationSpeed = 0f;
    [SerializeField] private float kaijuSurfRotSpeedMax = 45f;
    [SerializeField] private float kaijuSurfRotAcc = 75f;
    [SerializeField] private float kaijuSubRotSpeedMax = 360f;
    [SerializeField] private float kaijuSubRotAcc = 720f;
    private float submergedTime = 0f;
    private float submergedOffset = 0f;
    private float submergedCooldown = 0f;
    [SerializeField] private float submergedTimeMax = 9.1f;
    [SerializeField] private float submergedTransitionTime = 1.55f;
    [SerializeField] private float submergedOffsetMax = 300f;
    [SerializeField] private float submergedForwardPriority = 0.35f; // The priority modifier forward speed has over all other directions while the kaiju is submerged.
    [SerializeField] private float submergedCooldownMax = 10f;
    private bool icebeam = false;
    private Vector3 icebeamPoint;
    private Vector3[] icebergs;
    private int icebergsSpawned = 0;
    private float icebeamTime = 0f;
    private float icebeamCooldown = 0f;
    [SerializeField] private float icebeamTimeMax = 4f;
    [SerializeField] private float icebeamStepLength = 80f;
    [SerializeField] private float icebeamLengthMax = 1200f;
    [SerializeField] private float icebeamRangeMax = 8000f;
    [SerializeField] private float icebeamCooldownMax = 8f;
    private bool slash = false;
    private GameObject slashObject;
    private float slashCooldown = 0f;
    [SerializeField] private float slashDamageMax = 5f;
    [SerializeField] private float slashCooldownMax = 0.85f;

    private void Start()
    {
        kaijuPivot = transform.Find("Kaiju Pivot").gameObject;
        slashObject = kaijuPivot.transform.Find("Slash").gameObject;
    }

    private void Update()
    {
        Vector2 rot;
        if (FullySubmerged())
        {
            rot = RotateTo(kaijuRotation, kaijuDesiredRotation, kaijuRotationSpeed, kaijuSubRotSpeedMax, kaijuSubRotAcc, 0.5f);
            kaijuMovement = new Vector3(IncreaseTo(kaijuMovement.x, kaijuDesiredMovement.x * kaijuSubSpeedMax, kaijuSubAcc), 0f, IncreaseTo(kaijuMovement.z, kaijuDesiredMovement.z * kaijuSubSpeedMax, kaijuSubAcc));
        }
        else if (submergedTime <= submergedTransitionTime && submergedTime > 0f && kaijuMovement.magnitude > kaijuSurfSpeedMax) // Transition speed braking.
        {
            rot = RotateTo(kaijuRotation, kaijuDesiredRotation, kaijuRotationSpeed, kaijuSurfRotSpeedMax, kaijuSurfRotAcc, 0.5f);
            kaijuMovement = new Vector3(IncreaseTo(kaijuMovement.x, kaijuDesiredMovement.x * kaijuSurfSpeedMax, kaijuSubSpeedMax / submergedTransitionTime), 0f, IncreaseTo(kaijuMovement.z, kaijuDesiredMovement.z * kaijuSurfSpeedMax, kaijuSubSpeedMax / submergedTransitionTime));
        }
        else
        {
            rot = RotateTo(kaijuRotation, kaijuDesiredRotation, kaijuRotationSpeed, kaijuSurfRotSpeedMax, kaijuSurfRotAcc, 0.5f);
            kaijuMovement = new Vector3(IncreaseTo(kaijuMovement.x, kaijuDesiredMovement.x * kaijuSurfSpeedMax, kaijuSurfAcc), 0f, IncreaseTo(kaijuMovement.z, kaijuDesiredMovement.z * kaijuSurfSpeedMax, kaijuSurfAcc));
        }
        kaijuRotation = rot.x;
        kaijuRotationSpeed = rot.y;
        kaijuPivot.transform.rotation = Quaternion.Euler(0f, kaijuRotation, 0f);
        transform.position += new Vector3(kaijuMovement.x, 0f, kaijuMovement.z) * Time.deltaTime;

        //Submerge
        submergedCooldown = Mathf.Clamp(submergedCooldown - Time.deltaTime, 0f, submergedCooldownMax);
        if (submergedTime > 0f)
        {
            submergedTime = Mathf.Clamp(submergedTime - Time.deltaTime, 0f, submergedTimeMax);
            if (submergedTime == 0f)
            {
                submergedCooldown = submergedCooldownMax;
            }
        }
        if (submergedTime <= submergedTransitionTime && submergedTime > 0f)
        {
            submergedOffset = submergedOffsetMax * Mathf.Pow((submergedTime / submergedTransitionTime), 2f);
        }
        else if (submergedTime >= submergedTimeMax - submergedTransitionTime)
        {
            submergedOffset = submergedOffsetMax * Mathf.Pow(((submergedTimeMax - submergedTime) / submergedTransitionTime), 2f);
        }
        else if (submergedTime > 0f)
        {
            submergedOffset = submergedOffsetMax;
        }
        else
        {
            submergedOffset = 0f;
        }
        kaijuPivot.transform.localPosition = new Vector3(0f, -submergedOffset, 0f);

        //Slash
        slashCooldown = Mathf.Clamp(slashCooldown - Time.deltaTime, 0f, slashCooldownMax);

        //Icebeam
        icebeamCooldown = Mathf.Clamp(icebeamCooldown - Time.deltaTime, 0f, icebeamCooldownMax);
        if (icebeamTime > 0f)
        {
            bool pointClear = true;
            for (int i = 0; i < icebergsSpawned; i++)
            {
                if ((icebergs[i] - icebeamPoint).magnitude < icebeamStepLength)
                {
                    pointClear = false;
                }
            }
            if (pointClear)
            {
                icebergs[icebergsSpawned] = SpawnIceberg((icebergs[icebergsSpawned - 1] + (icebeamPoint - icebergs[icebergsSpawned - 1]).normalized * icebeamStepLength));
                icebergsSpawned++;
            }
            icebeamTime = Mathf.Clamp(icebeamTime - Time.deltaTime, 0f, icebeamTimeMax);
            if (icebeamTime == 0f || icebergsSpawned == icebergs.Length)
            {
                icebeamTime = 0f;
                icebeamCooldown = icebeamCooldownMax;
            }
        }
    }

    //Control Direction
    public void SetDesiredRotation(float desiredRotation)
    {
        kaijuDesiredRotation = desiredRotation;
    }

    public float GetDesiredRotation()
    {
        return kaijuDesiredRotation;
    }

    public void HardSetRotation(float rotation)
    {
        kaijuRotation = rotation;
    }

    public float GetRotation()
    {
        return kaijuRotation;
    }

    //Control Move
    public void SetDesiredMovement(Vector3 desiredMovement)
    {
        // Apply forward priority to desired speed while the kaiju is submerged.
        if (FullySubmerged())
        {
            if (desiredMovement.z > 0f)
            {
                desiredMovement = new Vector3(desiredMovement.x * (1f - submergedForwardPriority), 0f, desiredMovement.z);
            }
            else
            {
                desiredMovement = desiredMovement * (1f - submergedForwardPriority);
            }
        }
        kaijuDesiredMovement = Quaternion.Euler(0f, kaijuRotation, 0f) * desiredMovement;
    }

    public Vector3 GetDesiredMovement()
    {
        return kaijuDesiredMovement;
    }

    public void HardSetMovement(Vector3 movement)
    {
        kaijuMovement = movement;
    }

    public Vector3 GetMovement()
    {
        return kaijuMovement;
    }
    
    private bool FullySubmerged()
    {
        return submergedTime > submergedTransitionTime && submergedTime < submergedTimeMax - submergedTransitionTime;
    }

    private float IncreaseTo(float currentValue, float targetValue, float rate)
    {
        if (currentValue < targetValue)
        {
            if (currentValue + rate * Time.deltaTime > targetValue)
            {
                currentValue = targetValue;
            }
            else
            {
                currentValue += rate * Time.deltaTime;
            }
        }
        else if (currentValue > targetValue)
        {
            if (currentValue - rate * Time.deltaTime < targetValue)
            {
                currentValue = targetValue;
            }
            else
            {
                currentValue -= rate * Time.deltaTime;
            }
        }
        return currentValue;
    }

    //Control Submerge
    public void SetSubmerge()
    {
        if (submergedTime == 0f && submergedCooldown == 0f)
        {
            submergedTime = submergedTimeMax;
        }
        else if (FullySubmerged())
        {
            submergedTime = submergedTransitionTime;
        }
    }

    public void HardSetSubmerged(float submerged)
    {
        submergedTime = submerged;
    }

    public float GetSubmerged()
    {
        return submergedTime;
    }

    //Control Slash
    public void TriggerSlash()
    {
        if (slashCooldown == 0f && !FullySubmerged())
        {
            HardTriggerSlash();
            transform.parent.GetComponent<ClientMaster>().CmdTriggerSlash();
        }
    }

    public void HardTriggerSlash()
    {
        slashCooldown = slashCooldownMax;
        slashObject.GetComponent<KaijuSlash>().TriggerSlash(slashDamageMax);
    }

    //Control Icebeam
    public void TriggerIcebeam()
    {
        if (icebeamTime == 0f && icebeamCooldown == 0f && !FullySubmerged() && Mathf.Abs(RotateDifference(kaijuRotation, kaijuDesiredRotation)) < 75f)
        {
            icebergs = new Vector3[Mathf.CeilToInt(icebeamLengthMax / icebeamStepLength)];
            icebergs[0] = SpawnIceberg(icebeamPoint);
            icebergsSpawned = 1;
            icebeamTime = icebeamTimeMax;
        }
    }
    public void UpdateIcebeam(Vector3 icePoint)
    {
        icebeamPoint = new Vector3(icePoint.x, 0f, icePoint.z);
    }
    public bool IcebeamFiring()
    {
        return icebeamTime > 0f;
    }
    public float GetIcebeamRange()
    {
        return icebeamRangeMax;
    }
    private Vector3 SpawnIceberg(Vector3 position)
    {
        transform.parent.GetComponent<ClientMaster>().CmdTriggerCreateIceberg(position.x, position.z);
        return position;
    }

    //Returns a Vector2 containing the changed value and the changed speed.
    private Vector2 RotateTo(float value, float target, float speed, float speedMax, float acceleration, float snap)
    {
        float v = RotateDifference(value, target);      // Used for rotations without a limit
        //float v = target - value;                     // Used for rotations with less than 360 degrees of range
        if (v >= 0f)
        {
            if ((speed / acceleration) * speedMax < v)
            {
                speed = Mathf.Clamp(speed + acceleration * Time.deltaTime, -speedMax, speedMax);
            }
            else
            {
                speed = Mathf.Clamp(speed - acceleration * Time.deltaTime, -speedMax, speedMax);
            }
            if (v + speed * Time.deltaTime <= 0f || v < snap)
            {
                value = target;
                speed = 0f;
            }
            else
            {
                value += speed * Time.deltaTime;
            }
        }
        else
        {
            if ((speed / acceleration) * speedMax > v)
            {
                speed = Mathf.Clamp(speed - acceleration * Time.deltaTime, -speedMax, speedMax);
            }
            else
            {
                speed = Mathf.Clamp(speed + acceleration * Time.deltaTime, -speedMax, speedMax);
            }
            if (v + speed * Time.deltaTime >= 0f || v > snap)
            {
                value = target;
                speed = 0f;
            }
            else
            {
                value += speed * Time.deltaTime;
            }
        }
        return new Vector2(value, speed);
    }

    // Calculate absolute difference in degrees between two angles.
    private float RotateDifference(float value, float target)
    {
        Vector3 dir = Quaternion.Euler(0f, 0f, target) * Vector3.up;
        dir = Quaternion.Euler(0f, 0f, -value) * dir;
        float v = -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        if (v > 180f || v < -180f)
        {
            v -= 360f * Mathf.Sign(v);
        }
        return v;
    }
}
