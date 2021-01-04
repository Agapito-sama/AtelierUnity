using UnityEngine;
using System.Collections;

public class WaterGroundDetector : MonoBehaviour {

    /* Ce script utilise la fonctionalité des layers d'unity
     * Ce detecteur et l'eau ont des triggers attachés et se situe dans un layer différent (le layer WaterSurface)
     * à le reste d'objets. Ils réagisent seulment entre eux (on peut regler ça sur les Physics sur project settings)
     * quand ce trigger rentre en contacte avec l'eau, on active l'effet de particules. En même temps
     * en regle l'hauteur de celui là dépendant de l'hauteur de l'eau.
    */
    public ParticleSystem waterSpalshes;

    private void OnTriggerEnter(Collider other)
    {
        waterSpalshes.Play();
    }

    private void OnTriggerStay(Collider other)
    {
        waterSpalshes.transform.position = new Vector3(
            waterSpalshes.transform.position.x, 
            other.transform.position.y, 
            waterSpalshes.transform.position.z);
    }

    private void OnTriggerExit(Collider other)
    {
        waterSpalshes.Stop();
    }
}
