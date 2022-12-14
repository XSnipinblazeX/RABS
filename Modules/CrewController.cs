using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bullets {

public class CrewController : MonoBehaviour
{
	public string Position;
	private TankComponentManager tankController;
	public float MaxHP;
	[HideInInspector] public float currentHP;
	public float RegenSpeed; //How much it regens per second
	public float MaxRegenHealth;
	public float CritHP;
	public bool IsAlive;
	public bool IsCrit;
	public bool HasRegen;
	public bool IsOut; //Knocked out/blacked out
	public bool IsAwake; //Awakened from KO

	public float KOmaxHP; //Max HP before crew is unconsious (still alive)
	public float KOchance; //Chance of crew being knocked unconsious and still be alive;
	public float RegainConsciousSpeed; //How fast they wake up;
	public float MaxKO_Time; //the longest he will be KO
	public float MinKO_Time; //the least he will be KO'ed
	public float KOdeathChance; //Chance crew will die while unconscious

	private bool KOLoop = true;
    // Start is called before the first frame update
    void Start()
    {
        currentHP = MaxHP;
	tankController = GetComponentInParent<TankComponentManager>();
    }

    // Update is called once per frame
    void Update()
    {
        //Check if crew is dead
	if(currentHP < 0)
	{
		IsAlive = false;
		//Die();
	}
	// Check if crew is in critical condition
	if(currentHP < CritHP)
	{
		IsCrit = true;
	}
	//Check if he passed out

    }

	public IEnumerator PassedOut()
	{
		
		yield return new WaitForSeconds(Random.Range(MinKO_Time, MaxKO_Time));
		Debug.Log(name + " is breathing again");
		KOLoop = false;
		RegainCon();
	}

	public void RegainCon()
	{
		if(KOLoop == false && IsAlive == true)
		{
		//Check to see if he died
			if(Random.Range(0, 100) < KOdeathChance && IsAlive == true)
			{
				IsAlive = false;
				Debug.Log(name + " died to his injuries");
				KOLoop = false;
				Die();
			}
		
			else
			{

				Debug.Log(name + "woke up");
				IsAlive = true;
				IsAwake = true;
				IsOut = false;
				currentHP = 51.0f;
				KOLoop = true;
			}
			
		}
		else
		{
			Debug.Log(name + " was knocked unconscious and killed");
			IsAlive = false;
				KOLoop = false;
			Die();
		}
	}
	public IEnumerator Regen()
	{
		yield return new WaitForSeconds(1);
		if(currentHP < MaxRegenHealth && IsAlive == true)
		{
			currentHP += RegenSpeed;
		}
		if(currentHP >= MaxRegenHealth && IsAlive == true)
		{
			HasRegen = true;
		}
	

	}
	public void LoaderCovered(string _name)
	{
		Debug.Log("Loader Is Covered by " + _name);
	}
	public void DriverCovered(string _name)
	{
		Debug.Log("Driver Is Covered by " + _name);
		if(Position == _name)
		{
			IsAlive = true;
			GetComponent<Renderer>().enabled = true;
			GetComponent<Collider>().enabled = true;
		}
	}
	public void GunnerCovered(string _name)
	{
		//Debug.Log("Gunner is Covered by " + _name);
		if(Position == _name)
		{
			IsAlive = true;
			GetComponent<Renderer>().enabled = true;
			GetComponent<Collider>().enabled = true;
		}
	}
	public void SendVitalsMessage() // tells the console current state of crew
	{
		if(IsAlive == true)
		{
			Debug.Log(name + " is alive");
		}
		if(IsAlive == false)
		{
			Debug.Log(name + " was killed");
			Die();
		}
		if(IsCrit == true && IsAlive == true)
		{
			Debug.Log(name + " is in critical condition");
		}
		if(HasRegen == true && IsAlive == true)
		{
			Debug.Log(name + " has recovered from his injuries");
			HasRegen = false;
		}
		if(IsOut == true && IsAlive == true)
		{
			Debug.Log(name + " is unconscious");
			IsAwake = false;
		}
		if(IsAwake == true && IsAlive == true)
		{
			
			IsOut = false;
			
		}
		if(IsCrit == true && currentHP < KOmaxHP && KOLoop == true)
		{
			if(Random.Range(0, 100) < KOchance)
			{
				IsOut = true;
				Debug.Log(name + " passed out");
				if(KOLoop == true) 
				{
					StartCoroutine(PassedOut());
				}
		}
	}
	
	}

	public void Damage(float _damage)
	{
		currentHP -= _damage;
		SendVitalsMessage();
	}
	
	public void Die()
	{
		
		GetComponent<Renderer>().enabled = false;
		GetComponent<Collider>().enabled = false;
		tankController.LossOfCrew(name);
		tankController.CheckKill();
	}
}

}
