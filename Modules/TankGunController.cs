using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bullets {


public class TankGunController : MonoBehaviour
{
    public Gun gun;

    public int shootButton;
    public KeyCode reloadKey;

    public KeyCode fire;
    void Update() {
        if(Input.GetKeyDown(fire)) {
            gun.Shoot();
        }

        if(Input.GetKeyDown(reloadKey)) {
            gun.Reload();
        }
    }
}


}
