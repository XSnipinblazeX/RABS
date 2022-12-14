using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bullets {


public class EngineDamageController : MonoBehaviour
{
    [Header("Engine Settings")] //This is just more like a stat card. Also effect explosion, fire, failure, etc.
     public int MaxGears;
     public float MaxRPM;
     public float IdleRPM;
     public float CurrentRPM;
     public float RPMWaverAmount; //The RPM isn't the same all the time this adds some randomness to it so it is like it gets clumps of fuel
     public int FuelType; // 0 - Gasoline, 1 - Diesel, 2 - Petrol
     public int NMBCyclinders;
     public float NormalFailureChance; //Chance of failure under normal circumstances
     public float HorsePower;
     public float Torque;


    [Header("Engine Damage Variables (Totals)")]
    public float Integrity;
    private float currentIntegrity;
    public float FailureOnHitChance; //Chances of engine running or stopping on impact with bullet or spall
    public float MaxDamageRepairTime; //Max "do-able" repair time, if it is over this, repair is impossible
    public float MinDamageRepairTime; //Min "do-able" repair time, if it is under this, then no repair is needed
    private float repairTime;
    
    [Header("Engine Damage Fire Variables")]
    public float ChanceOfFire; //Chance of fire when hit
    public float MaxFireBurnTime; //max burn time for fire, calculated by the running RPM;
    public float FireSpreadToFuel; //Chance of engine fire spreading to fuel, regardless if extinguishing
    public float MaxFireDamage; //Max Damage a engine fire can cause;

    [Header("Engine Damage Fire GameObjects")]
	public GameObject fire;
	public GameObject fuelTank;
	
    [Header("Engine Damage Explosion Variables")]
	public float ChanceOfExplosion;
	public float ChanceOfFatalExplosion;
	public float ChanceOfFireAfterExplosion;
	public float ChanceOfExplosionDamagingWholeVehicle;
    [Header("Engine Damage Explosion GameObjects")]
	public GameObject smallExplosion;
	public GameObject FireballExplosion;
	public GameObject engineExplosion;

    [Header("Repair Time Calculation Booleans")]
	public float maxTimeInfluence; //max time influence the booleans have
	public float minTimeInfluence; //min time influence the booleans have

	public bool maxRPM;
	public bool idleRPM;

	public bool Above75Per; //integrity
	public bool Above50per; //integrity
	public bool Above25per; //integrity
	public bool below25per; //integrity
	public bool OnFire;
	public bool Exploded;
	public bool Failed;
    // Start is called before the first frame update
    void Start()
    {
        currentIntegrity = Integrity;
	if(FuelType >= 3)
	{
		FuelType = 3;
	}
	if(FuelType <= 0)
	{
		FuelType = 0;
	}
    }

    // Update is called once per frame
    void Update()
    {
        if(currentIntegrity >= 75.0f)
	{
		Above75Per = true;
	}
        if(currentIntegrity >= 50.0f)
	{
		Above50per = true;
	}
        if(currentIntegrity >= 25.0f)
	{
		Above25per = true;
	}
        if(currentIntegrity <= 25.0f)
	{
		below25per = true;
	}
    }
	public void CalculateRepairTime(float damage)
	{
		repairTime += damage / 8;
		if(Above75Per == true)
		{
			repairTime -= Random.Range(minTimeInfluence, maxTimeInfluence) / 2;
		}
		if(Above50per == true)
		{
			repairTime -= Random.Range(minTimeInfluence, maxTimeInfluence) / 5;
		}
		if(Above25per == true)
		{
			repairTime -= Random.Range(minTimeInfluence, maxTimeInfluence) / 10;
		}
		if(below25per == true)
		{
			repairTime += Random.Range(minTimeInfluence, maxTimeInfluence) * 2;
		}
		if(OnFire == true)
		{
			repairTime += Random.Range(minTimeInfluence, maxTimeInfluence) * 10;
		}
		if(Exploded == true)
		{
			repairTime += Random.Range(minTimeInfluence, maxTimeInfluence) * 15.0f;
		}
		if(Failed == true)
		{
			repairTime += Random.Range(minTimeInfluence, maxTimeInfluence) * 2;
		}
		if(repairTime <= 0)
		{
			repairTime = 1;
		}
		Debug.Log(repairTime);
			StartCoroutine(Repair());
	}

    public void TakeDamage(float _damage)
    {
		if(!Exploded) {
		currentIntegrity -= _damage;
		CalculateRepairTime(_damage);
		if(repairTime > MaxDamageRepairTime)
		{
			Debug.Log("repairs are impossible!");
		}
		if(repairTime < MinDamageRepairTime)
		{
			Debug.Log("'tis but a scratch");
		}
		if(repairTime > MinDamageRepairTime && repairTime < MaxDamageRepairTime)
		{
			Debug.Log("repairs starting");
			StartCoroutine(Repair());
		}
		if(ChanceOfExplosion < Random.Range(0, 100))
		{
			Instantiate(FireballExplosion, transform.position, transform.rotation);
			Exploded = true;
			repairTime += Random.Range(10.0f, 35.5f);
		}
		if(ChanceOfFatalExplosion < Random.Range(0, 100))
		{
			Instantiate(engineExplosion, transform.position, transform.rotation);
			Exploded = true;
			repairTime += Random.Range(60.0f, 120.5f);
			//Destroy(gameObject, 0.01f);
		}
		if(ChanceOfFireAfterExplosion < Random.Range(0, 100))
		{
			Instantiate(fire, transform.position, transform.rotation);
			OnFire = true;
		}
		}
    }	

    public IEnumerator Repair()
	{
		yield return new WaitForSeconds(repairTime);
		currentIntegrity = 75;
	 Above75Per = true; //integrity
	
	
	
	 OnFire = false;
	 Exploded = false;
	 Failed = false;	
	Debug.Log("Engine Repaired");	

	}	


}


}
