using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{
    /* L'arme du joueur a un trigger avec un tag ("PlayerWeapon"), quand celui-ci touche
     * la hitbox de l'ennemi (un trigger avec ce script attaché) le code ce declenche
     * détruissant le parent de cet objet. Il fait aussi apparaître un effet de particules
     * avec une rotation dependante de l'arme du joueur au moment de la collision.
    */
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
