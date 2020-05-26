using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaijuSlash : MonoBehaviour
{
    private float lifetime = 0f;
    [SerializeField] private float lifetimeMax = 0.45f;
    private float size = 0f;
    [SerializeField] private float startingSize = 150f;
    [SerializeField] private float endingSize = 500f;
    private float damage = 0f;
    private Material slashMaterial;
    [SerializeField] private AnimationCurve alphaOverTime;
    Color c = Color.white;

    private void Start()
    {
        transform.localScale = Vector3.zero;
        slashMaterial = GetComponent<Renderer>().material;
        c.a = 0f;
        slashMaterial.color = c;
    }
    
    private void Update()
    {
        if (lifetime > 0f)
        {
            lifetime = Mathf.Clamp(lifetime - Time.deltaTime, 0f, lifetimeMax);
            c.a = alphaOverTime.Evaluate(1f - (lifetime / lifetimeMax));
            transform.localScale = new Vector3(1f, 0.4f + 0.6f * (1f - (lifetime / lifetimeMax)), 1f) * (startingSize * (lifetime / lifetimeMax) + endingSize * (1f - (lifetime / lifetimeMax)));
            if (lifetime == 0f)
            {
                c.a = 0f;
                transform.localScale = Vector3.zero;
            }
            slashMaterial.color = c;
        }
    }

    public void TriggerSlash(float d)
    {
        damage = d;
        lifetime = lifetimeMax;
    }
}
