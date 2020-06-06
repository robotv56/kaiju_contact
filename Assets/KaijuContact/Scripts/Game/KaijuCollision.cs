using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaijuCollision : MonoBehaviour
{
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == 11)
        {
            /*Vector3 dir = Vector3.zero;
            for (int i = 0; i < collision.contacts.Length; i++)
            {
                dir += collision.contacts[i].point / collision.contacts.Length;
            }
            dir -= transform.position;
            if (dir.magnitude < 120f)
            {*/

            if (collision.transform.gameObject.GetComponent<Iceberg>() && transform.root.GetComponent<ClientMaster>().GetIsLocalPlayer())
            {
                collision.transform.gameObject.GetComponent<Iceberg>().LocalDamage(10000f, transform.root.gameObject);
            }
            //}
        }
    }
}
