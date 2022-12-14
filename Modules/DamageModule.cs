using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bullets {


public class DamageModule : MonoBehaviour
{
	CrewController crew;
	Ammo ammo;
	EngineDamageController engine;

	public bool IsAmmo;
	public bool IsEngine;
	public bool IsCrew;
	public float Health = 100.0f;

    // Start is called before the first frame update
    void Start()
    {
        if(IsAmmo == true)
	{
		ammo = GetComponent<Ammo>();
	}
        if(IsEngine == true)
	{
		engine = GetComponent<EngineDamageController>();
	}
        if(IsCrew == true)
	{
		crew = GetComponent<CrewController>();
	}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void Damage(float _damage)
	{
       if(IsAmmo == true)
	{
		ammo.Damage(_damage);
	}
        if(IsEngine == true)
	{
		engine.TakeDamage(_damage);
	}
        if(IsCrew == true)
	{
		crew.Damage(_damage);
		}
	}
}

}
