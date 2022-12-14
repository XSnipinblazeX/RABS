using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bullets {


public class Ammo : MonoBehaviour
{
        public bool HasBlowOutPannel;
	 bool CookingOff;
        public float HP;
	public float ChanceOfCookOff;	
	public float CookOffHp;
	public float ChanceOfExplosion;
        public float ExplosionHp;
	public float maxCookOffTimeBeforeExplosion;
	public float minCookOffTimeBeforeExplosion;
	public float TurretPopOffChance;
	public float FATALexplosionChance;
	public short CookOffMaxRandom;
	public GameObject startCook;
	public GameObject cookOff;
	public GameObject explosionNormal;
	public GameObject explosionNoMoreTank;
	public GameObject tankBits;
	public TankComponentManager parentTank;
	public Transform[] hatches;
	[HideInInspector] public GameObject _cookOff;
	[HideInInspector] public GameObject  _explosion2;   // Start is called before the first frame update
	[HideInInspector] public GameObject  _explosion;   // Start is called before the first frame update
    void Start()
    {
        
    }

	public void DestroyParticle()
	{
		Destroy(_cookOff.gameObject);
		Destroy(_explosion2.gameObject);
		Destroy(_explosion.gameObject);
		Destroy(gameObject);
	}

	IEnumerator Cook (int i)
	{
		Instantiate(startCook, hatches[i].position, hatches[i].rotation);
		yield return new WaitForSeconds(Random.Range(0, CookOffMaxRandom));
		 Instantiate(cookOff, hatches[i].position, hatches[i].rotation);
	}


     void CookOff()
	{
		print("Ammo cooking off");
		 for (int i = 0; i < hatches.Length; i++)
		{
			if(Random.Range(0, 100) > 50)
			{
		 	StartCoroutine(Cook(i));
			}
			else
			{
			 Instantiate(cookOff, hatches[i].position, hatches[i].rotation);
			}
		}
		parentTank.cookOff();
		this.gameObject.layer = 0;
	}
	void Explode()
	{
		print("Ammo exploded");
		 _explosion = Instantiate(explosionNormal, transform.position, Quaternion.identity) as GameObject;
		parentTank.Exploded();
		CookOff();
		this.gameObject.layer = 0;
	}
	void FatalExplode()
	{
		print(" ALL Ammo exploded");
		 _explosion2 = Instantiate(explosionNormal, transform.position, Quaternion.identity) as GameObject;
		Destroy(gameObject);
		Destroy(_cookOff);
		parentTank.FatalExploded();
		this.gameObject.layer = 0;
	}

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Damage(float _damage)
	{
		HP -= _damage;
		if(ChanceOfCookOff > Random.Range(0, 100))
		{
			CookOff();
		}
		if(ChanceOfExplosion > Random.Range(0, 100) && !HasBlowOutPannel )
		{
			Explode();
		}
		if(TurretPopOffChance > Random.Range(0, 100) && !HasBlowOutPannel )
		{

		}
		if(FATALexplosionChance > Random.Range(0, 100) && !HasBlowOutPannel )
		{
			FatalExplode();
		}
	}
}


}
