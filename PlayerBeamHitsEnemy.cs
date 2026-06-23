using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBeamHitsEnemy : MonoBehaviour
{
	public GameObject beam;
	public GameObject[] allEnemies;
	public float contact_distance = 10.0f;
	public string enemy_hit_animation = "hezbollah_terrorist_hit";
	public string enemy_hit_parameter = "IsHit";
	
	public float damage = 25.0f;
public AudioClip aarghClip;
    // Start is called before the first frame update
    void Start()
    {
        beam = GameObject.Find(this.name);
    }

    // Update is called once per frame
    void Update()
    {
        allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
		
		foreach(GameObject go in allEnemies)
	{
    if(Vector3.Distance(
        beam.transform.position,
        go.transform.position)
        <= contact_distance)
    {
        PlayEnemyHitAnimation(go);
    }
	}
    }
	
	void PlayEnemyHitAnimation(GameObject enemy)
{
    Animator animator =
        enemy.GetComponent<Animator>();

    if(animator != null)
    {
        animator.SetInteger(
            enemy_hit_parameter,
            1);

        animator.Play(
            enemy_hit_animation);
    }

    EnemyHealth health =
        enemy.GetComponent<EnemyHealth>();

    if(health != null)
    {
        if(Time.time - health.lastHitTime > 0.5f)
		{
			health.lastHitTime = Time.time;
			health.TakeDamage(damage);
		}
    }

    AudioSource audioSource =
        enemy.GetComponent<AudioSource>();

    if(audioSource != null &&
       aarghClip != null)
    {
        audioSource.PlayOneShot(
            aarghClip);
    }
}
}
