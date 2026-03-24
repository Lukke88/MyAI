using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
	public GameObject bullet;
	public Vector3 targetPoint;
	public float bullet_speed = 200.0f;
    // Start is called before the first frame update
    void Start()
    {
        bullet = this.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if(targetPoint!=Vector3.zero)
		{
			bullet.transform.position = Vector3.MoveTowards(bullet.transform.position, targetPoint, bullet_speed*Time.deltaTime);
		}
    }
}
