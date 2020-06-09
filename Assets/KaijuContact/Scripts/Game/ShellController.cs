using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellController : MonoBehaviour
{
    private GameObject from;
    private bool fromLocal;
    private Vector3 velocity;
    private float gravity;
    private Vector3 lastPosition;
    private int playerMask;
    private float damage;
    private float aliveTime;
    private float trailLifetime;
    private float maxLifetime;
    private bool deleting = false;

    private void Update()
    {
        if (!deleting) {
            aliveTime += Time.deltaTime;
            velocity += Vector3.down * gravity * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(velocity);
            lastPosition = transform.position;
            transform.position += velocity * Time.deltaTime;
            if (((Physics.Linecast(lastPosition, transform.position, out RaycastHit shellHit, playerMask) || transform.position.y < -0.1f) || aliveTime >= maxLifetime) && !deleting)
            {
                if (aliveTime < maxLifetime)
                {
                    bool hitLocal = false;
                    if (shellHit.transform.root.GetComponent<ClientMaster>())
                    {
                        hitLocal = shellHit.transform.root.GetComponent<ClientMaster>().GetIsLocalPlayer();
                    }
                    if (shellHit.transform.gameObject.layer == 11 && fromLocal)
                    {
                        shellHit.transform.GetComponent<Iceberg>().LocalDamage(damage, from);
                    }
                    if (shellHit.transform.gameObject.layer == 9 && hitLocal && shellHit.transform.gameObject.name != "Slash")
                    {
                        if (shellHit.transform.gameObject.name != "Kaiju Weakspot")
                        {
                            damage = damage * 0.25f;
                        }
                        shellHit.transform.root.GetComponent<ClientMaster>().CmdDamageKaiju(damage);
                    }
                }
                deleting = true;
                Destroy(transform.Find("Shell").gameObject);
                Destroy(transform.Find("Tracer").gameObject);
                if (aliveTime < trailLifetime)
                {
                    transform.Find("Trail").GetComponent<TrailRenderer>().time = aliveTime;
                    Destroy(gameObject, aliveTime);
                }
                else
                {
                    Destroy(gameObject, trailLifetime);
                }
            }
        }
    }

    public void Setup(Vector3 velocity, float gravity, int playerMask, float damage, float trailLifetime, float maxLifetime, GameObject from, bool fromLocal)
    {
        this.velocity = velocity;
        this.gravity = gravity;
        this.playerMask = playerMask;
        this.damage = damage;
        this.trailLifetime = trailLifetime;
        transform.Find("Trail").GetComponent<TrailRenderer>().time = trailLifetime;
        this.maxLifetime = maxLifetime;
        this.from = from;
        this.fromLocal = fromLocal;
    }
}
