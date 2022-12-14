using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Bullets {

public class TankComponentManager : MonoBehaviour
{
	[Header("Tank Settings")]
	public bool Operable;
	[Header("Crew Settings")]
	public int maxCrew; // 4 has no radio operator 5 requires
	public int currentCrew;
	public int minCrew; //Minimum crew before vehicle is knocked out;

	[Header("Crew GO Settings")]
	public GameObject commander;
	public bool commanderImportant;
	public bool commanderAlive;
	public int commanderPriority;
	public GameObject gunner;
	public bool gunnerImportant;
	public bool gunnerAlive;
	public int gunnerPriority;
	public GameObject loader;
	public bool loaderImportant;
	public bool loaderAlive;
	public int loaderPriority;
	public GameObject driver;
	public bool driverImportant;
	public bool driverAlive;
	public int driverPriority;
	public GameObject RadioOperator;
	public bool radioImportant;
	public bool radioAlive;
	public int radioPriority;
		public Ammo[] ammo;
	[Header("Ammo Settings")]
	public bool HasBlowOutPanel;
	public bool cookingOff;
	public bool FatalExplosion;
	
	public GameObject bigFire, bigExplosion;


    // Start is called before the first frame update
    void Start()
    {
        currentCrew = maxCrew;
	if(maxCrew == 5 && RadioOperator == null)
	{
		Debug.Log("Radio Operator position is empty, reverting to 4 person crew");
		currentCrew = 4;
	}
	if(maxCrew < 2)
	{
		Debug.Log("Not enough crew members to operate vehicle");
		Operable = false;
	}
    }

    // Update is called once per frame
    void Update()
    {
        if(minCrew > currentCrew)
	{
		CrewKnockedOut();
		Debug.Log("Disabled tank controller");
		this.enabled = false;
	}
	//move crew around to cover positions
	//Cover gunner
	if(!gunnerAlive)
	{
		if(commanderPriority > loaderPriority)
		{
			gunner.GetComponent<CrewController>().GunnerCovered("Commander");
		}
		if(commanderPriority < loaderPriority)
		{
			gunner.GetComponent<CrewController>().GunnerCovered("Loader");
		}
	}
    }
    public void CheckKill()
	{
        if(minCrew > currentCrew)
	{
		CrewKnockedOut();
	}
	if(currentCrew == 1)
	{
		MissionKill();
	}
	}

    public void RemoveCrewMembers(string _name)
	{
		if(_name == commander.name && commanderAlive)
		{
			currentCrew -= 1;
			commanderAlive = false;
		}
		if(_name == gunner.name && gunnerAlive)
		{
			currentCrew -= 1;
			gunnerAlive = false;
		}
		if(_name == loader.name && loaderAlive)
		{
			currentCrew -= 1;
			loaderAlive = false;
		}
		if(_name == driver.name && driverAlive)
		{
			currentCrew -= 1;
			driverAlive = false;
		}
		if(_name == RadioOperator.name && radioAlive)
		{
			currentCrew -= 1;
			radioAlive = false;
		}
	}
     public void cookOff()
	{
		print(name + " is cooking off");
		if(!HasBlowOutPanel)
		{
			CrewKnockedOut();
		}
	}
     public void BurntOut()
	{
		print(name + " burnt out");
	}
   public void Exploded()
	{
		print(name + " exploded");
	}
   public void FatalExploded()
	{
		print(name + " exploded");
			CrewKnockedOut();
			Instantiate(bigFire, transform.position, transform.rotation);
			Instantiate(bigExplosion, transform.position, transform.rotation);
				for (var i = 0; i < ammo.Length; i++)
				{
					ammo[i].DestroyParticle();
					//Destroy(ammo[i].gameObject);
				}
		Destroy(gameObject, 0.1f);
	}


    public void MissionKill()
	{
		Debug.Log("Vehicle Disabled: Mission Kill");
		Debug.Log(currentCrew + " member's left");
	}
    public void CrewKnockedOut()
	{
		Operable = false;
		Debug.Log("Vehicle " + name + " was lost! Crew Knocked Out");
	}

    public void LossOfCrew(string _name)
	{
		
		if(_name == commander.name)
		{
			Debug.Log("commander died");

			RemoveCrewMembers(commander.name);
		}
		if(_name == gunner.name)
		{
			Debug.Log("gunner died");

			RemoveCrewMembers(gunner.name);
		}
		if(_name == loader.name)
		{
			Debug.Log("loader died");

			RemoveCrewMembers(loader.name);
		}
		if(_name == driver.name)
		{
			Debug.Log("driver died");

			RemoveCrewMembers(driver.name);
		}
		if(_name == RadioOperator.name)
		{
			Debug.Log("RadioOperator died");

			RemoveCrewMembers(RadioOperator.name);
		}
	}
}


}
