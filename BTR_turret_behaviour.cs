using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTR_turret_behaviour : MonoBehaviour
{
	public GameObject turret, cursor, muzzleObject, cannon, projectilePrefab, generatedProjectile, generatedFlash;
	public float distance, z_difference, angle;
    // Start is called before the first frame update
    void Start()
    {
        turret = GameObject.Find(this.name);
    }

    // Update is called once per frame
    void Update()
    {
        if(turret!=null)
		{
			//turret.transform.rotation = Quaternion.Euler(0,-45.0f, 0.0f);
		}
		if(cursor==null)
		{
			cursor = GameObject.Find("Cursor_X");
		}
		else if(cursor!=null)
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			
			if(Physics.Raycast(ray, out hit, 2000))
			{
				cursor.transform.position = hit.point;
			}
			TurretRotationFunctionOriginal();
			cannon = turret.transform.GetChild(0).gameObject;
			muzzleObject = cannon.transform.GetChild(0).gameObject;
			
			if(cannon!=null && muzzleObject!=null)
			{
				projectilePrefab = GameObject.Find("projectile");
				
				if(generatedProjectile==null && Input.GetKeyDown(KeyCode.Space))
				{
					generatedProjectile = Instantiate(projectilePrefab, muzzleObject.transform.position, muzzleObject.transform.rotation);
					CreateFlash();
				}
				else if(generatedProjectile != null ) // shot
				{
					generatedProjectile.transform.GetComponent<Projectile_Behaviour>().muzzleObject2 = muzzleObject;//creates forward vector
					generatedProjectile.transform.GetComponent<Projectile_Behaviour>().IsTriggered = true;

				//generatedProjectile = null;
				}
				if(generatedProjectile!=null)
				if(Vector3.Distance(turret.transform.position,generatedProjectile.transform.position)>=50.0f)
					generatedProjectile = null;//can be created new one after then
			}
		}
    }
	
	public void CreateFlash()
	{
		GameObject flash = GameObject.Find("WFX_MF 4P RIFLE2");
		
		if(turret!=null && generatedFlash==null)
		{
			Quaternion flashRotation =
            turret.transform.rotation * Quaternion.Euler(0f, 180f, 0f);

        generatedFlash = Instantiate(
            flash,
            muzzleObject.transform.position,
            flashRotation
        );
		
			ParticleSystem ps = generatedFlash.transform.GetComponent<ParticleSystem>();
			ps.Play();
			
		}
		if(generatedFlash!=null)
		{
			Destroy(generatedFlash, 2.0f);
		}
	}
	
	public void TurretRotationFunctionOriginal()
	{
		//turret rotation, not erase, works perfectly !!!
			distance = Vector3.Distance(turret.transform.position, cursor.transform.position);
			z_difference = Mathf.Abs(turret.transform.position.z - cursor.transform.position.z);
			angle = Mathf.Asin(z_difference/distance)*180/Mathf.PI;
			if(cursor.transform.position.x<turret.transform.position.x)
			{
				if(cursor.transform.position.z<turret.transform.position.z)
					turret.transform.rotation = Quaternion.Euler(0, -angle, 0);
				else if(cursor.transform.position.z>=turret.transform.position.z)
				{
					turret.transform.rotation = Quaternion.Euler(0, angle, 0);
				}
			}
			else if(cursor.transform.position.x>=turret.transform.position.x)
			{
			if(cursor.transform.position.z>turret.transform.position.z)	
			turret.transform.rotation = Quaternion.Euler(0, -angle+180, 0);
			else
			{
				turret.transform.rotation = Quaternion.Euler(0, angle+180, 0);
			}
			}
	}
}
