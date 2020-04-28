using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellController : MonoBehaviour {

    private Vector3 velocity;
    private float gravity;
    private Vector3 lastPosition;
    private int playerMask;
    private float damage;
    private float aliveTime;
    private bool deleting = false;

    void Update() {
        if (!deleting) {
            aliveTime += Time.deltaTime;
            velocity += Vector3.down * gravity * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(velocity);
            lastPosition = transform.position;
            transform.position += velocity * Time.deltaTime * 3;
            if ((Physics.Linecast(lastPosition, transform.position, out RaycastHit shellHit, playerMask) || transform.position.y < -0.1f) && !deleting) {
                try
                {
                    if (shellHit.transform.gameObject.layer == 8 || shellHit.transform.gameObject.layer == 9)
                    {
                        shellHit.transform.root.GetComponent<CorvetteCore>().Damage(damage);
                    }
                }
                catch
                {
                    //TODO: handle exception properly
                }
                deleting = true;
                Destroy(transform.Find("Shell").gameObject);
                Destroy(transform.Find("Tracer").gameObject);
                Destroy(gameObject, 3f);
            }
        }
    }

    public void Setup(Vector3 v, float g, int m, float d) {
        velocity = v;
        gravity = g;
        playerMask = m;
        damage = d;
    }
}
