using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCore : MonoBehaviour
{
    private float rotation;
    private float rudder;
    private float rudderTarget;
    [SerializeField] private float rudderTurnRate = 10f;
    [SerializeField] private float rudderMaximum = 20f;
    [SerializeField] private float rudderMaxEffectSpeed = 25f;
    private float throttle;
    private float speed;
    [SerializeField] private float speedMaxFwd = 125f;
    [SerializeField] private float speedMaxRev = 25f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float coastDrag = 7f;
    [SerializeField] private float health = 30f;
    
    private GameObject shipPivot;

    void Start()
    {
        shipPivot = transform.Find("Ship Pivot").gameObject;
        rotation = shipPivot.transform.eulerAngles.y;
    }

    void Update()
    {
        // Rudder Control
        rudder = IncreaseTo(rudder, rudderTarget, rudderTurnRate * RudderSlowing(speed, speedMaxFwd, -0.2f), rudderTurnRate * RudderSlowing(speed, speedMaxFwd, 0.4f));
        // Speed Control and Coasting
        if (speed > throttle && throttle >= 0f)
        { // Coast if speed is above throttle but throttle not in reverse, also affected by rudder slowing
            speed = IncreaseTo(speed, throttle * RudderSlowing(rudder, rudderMaximum, 0.2f), coastDrag * (1f - Mathf.Abs(rudder) / rudderMaximum) + acceleration * (Mathf.Abs(rudder) / rudderMaximum));
        }
        else
        {
            speed = IncreaseTo(speed, throttle * RudderSlowing(rudder, rudderMaximum, 0.2f), acceleration);
        }
        //Debug.Log("Rudder: " + rudder + " Speed: " + speed + " Throttle: " + throttle);
        // Apply Transforms
        rotation += rudder * RudderEffectiveness(speed, rudderMaxEffectSpeed) * Time.deltaTime;
        shipPivot.transform.rotation = Quaternion.Euler(0f, rotation, Mathf.Clamp(rudder * Mathf.Abs(RudderEffectiveness(speed, rudderMaxEffectSpeed) * (speed / speedMaxFwd)) * 0.45f, -7.2f, 7.2f));
        transform.position += shipPivot.transform.forward * speed * Time.deltaTime;
    }

    public void Damage(float damage)
    {
        health -= damage;
        if (health < 0f)
        {
            health = 0f;
        }
    }

    public float GetHealth()
    {
        return health;
    }

    public Vector3 GetVelocity()
    {
        return shipPivot.transform.forward * speed;
    }
    
    public void SetRudder(float r)
    {
        rudderTarget = r * rudderMaximum;
    }

    public float GetRudder()
    {
        return rudderTarget;
    }

    public void HardSetRudder(float r)
    {
        rudder = r;
    }

    public float GetActualRudder()
    {
        return rudder;
    }

    public void HardSetRotation(float rot)
    {
        rotation = rot;
    }

    public float GetRotation()
    {
        return rotation;
    }

    public void SetThrottle(float t)
    {
        throttle = t * GetSpeedMax(t);
    }

    public float GetThrottle()
    {
        return throttle;
    }

    public void HardSetSpeed(float s)
    {
        speed = s;
    }

    public float GetSpeed()
    {
        return speed;
    }

    private float RudderEffectiveness(float spd, float maxEffectSpeed)
    {
        return Mathf.Clamp(Mathf.Abs(spd) / maxEffectSpeed, 0f, 1f) * Mathf.Sign(spd);
    }

    private float RudderSlowing(float rudder, float rudderMaximum, float slowing)
    {
        return 1f - (Mathf.Abs(rudder) / rudderMaximum) * slowing;
    }

    private float GetSpeedMax(float t)
    {
        if (t >= 0)
        {
            return speedMaxFwd;
        }
        else
        {
            return speedMaxRev;
        }
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

    private float IncreaseTo(float currentValue, float targetValue, float rateInward, float rateOutward)
    {
        if ((currentValue > 0 && currentValue > targetValue) || (currentValue < 0 && currentValue < targetValue))
        {
            currentValue = IncreaseTo(currentValue, targetValue, rateInward);
        }
        else
        {
            currentValue = IncreaseTo(currentValue, targetValue, rateOutward);
        }
        return currentValue;
    }
}
