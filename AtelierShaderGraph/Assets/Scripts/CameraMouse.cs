using UnityEngine;
using System.Collections;

public class CameraMouse : MonoBehaviour {

    public float movementSpeed = 1f;

    public Transform target; // Le personnage
    public Transform pivot;
    Camera cam;
    public CollisionCamera collisionScript;


    public float rotateXSpeed = 5f;
    public float rotateYSpeed = 3f;
    public float tiltMax = 75f;
    public float tiltMin = 45f;

    public bool cursorLock = true;
    public float maxDistance = 2.5f;
    public float maxDistanceFocused = 4f;

    float tiltAngle;
    Vector3 pivotEulers;
    Quaternion pivotRotation;
    Quaternion targetRotation;


    void Awake () {
        cam = Camera.main;
        pivotEulers = pivot.rotation.eulerAngles;
        pivotRotation = pivot.transform.localRotation;
        targetRotation = transform.localRotation;
    }

    void Start () {
        if (cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        // on comunique a collision script la distance qu'on a ajusté sur l'inspecteur pour commodité.
        collisionScript.maxDistanceFromPlayer = maxDistance;
    }




    void FixedUpdate() {
        // On positionne la caméra sur fixed update pour éviter du jitter.
        transform.position = Vector3.Lerp(transform.position, target.position, movementSpeed * Time.deltaTime);
    }

    void LateUpdate () {
        // Les movements de rotation sont calculés sur LateUpdate
        // comme ça on s'assure que le perso a terminé son mouvement et en évite du jitter.
        // On calcule combien la caméra doit orbiter dans l'horizontale.
        float xInput = Input.GetAxis("Mouse X") * rotateXSpeed;
        float yInput = Input.GetAxis("Mouse Y") * rotateYSpeed;

        float _lookAngle = transform.localEulerAngles.y;
        _lookAngle = _lookAngle + xInput;
        targetRotation = Quaternion.Euler(0f, _lookAngle, 0f);

        // Maintenant on calcule le pivot, pour aller en haut et en bas dans les limites de max et min tiltAngles.
        tiltAngle = tiltAngle - (yInput * rotateYSpeed);
        tiltAngle = Mathf.Clamp(tiltAngle, -tiltMin, tiltMax);

        pivotRotation = Quaternion.Euler(tiltAngle, pivotEulers.y, pivotEulers.z);


        pivot.localRotation = pivotRotation;
        transform.localRotation = targetRotation;
    }
}
