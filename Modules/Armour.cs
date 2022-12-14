using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Armour : MonoBehaviour
{

	[Tooltip("Thickness in mm")]
	public float Thickness = 75;
	public float ArmourDencityCoefficent = 1f;

	[Tooltip("Density in kg/m cubed")]
	public float density = 7850.0f;
	public float BrinellNumber = 237.0f;
	[Tooltip("High Hardness Armor (HHA): 1640 MPa Rolled Homogenous Armor (RHA): 1170 MPa Ti6AlV Armor: 970 MPa")]
	public float UTS = 1170;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
