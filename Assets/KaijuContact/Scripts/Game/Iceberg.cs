using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Iceberg : MonoBehaviour
{
    private int icebergID;
    private float sizeModifier = 0f;
    private bool growing = false;
    private bool shrinking = false;
    private float growSpeed = 2.8f;
    private float shrinkSpeed = 2.8f;
    
    private void Update()
    {
        if (growing)
        {
            sizeModifier = Mathf.Clamp(sizeModifier + Time.deltaTime / growSpeed, 0f, 1f);
            if (sizeModifier >= 1f)
            {
                sizeModifier = 1f;
                growing = false;
            }
        }
        if (shrinking)
        {
            sizeModifier = Mathf.Clamp(sizeModifier - Time.deltaTime / shrinkSpeed, 0f, 1f);
            if(sizeModifier <= 0f)
            {
                sizeModifier = 0f;
                shrinking = false;
                gameObject.SetActive(false);
            }
        }

        transform.localScale = new Vector3(1f, 1f - 0.8f * Mathf.Pow(1f - sizeModifier, 2f), 1f);
        transform.position = new Vector3(transform.position.x, -40f * Mathf.Pow(1f - sizeModifier, 2f), transform.position.z);
    }

    public void Setup(int id)
    {
        icebergID = id;
        transform.rotation = Quaternion.Euler(0f, Random.value * 360f, 0f);
    }

    public void Grow(float speed)
    {
        growing = true;
        shrinking = false;
        sizeModifier = 0f;
        growSpeed = speed;
    }

    public void Shrink(float speed)
    {
        growing = false;
        shrinking = true;
        sizeModifier = 1f;
        shrinkSpeed = speed;
    }

    public bool GetShrinking()
    {
        return shrinking;
    }

    public void LocalDamage(float damage, GameObject from)
    {
        if (!shrinking)
        {
            from.GetComponent<ClientMaster>().CmdTriggerDamageIceberg(icebergID, damage);
        }
    }

    public int GetID()
    {
        return icebergID;
    }
}
