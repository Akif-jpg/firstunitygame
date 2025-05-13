using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Character movements")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Player properties")]
    [SerializeField] private float characterHealth = 100f;
    [SerializeField] private PlayerHealth healthSystem;
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private float sprintMultiplier = 1f;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool canJump;
    private float xRotation = 0f;
    private float baseDistance;

    void Start()
    {
        this.canJump = true;
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        if (weaponTransform != null)
        {
            Vector2 cameraPosition = new Vector2(cameraTransform.position.x, cameraTransform.position.z);
            Vector2 weaponPosition = new Vector2(weaponTransform.position.x, weaponTransform.position.z);
            this.baseDistance = Vector2.Distance(cameraPosition, weaponPosition);
        }

        healthSystem.SetCharacterHealth(this.characterHealth);
    }

    void Update()
    {
        Move();
        LookAround();
    }

    void Move()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // These commands are for sprinting
        float speed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed *= sprintMultiplier;
        }

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded && canJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void LookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
        UpdateWeaponPosition();
    }

    void UpdateWeaponPosition()
    {
        weaponTransform.localRotation = Quaternion.Euler(-xRotation, -180f, 0f);

        // Adjust weapon position based on camera angle
        Vector3 weaponOffset = new Vector3(0.3519999f, 0.4010001f + Mathf.Sin(Mathf.Deg2Rad * xRotation) * baseDistance * -1, 0.813f);
        weaponTransform.localPosition = weaponOffset;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Elevator"))
        {
            canJump = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Elevator"))
        {
            canJump = true;
        }
    }



}