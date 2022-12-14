using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bullets {



public class TankBullet : MonoBehaviour {
	
	[Header("Bullet Type Settings")]
	[Tooltip("Penetrator Type: 0: Depleted Uranium, 1: Tungsten, 2: Steel")]
	public int PenetratorType = 0; //0 = Du, 1 = T, 2 = St
	[Tooltip("Bullet Type: 0: Full Caliber AP; 1: Full Caliber APHE; 2: Full Caliber HE; 3: Full Caliber HEAT; 4: Full Caliber APCR; 5: APDS; 6: APFSDS")]
	public int BulletType = 0;
	
	[Header("Bullet Extra Settings")]
	[Tooltip("Stabilizes the bullet when hitting angled armor")]
	public bool BallisticCap = false;
	[Tooltip("Helps improve the areodynamics of the shell")]
	public bool AreodynamicCap = false;
	[Tooltip("Normal shell stabilization factor (how much the angle of impact affects the bullet's penetration) 0.5 = 50 % 1 = 100% and 0 = 0%")]
	public float stabilization;
	[Tooltip("How much the ballistic cap stabilizes the shell: 0.5 = 50 % 1 = 100% and 0 = 0%")]
	public float cabStab;
	[Tooltip("Maximum amount of degrees for shell not to be affected")]
	public float deviateImmuneAngle = 2.0f;
	[Header("Bullet Explosive Settings")]
	[Tooltip("How far the shell traveles before the fuse is set off")]
	public float FuseDelay;
	[Tooltip("How much explosive filler the bullet has")]
	public float ExplosiveAmount;

	[Header("Bullet Collision Settings")]
       public LayerMask BulletIgnore;
       public LayerMask DamageModule;
       public LayerMask Terrain;

	[Header("Bullet Settings")]
	[Tooltip("Caliber in mm")]
       public short Caliber = 100;
	[Tooltip("Max Damage")]
       public short TotalDamage = 1000;
	[Tooltip("Absolute Max Angle the bullet can penetrate")]
       public short AbsMaxAngle = 80;

	[Header("Penetration Formula Variables")]
	[Tooltip("Velocity")]
       public float speed;
	public float StartSpeed;
	[Tooltip("Gravity")]
       public float gravity;
	public float mass;
	[Tooltip("Air density at sea level")]
	public float airDensity;
	[Tooltip("Air pressure at sea level")]
	public float airPressure;
	[Tooltip("Penetrator density in kg/m cubed")]
	public float density;
	[Tooltip("Length of High Density penetrator")]
	public float length;
	public float barrelLength;
	[Tooltip("Brinell hardness number of the shell")]
	public float penetrationBrinell;
	[Tooltip("Frustrum length (windshield cap)")]
	public float Frustrum_Length;
	[Tooltip("Diameter of the very tip of the frustrum cap")]
	public float Frustrum_Diameter;
						
	[Tooltip("Minimum velocity the shell has to be at to be considered super sonic")]
	public float SuperSonicMinVelo;
	[Tooltip("Minimum velocity the shell has to be at to be considered trans sonic")]
	public float TransSonicMinVelo;
	[Tooltip("Maximum velocity the shell has to be at to be considered sub sonic")]
	public float SubSonicMaxVelo;
	public float MinimumVelocityBeforeDestroyed;
	[Header("APFSDS Bullet penetration settings")]
	[Tooltip("Penetration (mm) by distance (meters), NOTE: ONLY USED WITH APFSDS BULLET TYPES OTHERWISE BULLET PENETRATION WILL BE CALCULATED WITH FORMULAS")]
       public AnimationCurve PenetrationByDistance;
	[Tooltip("Maximum penetration for the shell regardless of distance (used with calculated penetration, penetration curves already have a max and a min)")]
	public float MaxPen;
	public float LifeTime;
	[HideInInspector] public float ArmorPenDecrease = 0; //how much penetration the armor removes from the bullet
	[Tooltip("How many mm of pen lost by angle every 100 meters")]
	public float PenetrationDecreasePer100m;
	[HideInInspector]public float airDrag;
	public float DragCoefficient;
	[Tooltip("Start Position")]
       public Vector3 StartPosition;
	[Tooltip("Start Forward")]
       public Vector3 startForward;
	public float DistanceTraveled;
       private bool IsInitialized = false;
       private float StartTime = -1; 
	private float terminalVelo;
	[HideInInspector] public float _time;
	[HideInInspector] public float _seconds;
	[HideInInspector] private float perf;
	[Header("Effects")]
	public GameObject hard_Impact;
	public GameObject ricochet_Metal;
	public GameObject sand_Impact;
	public GameObject dirt_Impact;
	public GameObject soft_Impact;
	public GameObject flesh_Impact;
	public GameObject HE_Explosion_effect;
	public GameObject APHE_Explosion_effect;
	[HideInInspector] private float acceleration, deviation, actualImpactAngle, _AirTime, DeMarrePen, height, gravityAccel, previousArmourPlateThickness;
	[HideInInspector] private bool IsFuseWaiting = false;
	[HideInInspector] private bool Explode = false;
	public float fuseTime;
    [Header("Spall")] 

	[SerializeField]
	private TrailRenderer _trail;
       public int totalSpallRaycasts = 30; 
       public float variance = 1.0f; // This much variance
       public float distance = 3.0f; // at this distance

	[Header("Explosives")]
	
       public int totalFragRaycasts = 30; 
       public float _variance = 1.0f; // This much variance
       public float _distance = 3.0f; // at this distance
	
	[Header("Debug Settings")]
	public bool EnableDebugLogs;
	public bool EnablePrintLogs;

	[Header("Physics Scripts")]
	[Tooltip("The gameObject that holds this script")]
	public ExternalBallisticsController externalBallistics;
	public TemperatureCalculator temperatureCalculator;

	public void updateTimer(float currentTime)
	{
		currentTime += 1.0000f;

		float minutes = Mathf.FloorToInt(currentTime / 60.0000f);
		float seconds = Mathf.FloorToInt(currentTime % 60.0000f);

	 _seconds = currentTime;


	}
	
       public void Initialize(Vector3 StartPoint, Vector3 StartFor, float speed, float gravity)
      { 
		
       	StartPosition = StartPoint; 
      	 	startForward = StartFor;
       	this.speed = speed;
       	this.gravity = gravity; 
		height = gameObject.transform.position.y;
		float r = Frustrum_Diameter / 2;
		float a = Mathf.Pow(Frustrum_Length, 2);
		float b = Mathf.Pow(r, 2);
		float c = r + Mathf.Sqrt(a + b );
		float area = 3.1415f * r* c;
			
		float temperature = temperatureCalculator.temperature;
		
		float _airDensity = BallisticsCalculator.GetAirDensity(gameObject.transform.position.y);
		float _coef = BallisticsCalculator.GetDragCoefficient(_airDensity, area, DragCoefficient, speed);
		DragCoefficient = _coef;
		
		gravityAccel = BallisticsCalculator.GetGravitationalAcceleration(gameObject.transform.position.y);
		 //terminalVelo = Mathf.Sqrt(2*mass*gravity / density * area *DragCoefficient ) * 1000;
		Destroy(this.gameObject, LifeTime);
        	IsInitialized = true;
		
        }

	public void SetPen(float newPen)
	{
		PenetrationByDistance.Evaluate(newPen);
	}
       private Vector3 FindPointOnParobla(float time)
        {
           	Vector3 point = StartPosition + (startForward * speed * time);
           	Vector3 GravityVector = Vector3.down * gravityAccel * time * time; 
          	return point + GravityVector;
        } 

	  private bool CastRayBetweenPoints(Vector3 startPoint, Vector3 endPoint, out RaycastHit hit, LayerMask layerMask) 
	  { 
			return Physics.Raycast(startPoint, endPoint - startPoint, out hit, (endPoint - startPoint).magnitude, layerMask);
	  }
	 private void FixedUpdate() 
	{ 
		height = gameObject.transform.position.y;
		if (!IsInitialized) return; 
		if (StartTime < 0) StartTime = Time.time; 
		if(MinimumVelocityBeforeDestroyed > speed)
		{
			Destroy(gameObject);
		}
		RaycastHit hit;
		 float currentTime = Time.time - StartTime; 
		float nextTime = currentTime + Time.fixedDeltaTime;
		float AirTime = currentTime;
		float dragReduce = 0;
		if(AreodynamicCap == true)
		{
			dragReduce = 20;
		}
		else
		{
			dragReduce = 10;
		}
		if(speed > speed / 1.5) 
		{
		speed -= DragCoefficient * Time.deltaTime / dragReduce;
		}
		else
		{
		
			if(speed > speed / 2) 
			{
			speed -= DragCoefficient * Time.deltaTime/ (dragReduce * 1.5f);
			}
	
		
		}
		
		 Vector3 currentPosition = FindPointOnParobla(currentTime); 
		
		Vector3 nextPoint = FindPointOnParobla(nextTime); transform.position = currentPosition; 
		if (CastRayBetweenPoints(currentPosition, nextPoint, out hit, Terrain))
		{
			if(hit.collider.gameObject.layer == 6)  
			{
					float ImpactAngle = Vector3.Angle(hit.normal, transform.forward); 
					ImpactAngle -= 180;

           			ImpactAngle = Mathf.Abs(ImpactAngle);
					if(EnablePrintLogs) 
					{
						print("Hit terrain @ " + ImpactAngle + " degrees"); 
					}
					float distance = speed * AirTime; 
					if(distance > 1000.0f)
					{
						if(EnablePrintLogs) 
						{
						print("Hit terrain @ "+ AirTime + " seconds and @ " + distance / 1000 + " kilometers"); 
						} 
					}
					else
					{
						if(EnablePrintLogs) 
						{
						print("Hit terrain @ "+ AirTime + " seconds, and " + distance + " meters"); 
						}
					}
					if (ImpactAngle > AbsMaxAngle)
					{
						if (ImpactAngle > 90.0f)
						{
							if(hit.collider.tag == "Metal")
							{
								Instantiate(hard_Impact, hit.point, transform.rotation);

							}
							if(hit.collider.tag == "Dirt")
							{
								Instantiate(dirt_Impact, hit.point, transform.rotation);

								}
							if(hit.collider.tag == "Sand")
							{
								Instantiate(sand_Impact, hit.point, transform.rotation);

							}

						Destroy(gameObject); 
						}
						if(hit.collider.tag == "Metal")
						{
							Instantiate(ricochet_Metal, hit.point, transform.rotation);

						}
						if(hit.collider.tag == "Dirt")
						{
							Instantiate(dirt_Impact, hit.point, transform.rotation);

						}
						if(hit.collider.tag == "Sand")
						{
							Instantiate(sand_Impact, hit.point, transform.rotation);

						}
								speed /= Random.Range( 6, 10.5f);
								Initialize(hit.point, Vector3.Reflect(transform.forward, hit.normal), speed, 9.8f);
				}
				else
				{
					if(hit.collider.tag == "Metal")
					{
						Instantiate(hard_Impact, hit.point, transform.rotation);

					}
					if(hit.collider.tag == "Dirt")
					{
						Instantiate(dirt_Impact, hit.point, transform.rotation);

					}
					if(hit.collider.tag == "Sand")
					{
						Instantiate(sand_Impact, hit.point, transform.rotation);

					}

					Destroy(gameObject); 
				}

			}
		}
		if (CastRayBetweenPoints(currentPosition, nextPoint, out hit, BulletIgnore))
		 { //do Penetration calculation 
				if(EnablePrintLogs) {
				print("Hit" + hit.collider.name + " @ "+ AirTime + " seconds"); }
				float distance = speed * AirTime; 
				if(EnablePrintLogs) {
				print(name + "traveled " + distance + "m"); }
				if(hit.collider.tag == "Metal")
				{
					Instantiate(hard_Impact, hit.point, transform.rotation);

				}
				//Check If is armour 
				if(hit.collider.gameObject.layer == 3) 
				{ 
					//Do Calculations 
					Armour plate = hit.collider.GetComponent<Armour>(); 
					
					//Get Angle of Attack
					float ImpactAngle = Vector3.Angle(hit.normal, transform.forward); 
					ImpactAngle -= 180;
           				ImpactAngle = Mathf.Abs(ImpactAngle);
					
					if(ImpactAngle > deviateImmuneAngle)
					{
						float _stabilizationFactor = ImpactAngle * stabilization;
						 
						float impactBeforeDev = ImpactAngle;
						
						if(BallisticCap == true)
						{
							float _newFactor = _stabilizationFactor * cabStab;
							deviation = Random.Range(-_newFactor, -_newFactor / 5.0f);
							ImpactAngle += deviation;
							if(EnableDebugLogs) {
							Debug.Log("Shell stabilization and deviation factor is " + deviation); }
							actualImpactAngle = impactBeforeDev;
						}
						else
						{
							deviation = Random.Range(_stabilizationFactor / 5.0f, _stabilizationFactor);
							ImpactAngle += deviation;
							if(EnableDebugLogs) {
							Debug.Log("Shell deviation factor is " + deviation); }
							actualImpactAngle = impactBeforeDev;
						}
					}
					//Check LengthLineOfSight 
					float Llos = 0;
					Llos =  plate.Thickness / Mathf.Cos(ImpactAngle);
            		Llos = Mathf.Abs(Llos);
					
					
					
					if(BulletType == 6) // Gets apfsds calculations
					{
						float perforationlimit = BallisticsCalculator.PerforationLimit(length, Caliber, Frustrum_Length, Frustrum_Diameter, density, penetrationBrinell, speed, plate.density, plate.BrinellNumber, ImpactAngle, PenetratorType);
						

						//float DeMarrePen = BallisticsCalculator.Soviet_Demarre(plate.Thickness, mass, speed, Caliber, ImpactAngle);
						if(perforationlimit < 0.0f)
						{
							perforationlimit = 0.0f;
						}

						//Make the perforation limit into a more realistic value
						float perfLimit = Mathf.Abs(perforationlimit - plate.Thickness);
						perforationlimit = perfLimit;
							if(EnableDebugLogs) { Debug.Log(perfLimit); }


							if(EnableDebugLogs) {
							Debug.Log($"Hit at angle" + actualImpactAngle +  "+ " + deviation + "= " + ImpactAngle + ", Bullet has to travel through" +  Llos + ", shell perforated " + perforationlimit + "mm, shell velocity is " + speed);  }
							//Check for pen 

							if(EnableDebugLogs) {
							Debug.Log($"Can Pen {PenetrationByDistance.Evaluate(distance)}mm of Armour");  }
						if(perforationlimit > Llos)
						{
							if(EnableDebugLogs) {
							Debug.Log($" shell has perforated " + Llos + "mm of armor"); }
						
							PerforationSpall(hit.point, Llos);
						}
						if (Caliber > plate.Thickness * 3 || length > plate.Thickness * 3) 
						{ 
							if(EnableDebugLogs) {
							Debug.Log($"Has Pend {Llos}mm of Armour"); }
							if(length > plate.Thickness * 3)
							{
								length -= plate.Thickness; //makes it so that u can just abuse APFSDS rod lengths
							}
							//has pend 
							Spall(hit.point, Llos);

					 	} 
						else if (ImpactAngle > AbsMaxAngle)
					 	{ 
							if(perforationlimit > Llos)
							{
								if(EnableDebugLogs) {
								Debug.Log($" shell has perforated  " + Llos + "mm of angled armor"); }
								TailSpall(hit.point, Llos, totalSpallRaycasts / 2);
								PerforationSpall(hit.point, Llos);
							}
							else
							{
								if(EnableDebugLogs) {
								Debug.Log($"Has ricocheted off {Llos}mm of Armour"); } //a ricochet has occured. }
								Instantiate(ricochet_Metal, hit.point, transform.rotation);
								speed /= 6.5f;
								Initialize(hit.point, Vector3.Reflect(transform.forward, hit.normal), speed, 9.8f);
							} 
						} 
						else if(PenetrationByDistance.Evaluate( + Llos / 1000) + perforationlimit > Llos ) 
						{ 
							//Debug.Log($"Has Pend {Llos}mm of straight  Armour"); //has pend 
							//Calculate the penetration for distance and angle
							float PenetrationCapability = PenetrationByDistance.Evaluate(distance) + perforationlimit;
							float DistanceTraveledIn100m = distance / 100; //checks for every 100 meters
							float angleDecrease = DistanceTraveledIn100m * Mathf.Pow(PenetrationDecreasePer100m, 2); //removes penetration based on distance and angle
							PenetrationCapability -= angleDecrease;
							PenetrationCapability = Mathf.Abs(PenetrationCapability);
							if(EnableDebugLogs) {
							Debug.Log($"has " + PenetrationCapability + "mm of pen at this distance and angle"); }
							if(PenetrationCapability > Llos || perforationlimit > Llos)
							{
								if(EnableDebugLogs) {
								Debug.Log($"Has Pend {Llos}mm of  Armour"); }//has pend	
								speed -= plate.ArmourDencityCoefficent;	
								if(speed < MinimumVelocityBeforeDestroyed)
								{
									Destroy(gameObject);
								}				
								else {	
								Spall(hit.point, Llos); 
								PerforationSpall(hit.point, Llos); }
							}
							else if(PenetrationCapability < Llos)
					       	{ 
								if(EnableDebugLogs) {
								Debug.Log($"Has NOT Pend {Llos}mm of Armour"); }//has not pend 
								Destroy(transform.gameObject);
					       	}

						} 
					//else 
					// { 
					//	Debug.Log($"Has NOT Pend {Llos}mm of Armour"); //has not pend 
					//	Destroy(transform.gameObject);
					// }
					 }  //APFSDS End
					if(BulletType == 5 || BulletType == 4) // Gets APDS / CR calculations
					{
						float Patel_Gillingham = BallisticsCalculator.Patel_Gillingham(speed, mass, Caliber, plate.UTS);
						if(Patel_Gillingham > MaxPen)
						{
								Patel_Gillingham = MaxPen;
						}
							if(EnableDebugLogs) {
							Debug.Log($"Hit at angle" + actualImpactAngle +  "+ " + deviation + "= " + ImpactAngle + ", Bullet has to travel through" +  Llos + ", shell velocity is " + speed); }
							//Check for pen 

							if(EnableDebugLogs) {
							Debug.Log($"Can Pen {Patel_Gillingham}mm of Armour"); }
						if(Patel_Gillingham > Llos)
						{
							if(EnableDebugLogs) {
							Debug.Log($" shell has perforated " + Llos + "mm of armor"); }
						
							PerforationSpall(hit.point, Llos);
						}
						if (Caliber > plate.Thickness * 3) 
						{ 
							if(EnableDebugLogs) {
							Debug.Log($"Has Pend {Llos}mm of Armour"); }
								speed -= plate.ArmourDencityCoefficent;	
								if(speed < MinimumVelocityBeforeDestroyed)
								{
									Destroy(gameObject);
								}			
								else {		
							//has pend 
							Spall(hit.point, Llos); }

					 	} 
						else if (ImpactAngle > AbsMaxAngle)
					 	{ 
							if(Patel_Gillingham > Llos)
							{
								if(EnableDebugLogs) {
								Debug.Log($" shell has perforated  " + Llos + "mm of angled armor"); }
								TailSpall(hit.point, Llos, totalSpallRaycasts / 2);
								PerforationSpall(hit.point, Llos);
							}
							else
							{
								if(EnableDebugLogs) {
								Debug.Log($"Has ricocheted off {Llos}mm of Armour"); }//a ricochet has occured. 
								Instantiate(ricochet_Metal, hit.point, transform.rotation);
								Initialize(hit.point, Vector3.Reflect(transform.forward, hit.normal), speed / 10, 9.8f);
							} 
						} 
						else if(PenetrationByDistance.Evaluate(distance + Llos / 1000) > Llos ) 
						{ 
							//Debug.Log($"Has Pend {Llos}mm of straight  Armour"); //has pend 
							//Calculate the penetration for distance and angle
							float PenetrationCapability = Patel_Gillingham;
							float DistanceTraveledIn100m = distance / 100; //checks for every 100 meters
							float angleDecrease = DistanceTraveledIn100m * Mathf.Pow(PenetrationDecreasePer100m, 2); //removes penetration based on distance and angle
							PenetrationCapability -= angleDecrease;
							if(PenetrationCapability < 0.0f)
							{
								PenetrationCapability = 0f;
								if(EnableDebugLogs) {
								Debug.Log($"has no penetration at this distance and angle because of drag");		}						
							}
							else
							{
							if(EnableDebugLogs) {
							Debug.Log($"has " + PenetrationCapability + "mm of pen at this distance and angle"); }
							}
							if(PenetrationCapability > Llos || Patel_Gillingham > Llos)
							{
								if(EnableDebugLogs) {
								Debug.Log($"Has Pend {Llos}mm of  Armour"); }//has pend	
								speed -= plate.ArmourDencityCoefficent;	
								if(speed < MinimumVelocityBeforeDestroyed)
								{
									Destroy(gameObject);
								}					
								else {
								Spall(hit.point, Llos); 
								PerforationSpall(hit.point, Llos);
								}
							}
							else if(Patel_Gillingham < Llos)
					       	{ 
								if(EnableDebugLogs) {
								Debug.Log($"Has NOT Pend {Llos}mm of Armour"); } //has not pend 
								Destroy(transform.gameObject);
					       	}

						} 
					else 
					{ 
						if(EnableDebugLogs) {
						Debug.Log($"Has NOT Pend {Llos}mm of Armour"); }//has not pend 
						Destroy(transform.gameObject);
					}
					 }  //APDS / CREnd

					if(BulletType == 3) // Gets HEAT calculations
					{


						//float DeMarrePen = BallisticsCalculator.Soviet_Demarre(plate.Thickness, mass, speed, Caliber, ImpactAngle);		
							if(EnableDebugLogs) {
							Debug.Log($"Hit at angle" + actualImpactAngle +  "+ " + deviation + "= " + ImpactAngle + ", Bullet has to travel through" +  Llos + ", mm, shell velocity is " + speed); }
							//Check for pen 

						if(EnableDebugLogs) {
						Debug.Log($"Can Pen {PenetrationByDistance.Evaluate(distance)}mm of Armour"); }
						
						if (Caliber > plate.Thickness * 3) 
						{ 
							if(EnableDebugLogs) {
							Debug.Log($"Has Pend {Llos}mm of Armour"); }
							//has pend 
								Instantiate(HE_Explosion_effect, hit.point, Quaternion.identity);
							Spall(hit.point, Llos);
								Destroy(transform.gameObject);

					 	} 
						else if (ImpactAngle > AbsMaxAngle)
					 	{ 
							if(EnableDebugLogs) {
								Debug.Log($"Has ricocheted off {Llos}mm of Armour"); }//a ricochet has occured. 
								Instantiate(ricochet_Metal, hit.point, transform.rotation);
								Initialize(hit.point, Vector3.Reflect(transform.forward, hit.normal), speed / 10, 9.8f);
						
						} 
						else if(PenetrationByDistance.Evaluate(distance) > Llos ) 
						{ 
							//Debug.Log($"Has Pend {Llos}mm of straight  Armour"); //has pend 
							//Calculate the penetration for distance and angle
							float PenetrationCapability = PenetrationByDistance.Evaluate(distance);
							if(EnableDebugLogs) {
							Debug.Log($"has " + PenetrationCapability + "mm of pen at this distance and angle"); }
							if(PenetrationCapability > Llos)
							{
								if(EnableDebugLogs) {
								Debug.Log($"Has Pend {Llos}mm of  Armour"); }//has pend							
								Spall(hit.point, Llos); 
								PerforationSpall(hit.point, Llos);
								Instantiate(HE_Explosion_effect, hit.point, Quaternion.identity);
								Destroy(transform.gameObject);
							}
							else if(PenetrationCapability < Llos)
					       	{ 
								if(EnableDebugLogs) {
								Debug.Log($"Has NOT Pend {Llos}mm of Armour"); }//has not pend 
								Destroy(transform.gameObject);
					       	}

						} 
					//else 
					// { 
					//	Debug.Log($"Has NOT Pend {Llos}mm of Armour"); //has not pend 
					//	Destroy(transform.gameObject);
					// }
					 }  //HEAT End
					if(BulletType == 2) // Gets HE calculations
					{

						Llos = plate.Thickness;
						//float DeMarrePen = BallisticsCalculator.Soviet_Demarre(plate.Thickness, mass, speed, Caliber, ImpactAngle);		
							if(EnableDebugLogs) {
							Debug.Log($"Hit at angle" + actualImpactAngle +  "+ " + deviation + "= " + ImpactAngle + ", Bullet has to travel through" +  Llos + ", mm, shell velocity is " + speed); }
							//Check for pen 

						if(EnableDebugLogs) {
						Debug.Log($"Can Pen {PenetrationByDistance.Evaluate(distance)}mm of Armour"); }
						
						if (Caliber > plate.Thickness * 3) 
						{ 
							if(EnableDebugLogs) {
							Debug.Log($"Has Pend {Llos}mm of Armour"); }
							//has pend 
								Instantiate(HE_Explosion_effect, hit.point, Quaternion.identity);
							Spall(hit.point, Llos);
								Destroy(transform.gameObject);

					 	} 
						
						else if(PenetrationByDistance.Evaluate(distance) > Llos ) 
						{ 
							//Debug.Log($"Has Pend {Llos}mm of straight  Armour"); //has pend 
							//Calculate the penetration for distance and angle
							float PenetrationCapability = PenetrationByDistance.Evaluate(distance);
							if(EnableDebugLogs) {
							Debug.Log($"has " + PenetrationCapability + "mm of pen at this distance and angle"); }
							if(PenetrationCapability > Llos)
							{
								if(EnableDebugLogs) {
								Debug.Log($"Has Pend {Llos}mm of  Armour");} //has pend	}						
								Spall(hit.point, Llos); 
								PerforationSpall(hit.point, Llos);
								Instantiate(HE_Explosion_effect, hit.point, Quaternion.identity);
								Destroy(transform.gameObject);
							}
							else if(PenetrationCapability < Llos)
					       	{ 
								if(EnableDebugLogs) {
								Debug.Log($"Has NOT Pend {Llos}mm of Armour"); } //has not pend 
								Destroy(transform.gameObject);
					       	}

						} 
					//else 
					// { 
					//	Debug.Log($"Has NOT Pend {Llos}mm of Armour"); //has not pend 
					//	Destroy(transform.gameObject);
					// }
					 }  //HE End
					if(BulletType == 1) // Gets APHE calculations
					{
						float Thickness = plate.Thickness;
						if(EnableDebugLogs) {
						Debug.Log("Formula: Thickness : " + Thickness + " Mass: " + mass + " speed: " + speed + " caliber: " + Caliber + " impact angle: " + ImpactAngle); }
						float SonicInt = BallisticsCalculator.CalculateSonic(speed, SuperSonicMinVelo, TransSonicMinVelo, SubSonicMaxVelo);

						if(SonicInt == 2)
						{		
									if(EnablePrintLogs) {
								print("Shell has impacted the target with a supersonic velocity, using DeMarre's formula..."); }
							 DeMarrePen = BallisticsCalculator.calculateDemarre(MaxPen, Caliber, mass, StartSpeed, Caliber, mass, speed);
						}
						if(SonicInt == 1)
						{
								if(EnablePrintLogs) {
								print("Shell has impacted the target with in a trans-sonic velocity, getting average between DeMarre and Krupp formulas to calculate penetration..."); }
								float Krupp = BallisticsCalculator.calculateKrupp(Caliber, mass, speed);
								float Marre =  BallisticsCalculator.calculateDemarre(MaxPen, Caliber, mass, StartSpeed, Caliber, mass, speed);
								float a = Krupp + Marre;
								float newPen = a / 2;
								if(EnablePrintLogs) {
								print("Trans-sonic penetration value is " + newPen + " mm's of penetration"); }
						 DeMarrePen = newPen;
						}
						if(SonicInt == 0)
						{
								if(EnablePrintLogs) {
								print("Shell has impacted the target with a subsonic velocity, using Krupp's formula..."); }
								float Krupp = BallisticsCalculator.calculateKrupp(Caliber, mass, speed);
								
						 DeMarrePen = Krupp;
						}
					if(SonicInt == 3)
						{
								if(EnablePrintLogs) {
								print("Shell has impacted the target between a trans-sonic and sub sonic velocity, getting average between DeMarre and Krupp formulas to calculate penetration..."); }
								float Krupp = BallisticsCalculator.calculateKrupp(Caliber, mass, speed);
								float Marre =  BallisticsCalculator.calculateDemarre(MaxPen, Caliber, mass, StartSpeed, Caliber, mass, speed);
								float a = Krupp + Marre;
								float newPen = a / 2;
								if(EnablePrintLogs) {
								print("Trans/Sub-sonic penetration value is " + newPen + " mm's of penetration"); }
								
						 DeMarrePen = newPen;
						}
						
						float CurvePen = PenetrationByDistance.Evaluate(distance);
						if(DeMarrePen > MaxPen)
						{
							DeMarrePen = MaxPen;
						}
							if(EnableDebugLogs) {
							Debug.Log($"Hit at angle" + actualImpactAngle +  "+ " + deviation + "= " + ImpactAngle + ", Bullet has to travel through" +  Llos + ", shell velocity is " + speed); }
							//Check for pen 


						//Debug.Log($"Can Pen {PenetrationByDistance.Evaluate(distance)}mm of Armour"); 
						if(EnableDebugLogs) {
						Debug.Log($"Penetration formula declares {DeMarrePen}mm of Armour penetration"); }
					
						if (Caliber > plate.Thickness * 3) 
						{ 
							if(EnableDebugLogs) {
							Debug.Log($"Has Pend {Llos}mm of Armour"); }
								speed -= plate.ArmourDencityCoefficent;	
								if(speed < MinimumVelocityBeforeDestroyed)
								{
									Destroy(gameObject);
								}			
								else {		
							//has pend 
							Spall(hit.point, Llos); }

					 	} 
						else if (ImpactAngle > AbsMaxAngle)
					 	{ 
								if(EnableDebugLogs) {
								Debug.Log($"Has ricocheted off {Llos}mm of Armour"); }//a ricochet has occured. 
								Instantiate(ricochet_Metal, hit.point, transform.rotation);
								Initialize(hit.point, Vector3.Reflect(transform.forward, hit.normal), speed / 10, 9.8f);
					
						} 
						else if(DeMarrePen > Llos ) 
						{ 
							//Debug.Log($"Has Pend {Llos}mm of straight  Armour"); //has pend 
							//Calculate the penetration for distance and angle
							float PenetrationCapability = PenetrationByDistance.Evaluate(distance);
							float deMarrePenCap = DeMarrePen;
							if(deMarrePenCap == PenetrationCapability)
							{
								PenetrationCapability = DeMarrePen;
							}
							float DistanceTraveledIn100m = distance / 100; //checks for every 100 meters
							float angleDecrease = DistanceTraveledIn100m * Mathf.Pow(PenetrationDecreasePer100m, 2); //removes penetration based on distance and angle
							DeMarrePen -= angleDecrease;
							if(DeMarrePen < 0.0f)
							{
								DeMarrePen = 0f;
								if(EnableDebugLogs) {
								Debug.Log($"has no penetration at this distance and angle because of drag");		}						
							}
							else
							{
							if(EnableDebugLogs) {
							Debug.Log($"has " + DeMarrePen + "mm of pen at this distance and angle"); }
							}
							if( DeMarrePen > Llos)
							{
								if(EnableDebugLogs) {
								Debug.Log($"Has Pend {Llos}mm of  Armour"); }//has pend							
								Spall(hit.point, Llos); 
	
									if(EnableDebugLogs) {
									Debug.Log("Shell exploded"); }
									Vector3 apheExplosion = new Vector3(transform.position.x, transform.position.y, transform.position.z + FuseDelay);
									GameObject explosion = Instantiate(APHE_Explosion_effect, apheExplosion, Quaternion.identity) as GameObject;		
									APHE_Fragments(apheExplosion, Llos, 20);
									Destroy(this.gameObject);
						

							}
							else if(DeMarrePen < Llos)
					       	{ 
								if(EnableDebugLogs) {
								Debug.Log($"Has NOT Pend {Llos}mm of Armour"); }//has not pend 
								Destroy(transform.gameObject);
					       	}

						} 
					else 
					{ 
						if(EnableDebugLogs) {
						Debug.Log($"Has NOT Pend {Llos}mm of Armour"); }//has not pend 
						Destroy(transform.gameObject);
					}
					 }  //APDS / CREnd
					if(BulletType == 0) // Gets AP calculations
					{
						float Thickness = plate.Thickness;
						if(EnableDebugLogs) {
						Debug.Log("Formula: Thickness : " + Thickness + " Mass: " + mass + " speed: " + speed + " caliber: " + Caliber + " impact angle: " + ImpactAngle); }
						float SonicInt = BallisticsCalculator.CalculateSonic(speed, SuperSonicMinVelo, TransSonicMinVelo, SubSonicMaxVelo);

						if(SonicInt == 2)
						{		
									if(EnablePrintLogs) {
								print("Shell has impacted the target with a supersonic velocity, using DeMarre's formula..."); }
							 DeMarrePen = BallisticsCalculator.calculateDemarre(MaxPen, Caliber, mass, StartSpeed, Caliber, mass, speed);
						}
						if(SonicInt == 1)
						{
								if(EnablePrintLogs) {
								print("Shell has impacted the target with in a trans-sonic velocity, getting average between DeMarre and Krupp formulas to calculate penetration..."); }
								float Krupp = BallisticsCalculator.calculateKrupp(Caliber, mass, speed);
								float Marre =  BallisticsCalculator.calculateDemarre(MaxPen, Caliber, mass, StartSpeed, Caliber, mass, speed);
								float a = Krupp + Marre;
								float newPen = a / 2;
								if(EnablePrintLogs) {
								print("Trans-sonic penetration value is " + newPen + " mm's of penetration"); }
						 DeMarrePen = newPen;
						}
						if(SonicInt == 0)
						{
								if(EnablePrintLogs) {
								print("Shell has impacted the target with a subsonic velocity, using Krupp's formula..."); }
								float Krupp = BallisticsCalculator.calculateKrupp(Caliber, mass, speed);
								
						 DeMarrePen = Krupp;
						}
					if(SonicInt == 3)
						{
								if(EnablePrintLogs) {
								print("Shell has impacted the target between a trans-sonic and sub sonic velocity, getting average between DeMarre and Krupp formulas to calculate penetration..."); }
								float Krupp = BallisticsCalculator.calculateKrupp(Caliber, mass, speed);
								float Marre =  BallisticsCalculator.calculateDemarre(MaxPen, Caliber, mass, StartSpeed, Caliber, mass, speed);
								float a = Krupp + Marre;
								float newPen = a / 2;
								if(EnablePrintLogs) {
								print("Trans/Sub-sonic penetration value is " + newPen + " mm's of penetration"); }
								
						 DeMarrePen = newPen;
						}
						
						float CurvePen = PenetrationByDistance.Evaluate(distance);
						if(DeMarrePen > MaxPen)
						{
							DeMarrePen = MaxPen;
						}
							if(EnableDebugLogs) {
							Debug.Log($"Hit at angle" + actualImpactAngle +  "+ " + deviation + "= " + ImpactAngle + ", Bullet has to travel through" +  Llos + ", shell velocity is " + speed); }
							//Check for pen 


						//Debug.Log($"Can Pen {PenetrationByDistance.Evaluate(distance)}mm of Armour"); 
						if(EnableDebugLogs) {
						Debug.Log($"Penetration formula declares {DeMarrePen}mm of Armour penetration"); }
					
						if (Caliber > plate.Thickness * 3) 
						{ 
							if(EnableDebugLogs ) {
							Debug.Log($"Has Pend {Llos}mm of Armour"); }
								speed -= plate.ArmourDencityCoefficent;	
								if(speed < MinimumVelocityBeforeDestroyed)
								{
									Destroy(gameObject);
								}					
							//has pend 
							Spall(hit.point, Llos);

					 	} 
						else if (ImpactAngle > AbsMaxAngle)
					 	{ 
								if(EnableDebugLogs) {
								Debug.Log($"Has ricocheted off {Llos}mm of Armour"); }//a ricochet has occured. 
								Instantiate(ricochet_Metal, hit.point, transform.rotation);
								Initialize(hit.point, Vector3.Reflect(transform.forward, hit.normal), speed / 10, 9.8f);
					
						} 
						else if(DeMarrePen > Llos ) 
						{ 
							//Debug.Log($"Has Pend {Llos}mm of straight  Armour"); //has pend 
							//Calculate the penetration for distance and angle
							float PenetrationCapability = PenetrationByDistance.Evaluate(distance);
							float deMarrePenCap = DeMarrePen;
							if(deMarrePenCap == PenetrationCapability)
							{
								PenetrationCapability = DeMarrePen;
							}
							float DistanceTraveledIn100m = distance / 100; //checks for every 100 meters
							float angleDecrease = DistanceTraveledIn100m * Mathf.Pow(PenetrationDecreasePer100m, 2); //removes penetration based on distance and angle
							DeMarrePen -= angleDecrease;
							if(DeMarrePen < 0.0f)
							{
								DeMarrePen = 0f;
								if(EnableDebugLogs) {
								Debug.Log($"has no penetration at this distance and angle because of drag");		}						
							}
							else
							{
							if(EnableDebugLogs) {
							Debug.Log($"has " + DeMarrePen + "mm of pen at this distance and angle"); }
							}
							if( DeMarrePen > Llos)
							{
								if(EnableDebugLogs) {
								Debug.Log($"Has Pend {Llos}mm of  Armour"); }//has pend	
								speed -= plate.ArmourDencityCoefficent;	
								if(speed < MinimumVelocityBeforeDestroyed)
								{
									Destroy(gameObject);
								}	
								else
								{										
								Spall(hit.point, Llos); 
								}
					

							}
							else if(DeMarrePen < Llos)
					       	{ 
								if(EnableDebugLogs) {
								Debug.Log($"Has NOT Pend {Llos}mm of Armour"); }//has not pend 
								Destroy(transform.gameObject);
					       	}

						} 
					else 
					{ 
						if(EnableDebugLogs) {
						Debug.Log($"Has NOT Pend {Llos}mm of Armour"); }//has not pend 
						Destroy(transform.gameObject);
					}
					 }  //APDS / CREnd

				} // collider end
				if (hit.collider.gameObject.layer == 7)
				 { 
					if(EnableDebugLogs) {
					Debug.Log($"has hit the '{hit.collider.name}' damage module."); }
					 DamageModule module = hit.collider.GetComponent<DamageModule>(); 
						float _damage = module.Health;
					if ((_damage - TotalDamage) <= 0) 
					{ 

					
						_damage -= TotalDamage; 
						module.Damage(_damage);
						if(EnableDebugLogs) {
						Debug.Log($"The Module '{hit.collider.name}' has been destroyed."); }
					} 
					else 
					{
						
						_damage -= TotalDamage;
						module.Damage(_damage);
					 } 
				} 
				
			
			 } 



	void Update() 
	{ 
		if (!IsInitialized || StartTime < 0) return;
		 float currentTime = Time.time - StartTime; 
		Vector3 currentPosition = FindPointOnParobla(currentTime);
		 transform.position = currentPosition;
		
					
		}
		
	} 
	  


	private Vector3 GetDirection()
	{
			Vector3 direction = transform.forward;
			return direction;
	}


	private IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit, Vector3 _point)
	{
		float time = 0;
		Vector3 startPos = trail.transform.position;
		
		while (time < 1)
		{
			trail.transform.position = Vector3.Lerp(startPos, hit.point, time);
			time += Time.deltaTime / trail.time;
		
			yield return null;
		}
		trail.transform.position = hit.point;

		Destroy(trail.gameObject, trail.time);
		
	}

	void Spall(Vector3 point, float pen)
	 { //7
		for (var i = 0; i < totalSpallRaycasts; i++)
		 { //6
			Vector3 v3Offset = transform.up * Random.Range(0.0f, variance);
			 v3Offset = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), transform.forward) * v3Offset; 
			Vector3 v3Hit = transform.forward * distance + v3Offset; 
			RaycastHit hit; 
			Debug.DrawLine(this.transform.position, this.transform.position + v3Hit);
			if(Physics.Raycast(point, v3Hit, out hit, distance, DamageModule)) 
			{ //5
			
				if(hit.collider.gameObject.layer == 3)
				{//4
					if(EnableDebugLogs) {
					Debug.Log($"has hit a armor collider"); }
				}//4
				if(hit.collider.gameObject.layer == 7) 
				{//3
					if(EnableDebugLogs) {
					 Debug.Log($"has hit the '{hit.collider.name}' damage module"); }
					DamageModule module = hit.collider.GetComponent<DamageModule>();
					float _damage = module.Health;
					 if((_damage - TotalDamage / totalSpallRaycasts) <= 0) 
					{//2
						
					
						_damage -= TotalDamage / totalSpallRaycasts; 
						module.Damage(_damage);
						if(EnableDebugLogs) {
						Debug.Log($"The Module '{hit.collider.name}' has been destroyed."); }
					} //2
					else 
					{ //1
						
						_damage -= TotalDamage / totalSpallRaycasts;
						module.Damage(_damage);
					 } //1
				}//3
			 } //5
		}//6
	 } //7
	void PerforationSpall(Vector3 point, float pen)
	 { //7
		for (var i = 0; i < totalSpallRaycasts; i++)
		 { //6
			Vector3 v3Offset = transform.up * Random.Range(0.0f, variance * 2);
			 v3Offset = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), transform.forward) * v3Offset; 
			Vector3 v3Hit = transform.forward * distance / 2 + v3Offset; 
			RaycastHit hit; 
			Debug.DrawLine(this.transform.position, this.transform.position + v3Hit, Color.green);
			if(Physics.Raycast(point, v3Hit, out hit, distance, DamageModule)) 
			{ //5
			
				if(hit.collider.gameObject.layer == 3)
				{//4
					if(EnableDebugLogs) {
					Debug.Log($"has hit a armor collider"); }
				}//4
				if(hit.collider.gameObject.layer == 7) 
				{//3
					if(EnableDebugLogs) {
					 Debug.Log($"has hit the '{hit.collider.name}' damage module");  }
					DamageModule module = hit.collider.GetComponent<DamageModule>();
					float _damage = module.Health;
					 if((_damage - TotalDamage / totalSpallRaycasts) <= 0) 
					{//2
						
					
						_damage -= TotalDamage / totalSpallRaycasts; 
						module.Damage(_damage);
						if(EnableDebugLogs) {
						Debug.Log($"The Module '{hit.collider.name}' has been destroyed."); }
					} //2
					else 
					{ //1
						
						_damage -= TotalDamage / totalSpallRaycasts;
						module.Damage(_damage);
					 } //1
				}//3
			 } //5
		}//6
	 } //7
	void TailSpall(Vector3 point, float pen, int SpallRaycasts)
	 { //7
		for (var i = 0; i < SpallRaycasts; i++)
		 { //6
			Vector3 v3Offset = transform.up * Random.Range(0.0f, variance);
			 v3Offset = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), transform.forward) * v3Offset; 
			Vector3 v3Hit = transform.forward * distance + v3Offset; 
			RaycastHit hit; 
			Debug.DrawLine(this.transform.position, this.transform.position + v3Hit, Color.cyan);
			if(Physics.Raycast(point, v3Hit, out hit, distance, DamageModule)) 
			{ //5
			
				if(hit.collider.gameObject.layer == 3)
				{//4
					if(EnableDebugLogs) {
					Debug.Log($"has hit a armor collider"); }
				}//4
				if(hit.collider.gameObject.layer == 7) 
				{//3
					if(EnableDebugLogs) {
					 Debug.Log($"has hit the '{hit.collider.name}' damage module"); }
					DamageModule module = hit.collider.GetComponent<DamageModule>();
					float _damage = module.Health;
					 if((_damage - TotalDamage / SpallRaycasts) <= 0) 
					{//2
						
					
						_damage -= TotalDamage / SpallRaycasts; 
						module.Damage(_damage);
						if(EnableDebugLogs) {
						Debug.Log($"The Module '{hit.collider.name}' has been destroyed."); }
					} //2
					else 
					{ //1
						
						_damage -= TotalDamage / SpallRaycasts;
						module.Damage(_damage);
					 } //1
				}//3
			 } //5
		}//6
	 } //7
	void APHE_Fragments(Vector3 point, float pen, int SpallRaycasts)
	 { //7
		for (var i = 0; i < SpallRaycasts; i++)
		 { //6
			Vector3 v3Offset = transform.up * Random.Range(0.0f, variance * 12);
			 v3Offset = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), transform.forward) * v3Offset; 
			Vector3 v3Hit = transform.forward * distance / 5 + v3Offset; 
			RaycastHit hit; 
			Vector3 fuseDelay = new Vector3(transform.position.x, transform.position.y, transform.position.z + FuseDelay);
			Debug.DrawLine(point,point + fuseDelay, Color.cyan);
			if(Physics.Raycast(point, v3Hit, out hit, distance / 5, DamageModule)) 
			{ //5
			
				if(hit.collider.gameObject.layer == 3)
				{//4
					if(EnableDebugLogs) {
					Debug.Log($"has hit a armor collider"); }
				}//4
				if(hit.collider.gameObject.layer == 7) 
				{//3
					if(EnableDebugLogs) {
					 Debug.Log($"has hit the '{hit.collider.name}' damage module");  }
					DamageModule module = hit.collider.GetComponent<DamageModule>();
					float _damage = module.Health;
					 if((_damage - TotalDamage / SpallRaycasts) <= 0) 
					{//2
						
					
						_damage -= TotalDamage / SpallRaycasts; 
						module.Damage(_damage);
						if(EnableDebugLogs) {
						Debug.Log($"The Module '{hit.collider.name}' has been destroyed."); }
					} //2
					else 
					{ //1
						
						_damage -= TotalDamage / SpallRaycasts;
						module.Damage(_damage);
					 } //1
				}//3
			 } //5
		}//6
	 } //7
}


}


