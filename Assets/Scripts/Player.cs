using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Character movements")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Player properties")]
    [SerializeField] private float characterHealth = 100f;
    [SerializeField] private PlayerHealth healthSystem;

    [Header("Camera")]
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

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

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
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
    }

    void OnTriggerEnter(Collider other)
    {
        string tagName = other.tag;

        if (tagName == DamageAreas.STANDARD_ENEMY_DAMAGE_AREA)
        {
            Debug.Log("Haasr uygulanıyor");
            // 10 DPS, 1 saniyede bir
            healthSystem.AddDamage(DamageAreas.STANDARD_ENEMY_DAMAGE_AREA_VALUE, tagName, 1f);
        }

        Debug.Log("Enter area" + tagName);
    }

    void OnTriggerExit(Collider other)
    {
        string tagName = other.tag;

        if (tagName == DamageAreas.STANDARD_ENEMY_DAMAGE_AREA)
        {
            healthSystem.RemoveDamage(tagName);
        }

        Debug.Log("Exit area" + tagName);
    }
}
