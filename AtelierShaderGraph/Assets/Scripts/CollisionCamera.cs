using UnityEngine;
using System.Collections;

public class CollisionCamera : MonoBehaviour {
    public Transform camPosition;
    public Transform targetPosition;
    public float maxDistanceFromPlayer = 2.5f;
    public float sphereCastRadius = 0.3f;
    Vector3 velocity = Vector3.zero;
    public LayerMask layersToIgnore;


    // On check avec le SphereCast si il y a quelque chose entre le target et la caméra, 
    // en la rapprochant si c'est le cas. et en la laissant a ça place si ce n'est pas.

    void FixedUpdate () {
        RaycastHit hitPivot;
        if (Physics.SphereCast(targetPosition.position, sphereCastRadius, transform.position - targetPosition.position, 
            out hitPivot, maxDistanceFromPlayer, ~layersToIgnore, QueryTriggerInteraction.Ignore))
        {
            float _distanceHit = -hitPivot.distance + .1f;
            // _newPos utilise un offset sur le x, pour que la caméra ne soit pas diréctement derrière le perso
            // On calcule ça avec un règle de trois avec l'offset, la maxdistance et la distance où il y a eu la collision.
            float offsetX = transform.localPosition.x * _distanceHit / -maxDistanceFromPlayer;
            Vector3 _newPos = new Vector3(offsetX, transform.localPosition.y, _distanceHit);
            camPosition.localPosition = Vector3.SmoothDamp(camPosition.localPosition, _newPos, ref velocity, 0.001f);
        }
        else
        {
            Vector3 _newPos = new Vector3(transform.localPosition.x, transform.localPosition.y, -maxDistanceFromPlayer);
            camPosition.localPosition = Vector3.SmoothDamp(camPosition.localPosition, _newPos, ref velocity, 1f);
        }
    }
}
