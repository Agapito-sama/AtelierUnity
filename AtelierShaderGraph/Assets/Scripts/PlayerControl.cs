using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    #region Attributes
    CharacterController character;
    Animator anim;
    public Transform forwardIndicator;

    // Ground Movement
    [Header("Movement")]
    public float speed = 1;
    float originalSpeed;
    public float acceleration = 0.5f;
    public float turnSpeed = 1;
    Vector3 movementTotal;
    Vector3 move;
    float turnAmount;
    int forwardHash;
    int turnHash;
    int slideHash;

    // Air movement
    [Header("Air")]
    public LayerMask layersToIgnore;
    bool isGrounded;
    bool jumping;
    public float jumpStrength = 0.5f;
    float maxJumpStrength;
    public float jumpAcceleration = 1f;
    public float gravity = 1;
    float groundCheckDistance = 0.1f;

    [Header("Attack")]
    bool attacking;

    [Header("Slope")]
    public float groundSlopeAngle = 0f;            // Angle of the slope in degrees
    public Vector3 groundSlopeDir = Vector3.zero;  // The calculated slope as a vector
    public float slideSpeed = 1f;
    public float slideAcceleration = 1f;
    public float turnSlideSpeed = 1;

    [Header("Debug")]
    public bool showDebug = false;                  // Show debug gizmos and lines
    public LayerMask castingMask;                  // Layer mask for casts. You'll want to ignore the player.
    public float startDistance = 0.2f;   // Should probably be higher than skin width
    public float sphereCastRadius = 0.25f;
    public float sphereCastDistance = 0.75f;       // How far spherecast moves down from origin point


    public float raycastLength = 0.75f;
    public Vector3 rayOriginOffset1 = new Vector3(-0.2f, 0f, 0.16f);
    public Vector3 rayOriginOffset2 = new Vector3(0.2f, 0f, -0.16f);



    #endregion

    void Start()
    {
        character = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        // On transforme ces strings en Hash parce qu'on les cherche tout le temps sur FixedUpdate et c'est plus optimisé.
        forwardHash = Animator.StringToHash("Forward");
        turnHash = Animator.StringToHash("Turn");
        slideHash = Animator.StringToHash("Slide");

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
        if (Input.GetKeyDown(KeyCode.LeftShift) && !attacking)
        { StartCoroutine(Attack()); StartCoroutine(Jump()); }
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
        if (jumping || attacking)
        {
            // On determine l'hauteur en dépendent de combien de temps on appuie sur espace.
            if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift) && jumpStrength < maxJumpStrength)
            {
                jumpStrength += jumpAcceleration * Time.fixedDeltaTime;
                speed += acceleration * Time.fixedDeltaTime; // On bouge légérement plus vite dans l'air.
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
        {
            isGrounded = true;
        }
        else
        { isGrounded = false; }

        CheckGroundSlope();
    }

    // Bool pour s'assurer de qu'on saut pas deux fois
    IEnumerator Jump()
    {
        jumping = true;
        jumpStrength = 0; 
        yield return new WaitForSeconds(0.5f);
        jumping = false;
    }
    IEnumerator Attack()
    {
        attacking = true;
        if (!jumping) { jumpStrength = 0; }
        anim.Play("Attack");
        yield return new WaitForSeconds(1f);
        attacking = false;
    }


    public void CheckGroundSlope()
    {
        Vector3 origin = new Vector3(transform.position.x, transform.position.y + startDistance, transform.position.z);


        bool _evenGround = false;
        RaycastHit[] hits = Physics.SphereCastAll(origin, sphereCastRadius, Vector3.down, sphereCastDistance, castingMask);
        for (int i = 0; i < hits.Length; i++)
        {
            float _angle = Vector3.Angle(hits[i].normal, Vector3.up);
            if (_angle < character.slopeLimit)
            {
                _evenGround = true;
                groundSlopeAngle = _angle;
            }
            else if (!_evenGround)
            {
                _evenGround = false;
                groundSlopeAngle = _angle;
                Vector3 temp = Vector3.Cross(hits[i].normal, Vector3.down);
                groundSlopeDir = Vector3.Cross(temp, hits[i].normal);
            }
        }

        if (groundSlopeAngle > character.slopeLimit)
        {
            Slide(groundSlopeDir, groundSlopeAngle);
        }
        else { anim.SetFloat(slideHash, 2); }
        _evenGround = false;

        /*
        RaycastHit hit;
        // SPHERECAST
        // "Casts a sphere along a ray and returns detailed information on what was hit."
        if (Physics.SphereCastAll(origin, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, castingMask))
        {
            Debug.Log("Slope");
            groundSlopeAngle = Vector3.Angle(hit.normal, Vector3.up);

            // Find the vector that represents our slope as well. 
            //  temp: basically, finds vector moving across hit surface 
            Vector3 temp = Vector3.Cross(hit.normal, Vector3.down);
            //  Now use this vector and the hit normal, to find the other vector moving up and down the hit surface
            groundSlopeDir = Vector3.Cross(temp, hit.normal);
            if (groundSlopeAngle > character.slopeLimit)
            {
                Slide(groundSlopeDir, groundSlopeAngle);
            }
        }
        */

    }

    void Slide(Vector3 direction, float angle)
    {
        direction = Vector3.Scale(direction, new Vector3(1, 0, 1)).normalized;
        // Mouvement directionel, on normalise pour s'assurer de que le vecteur move a une magnitude d'un.
        // Si il y a pas de movement, on arrête la rotation.
        if (direction != Vector3.zero)
        {
            Quaternion _targetRotation = Quaternion.LookRotation(direction, transform.up);
            Quaternion _newRotation = Quaternion.Lerp(transform.rotation, _targetRotation, turnSlideSpeed * Time.fixedDeltaTime);
            transform.rotation = _newRotation;
        }
        movementTotal = Vector3.Lerp(movementTotal, direction * angle * slideSpeed * Time.fixedDeltaTime, slideAcceleration); ;
        

        Vector3 _move = transform.InverseTransformDirection(direction);
        turnAmount = Mathf.Atan2(_move.x, _move.z);
        anim.SetFloat(forwardHash, _move.z, 0.1f, Time.deltaTime);
        anim.SetFloat(turnHash, turnAmount, 0.1f, Time.deltaTime);

        if (turnAmount < 0) { turnAmount *= -1; }
        anim.SetFloat(slideHash, turnAmount);
    }

    void OnDrawGizmosSelected()
    {
        if (showDebug)
        {
            // Visualize SphereCast with two spheres and a line
            Vector3 startPoint = new Vector3(transform.position.x, transform.position.y + startDistance, transform.position.z);
            Vector3 endPoint = new Vector3(transform.position.x, transform.position.y + startDistance - sphereCastDistance, transform.position.z);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(startPoint, sphereCastRadius);

            Gizmos.color = Color.gray;
            Gizmos.DrawWireSphere(endPoint, sphereCastRadius);

            Gizmos.DrawLine(startPoint, endPoint);
        }
    }

}
