using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{
    public ParticleSystem explosion;
    void OnTriggerEnter(Collider weapon)
    {
        if (weapon.tag == "PlayerWeapon")
        {
            Quaternion lookAtPlayer = Quaternion.LookRotation(weapon.transform.position, Vector3.up);
            GameObject.Instantiate(explosion,transform.position, lookAtPlayer, null);
            
            Destroy(transform.parent.gameObject);
        }
    }
}
