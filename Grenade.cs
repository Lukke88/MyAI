using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
	public bool IsGrenadeActivated;
    private DesertReaperBehaviour owner;

    public float delay = 2f;

    public void Init(DesertReaperBehaviour reaper)
    {
        owner = reaper;
        Invoke("Explode", delay);
    }

    void Explode()
    {
        // efekt
        if (owner.explosionEffect != null)
        {
            Instantiate(owner.explosionEffect, transform.position, Quaternion.identity);
        }

        if (owner.explosionAudio != null)
        {
            AudioSource.PlayClipAtPoint(owner.explosionAudio.clip, transform.position);
        }

        // physics
        Collider[] colliders = Physics.OverlapSphere(transform.position, owner.explosionRadius);

        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.AddExplosionForce(owner.explosionForce, transform.position, owner.explosionRadius);
            }

            // damage enemy
            if (nearbyObject.CompareTag("Enemy"))
            {
                Animator anim = nearbyObject.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.SetTrigger("Hit");
                }
            }
        }

        Destroy(gameObject);
    }
}
