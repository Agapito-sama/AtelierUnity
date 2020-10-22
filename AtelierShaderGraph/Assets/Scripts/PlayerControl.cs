using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    #region Attributes
    CharacterController character;
    Animator anim;
    public Transform forwardIndicator;

    // Ground Movement
    public float speed = 1;
    float originalSpeed;
    public float acceleration = 0.5f;
    public float turnSpeed = 1;
    Vector3 movementTotal;
    Vector3 move;
    float turnAmount;
    int forwardHash;
    int turnHash;

    // Air movement
    public LayerMask layersToIgnore;
    bool isGrounded;
    bool jumping;
    public float jumpStrength = 0.5f;
    float maxJumpStrength;
    public float jumpAcceleration = 1f;
    public float gravity = 1;
    Vector3 fallingInitialMovement;
    float groundCheckDistance = 0.1f;
    #endregion

    void Start()
    {
        character = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        // On transforme ces strings en Hash parce qu'on les cherche tout le temps sur FixedUpdate et c'est plus optimisé.
        forwardHash = Animator.StringToHash("Forward");
        turnHash = Animator.StringToHash("Turn");

        originalSpeed = speed;
        maxJumpStrength = jumpStrength;
    }

    private void Update()
    {
        // On check si le joueur saut sur Update parce que sur FixedUpdate on peut perdre des inputs.
        if (isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.Space) && !jumping)
            { StartCoroutine(Jump()); }
        }
    }

    private void FixedUpdate()
    {
        Move();

        AirMovement();

        character.Move(movementTotal);
    }


    void Move()
    {
        // Mouvement directionel, on normalise pour s'assurer de que le vecteur move a une magnitude d'un.
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 forwardVector = Vector3.Scale(forwardIndicator.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 rightVector = Vector3.Scale(forwardIndicator.right, new Vector3(1, 0, 1)).normalized;
        move = vertical * forwardVector + horizontal * rightVector;

        if (move.magnitude > 1f) { move.Normalize(); }
        // Si il y a pas de movement, on arrête la rotation.
        if (move != Vector3.zero){
            Quaternion _targetRotation = Quaternion.LookRotation(move, transform.up);
            Quaternion _newRotation = Quaternion.Lerp(transform.rotation, _targetRotation, turnSpeed * Time.fixedDeltaTime);
            transform.rotation = _newRotation;
        }
        
        movementTotal = Vector3.Lerp(movementTotal, move * speed * Time.fixedDeltaTime, acceleration);

        // On transforme le mouvement en des vecteurs locales (maintenant move.x indique du movement vers le front) 
        // alors si il y a du movement sur z on sait que le personnage tourne et en passe la valeur a l'animator.
        Vector3 _move = transform.InverseTransformDirection(move);
        turnAmount = Mathf.Atan2(_move.x, _move.z);
        anim.SetFloat(forwardHash, _move.z, 0.1f, Time.deltaTime);
        anim.SetFloat(turnHash, turnAmount, 0.1f, Time.deltaTime);
    }

    void AirMovement()
    {
        if (jumping)
        {
            // On determine l'hauteur en dépendent de combien de temps on appuie sur espace.
            if (Input.GetKey(KeyCode.Space) && jumpStrength < maxJumpStrength)
            {
                jumpStrength += jumpAcceleration * Time.fixedDeltaTime;
                speed += jumpStrength; // On bouge légérement plus vite dans l'air.
                movementTotal.y = jumpStrength;
            }
        }
        else { speed = originalSpeed; } // Reset speed après le saut.
        CheckGroundStatus();
        // Gravité constante pour s'assurer qu'on descends correctement les pentes et pour quand on tombe.
        movementTotal.y -= gravity * Time.fixedDeltaTime; 

    }

    void CheckGroundStatus()
    {
        // Sphere pour déterminer si on est Grounded, elle est légerement plus grande que la capsule
        // du CharacterController, pour s'assurer qu'on ne reste pas bloqué sur la geometrie.
        if (Physics.CheckSphere(transform.position + (Vector3.up * 0.3f), 0.4f, layersToIgnore, QueryTriggerInteraction.Ignore))
        { isGrounded = true; }
        else
        { isGrounded = false; }
    }

    // Bool pour s'assurer de qu'on saut pas deux fois
    IEnumerator Jump()
    {
        jumping = true;
        jumpStrength = 0; 
        yield return new WaitForSeconds(0.5f);
        jumping = false;
    }
}
