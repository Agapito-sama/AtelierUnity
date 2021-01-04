using UnityEngine;
using System.Collections;

public class WaterGroundDetector : MonoBehaviour {

    public ParticleSystem waterSpalshes;

    private void OnTriggerEnter(Collider other)
    {
        waterSpalshes.Play();
        Debug.Log("Splash");
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
        Debug.Log("Out");
    }
}
