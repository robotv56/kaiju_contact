using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCollision : MonoBehaviour
{
    private void OnCollisionStay(Collision collision)
    {
        //Debug.Log("Hit shit on this layer: " + collision.gameObject.layer);
        if (collision.gameObject.layer == 9 || collision.gameObject.layer == 11)
        {
            Vector3 dir = Vector3.zero;
            for (int i = 0; i < collision.contacts.Length; i++)
            {
                dir += collision.contacts[i].point / collision.contacts.Length;
            }
            dir -= transform.parent.parent.position;
            dir = new Vector3(dir.x, 0f, dir.z);
            dir = Quaternion.Euler(0f, -transform.parent.parent.GetComponent<ShipCore>().GetRotation(), 0f) * dir.normalized;
            if (dir.z > 0.2f)
            {
                if (transform.parent.parent.GetComponent<ShipCore>().GetSpeed() > 15f && collision.gameObject.layer == 11)
                    transform.parent.parent.GetComponent<ShipCore>().HardSetSpeed(15f);
            }
            if (dir.z < -0.2f)
            {
                if (transform.parent.parent.GetComponent<ShipCore>().GetSpeed() < -8f && collision.gameObject.layer == 11)
                    transform.parent.parent.GetComponent<ShipCore>().HardSetSpeed(-8f);
            }
                float maxDist = 220f;
                float minDist = 80f;
            if (collision.gameObject.layer == 9)
            {
                maxDist = 300f;
                minDist = 160f;
            }
            dir = collision.transform.position - transform.parent.parent.position;
            dir = new Vector3(dir.x, 0f, dir.z);
            if (dir.magnitude < minDist)
            {
                transform.parent.parent.position = new Vector3(collision.transform.position.x, 0f, collision.transform.position.z) - dir.normalized * minDist;
            }
            float mag = Mathf.Pow(1f - Mathf.Clamp((dir.magnitude - minDist) / (maxDist - minDist), 0f, 1f), 2f);
            transform.parent.parent.position += dir.normalized * -mag * 15f * Time.fixedDeltaTime;
        }
    }
}
