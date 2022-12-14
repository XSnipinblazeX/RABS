using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Bullets {


public static class BallisticsCalculator 
{

	public static float perf;
	private static float Sonic;
    public static  float airPressure = 101325.0f; // default value for sea level on Earth
    public static float airDensity = 1.225f; // default value for sea level on Earth
    public static float humidity = 0.5f; // default value for 50% relative humidity




	public static float GetAirPressure(float altitude)
	{
        // calculate the altitude of the target object
        
		// calculate the temperature in Celsius at the target object's altitude
		float temperatureCelsius = 15.0f - 0.0065f * altitude;
		// convert the temperature to Kelvin
		float temperatureKelvin = temperatureCelsius + 273.15f;
        // calculate the air pressure at the target object's altitude
        // this is a simplified equation that only takes into account the altitude and a standard atmosphere model
        // for a more accurate calculation, other factors such as temperature and humidity would also need to be considered
        float _airPressure = airPressure * Mathf.Pow((1 - 2.25577e-5f * altitude), 5.25588f);
	
        // calculate the air density at the target object's altitude
        // this is a simplified equation that only takes into account the altitude, air pressure, and temperature
        // for a more accurate calculation, other factors such as humidity would also need to be considered
        airDensity = _airPressure / 287.05f * (temperatureKelvin + 273.15f);

        // calculate the humidity at the target object's altitude
        // this is a simplified equation that only takes into account the altitude and a constant relative humidity
        // for a more accurate calculation, other factors such as temperature and air pressure would also need to be considered
        humidity = 0.5f * Mathf.Exp(-0.0006396f * altitude);

		return _airPressure;
	}

	public static float GetAirDensity(float altitude)
	{
       // calculate the altitude of the target object
        
		// calculate the temperature in Celsius at the target object's altitude
		float temperatureCelsius = 15.0f - 0.0065f * altitude;
		// convert the temperature to Kelvin
		float temperatureKelvin = temperatureCelsius + 273.15f;
        // calculate the air pressure at the target object's altitude
        // this is a simplified equation that only takes into account the altitude and a standard atmosphere model
        // for a more accurate calculation, other factors such as temperature and humidity would also need to be considered
        airPressure = 101325.0f * Mathf.Pow((1 - 2.25577e-5f * altitude), 5.25588f);
	
        // calculate the air density at the target object's altitude
        // this is a simplified equation that only takes into account the altitude, air pressure, and temperature
        // for a more accurate calculation, other factors such as humidity would also need to be considered
        airDensity = airPressure / (287.05f * (temperatureKelvin + 273.15f));

        // calculate the humidity at the target object's altitude
        // this is a simplified equation that only takes into account the altitude and a constant relative humidity
        // for a more accurate calculation, other factors such as temperature and air pressure would also need to be considered
        humidity = 0.5f * Mathf.Exp(-0.0006396f * altitude);
		return airDensity;
	}





  	public static float ThicknessCalculation(float angle, float ArmourThickness)
	{
		float Llos = ArmourThickness / Mathf.Cos(angle);

		return Llos;
	}

	public static float InitialVelocity(float propellantMass, float explosiveVelocity, float projectileMass)
	{
		float initialVelocity = propellantMass * explosiveVelocity / projectileMass;
		return initialVelocity;
	}

	public static float GetDragCoefficient(float airDensity, float projectileArea, float dragCoefficient,float speed)
	{
 		float _dragCoefficient = 0.5f * airDensity * projectileArea;
		float dragForce = 0.5f * dragCoefficient * Mathf.Sqrt(speed);
		return dragForce;
	}
	public static float GetGravitationalAcceleration( float projectileAltitude)
	{
		float gravitationalAcceleration = 9.80665f * Mathf.Pow((1 - 2.25577e-5f * projectileAltitude), 2.25577f);
		return gravitationalAcceleration;
	}

	public static float GetFullCaliberPenetration(float bulletVelocity, float impactAngle, float targetThickness)
	{
		float penetration = bulletVelocity * bulletVelocity * Mathf.Sin(2 * impactAngle) / targetThickness;
		return penetration;
	}

	public static float PerforationLimit( float shellLength, float shellDiameter, float frustrumLength, float tipDiameter, float penetratorDensity, float penetratorHardness, float impactVelocity, float plateDensity, float plateHardness, float impactAngle, int PenetratorType) //How far the shell can penetrate with its kinetic energy
	{

			float b0 = 0.283f;
			float b1 = 0.0656f;
			float m  = -0.224f;

			float a_t = 0.994f;
			float a_sit=0.921f;
			float a_d = 0.825f;
			float a_s = 1.104f;

			float c0_t = 134.5f;
			float c0_sit=138f;
			float c0_d = 90.0f;
			float c0_s = 9874.0f;

			float c1_t = -0.148f;
			float c1_sit=-0.100f;
			float c1_d = -0.0849f;

			float k_s = 0.3598f;
			float n_s = -0.2342f;

	

			float p_len  = shellLength;
			float dia = shellDiameter;
			float f_len = frustrumLength;
			float df = tipDiameter;
			float rhop = penetratorDensity;
			float bhnp = penetratorHardness;
			float vt = impactVelocity / 1000;
			float rhot = plateDensity;
			float bhnt = plateHardness;
			float nato = impactAngle;
			
			// calculations 
			// -------------

			float lw  = p_len-f_len*(1f-(1f+df/dia*(1f+df/dia))/3f);   // working length
			float lwd = lw/dia;   

			float elwd = 1/math.tanh(b0 + b1*lwd);   //L/d influence

			float nato_s = nato;
			float enato = Mathf.Pow(Mathf.Cos(nato_s/180f*Mathf.PI),m);            // obliquity influence
		
			float edens = Mathf.Pow(rhop/rhot,0.5f);    //density influence
	
			float vt_s = vt;

			
			//tungsten perforation equation
			if(PenetratorType == 1)
			{
				float eterm5_t = Mathf.Exp(-(c0_t + c1_t*bhnt)*bhnt/rhop/vt_s/vt_s); 
				float vt_min = Mathf.Pow((c0_t + c1_t*bhnt)*bhnt/rhop/1.5f,0.5f); 
					float vt_min_s = vt_min;
				        if (vt < vt_min)  { Debug.Log("- The impact velocity is less than the minimal velocity   ("+vt_min_s+" km/s)   for eroding penetration.");  } 
           				 if (bhnt < 149)   { Debug.Log("- The target hardness should be greater or equal than 150.");  }
            				if (bhnt > 501)   { Debug.Log("- The target hardness should be less or equal than 500.");  }

            				if (rhop < 16500)   { Debug.Log("- The penetrator density should be greater than 16500 kg/m^3.");  }
           				 if (rhop > 19300)   { Debug.Log("- The penetrator density should be less than 19300 kg/m^3.");  } 

					 perf = a_t * lw * elwd * enato * edens * eterm5_t;

					
			}

			//DU perforation equation
			if(PenetratorType == 0)
			{
				float eterm5_d = Mathf.Exp(-(c0_d + c1_d*bhnt)*bhnt/rhop/vt_s/vt_s);
				float vt_min = Mathf.Pow((c0_t + c1_t*bhnt)*bhnt/rhop/1.5f,0.5f); 
					float vt_min_s = vt_min;
				if (vt < vt_min)  {  Debug.Log("- The impact velocity is less than the minimal velocity   ("+vt_min_s+" km/s)   for eroding penetration.");  } 
         
            			if (bhnt < 149)   { Debug.Log("- The target hardness should be greater or equal than 150.");  }
            			if (bhnt > 501)   {Debug.Log("- The target hardness should be less or equal than 500.");  }

           			if (rhop < 16500)   {Debug.Log("- The penetrator density should be greater than 16500 kg/m^3.");  }
            			if (rhop > 19100)   {Debug.Log("- The penetrator density should be less than 19100 kg/m^3.");   } 

				perf = a_d * lw * elwd * enato * edens * eterm5_d; 
				
				
			}
			
			if(PenetratorType == 2) //Steel
			{
				float eterm5_s = Mathf.Exp(-c0_s*Mathf.Pow(bhnt,k_s)*Mathf.Pow(bhnp,n_s)/rhop/vt_s/vt_s);
				float vt_min = Mathf.Pow(   c0_s*Mathf.Pow(bhnt,k_s)  *  Mathf.Pow(bhnp,n_s)/rhop/1.5f,0.5f);
					float vt_min_s = vt_min;
				perf = a_s * lw * elwd * enato * edens * eterm5_s;

				
			}
			
			return perf;
	}

	public static float Soviet_Demarre(float b, float q, float V, float d, float A)
	{
		float _A = A * Mathf.Deg2Rad;
		float K = 2200.0f;
		if(A > 60.0f)
		{
			K = 2400.0f;
		}
		b = V * Mathf.Pow(q, 0.5f) * Mathf.Pow(Mathf.Sin(_A),  0.7f) / K * Mathf.Pow(d, 0.75f);
		
		float penetration = b * 10.0f;
		return penetration;
	}

	public static float Patel_Gillingham(float v, float m, float D, float T)
	{
		float P = Mathf.Pow(m * Mathf.Pow(v, 2) / 0.7f * T * Mathf.PI * D, 0.5f);

		return P / 10000;
	}
   public static float  calculateKrupp(float _estCaliber, float _estMass, float _estVelocity) {
   
    float estCaliber = _estCaliber;
    float estMass = _estMass;
    float estVelocity = _estVelocity;
    
    float penetration = 100*(estVelocity*Mathf.Sqrt(estMass))/(2400*Mathf.Sqrt(estCaliber/100));
    
    return penetration;

	}

   public static float  calculateDemarre(float refPen, float refCal, float refMs, float refVelo, float estCal, float estMs, float estVelo) {
       float refPenetration = refPen;
    float refCaliber = refCal;
    float refMass = refMs;
    float refVelocity = refVelo;
    
    float estCaliber = estCal;
    float estMass = estMs;
    float estVelocity = estVelo;
    
    float penetration = refPenetration * Mathf.Pow(estVelocity/refVelocity, 1.4283f) * Mathf.Pow(estCaliber/refCaliber, 1.0714f) * Mathf.Pow(estMass/Mathf.Pow(estCaliber,3), 0.7143f) / Mathf.Pow(refMass/Mathf.Pow(refCaliber,3), 0.7143f);
    
    return penetration;

   }



	public static float CalculateSonic(float speed, float SuperSonicMinVelo, float TransSonicMinVelo, float SubSonicMaxVelo) //calculates where the bullet is high velocity or low
	{
					

						
						if(speed > SuperSonicMinVelo)
						{
							Sonic = 2;
						}
						if(speed < TransSonicMinVelo && speed > SubSonicMaxVelo) //If it is not trans sonic or sub sonic
						{
							Sonic = 3;
						}
						if(speed > TransSonicMinVelo || speed < SuperSonicMinVelo)
						{
							Sonic = 1;
						}
						if(speed < SubSonicMaxVelo || speed < TransSonicMinVelo)
						{
							Sonic = 0;
						}

						return Sonic;
	}

}





}
