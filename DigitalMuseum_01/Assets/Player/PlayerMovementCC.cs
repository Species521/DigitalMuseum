using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerMovementCC : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 2f;

    [Header("Movement Smoothing")]
    public float acceleration = 10f;
    public float deceleration = 12f;

    [Header("Gravity")]
    public float gravity = -9.81f;

    [Header("Footstep Audio")]
    public AudioClip footstepLoop;

    [Range(0f, 1f)]
    public float footstepVolume = 0.6f;

    public float walkPitch = 1.0f;
    public float sprintPitch = 1.35f;

    public float fadeOutTime = 0.25f;

    private CharacterController controller;
    private AudioSource audioSource;

    private Vector3 velocity;
    private Vector3 currentMove;

    private bool footstepsPlaying = false;
    private Coroutine fadeRoutine;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Update()
    {
        if (CameraControllerFPS.IsInCursorMode)
            return;

        HandleMovement();
        HandleGravity();
        HandleFootsteps();
    }

    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 inputMove = transform.right * x + transform.forward * z;
        inputMove = inputMove.normalized;

        float speed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
            speed *= sprintMultiplier;

        Vector3 targetMove = inputMove * speed;

        float smooth = inputMove.magnitude > 0 ? acceleration : deceleration;

        currentMove = Vector3.Lerp(currentMove, targetMove, smooth * Time.deltaTime);

        controller.Move(currentMove * Time.deltaTime);
    }

    void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void HandleFootsteps()
    {
        bool isMoving = new Vector3(currentMove.x, 0, currentMove.z).magnitude > 0.1f;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (isMoving && controller.isGrounded)
        {
            if (!footstepsPlaying)
            {
                audioSource.clip = footstepLoop;
                audioSource.loop = true;
                audioSource.volume = footstepVolume;
                audioSource.Play();

                footstepsPlaying = true;
            }

            audioSource.pitch = isSprinting ? sprintPitch : walkPitch;

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }
        }
        else
        {
            if (footstepsPlaying)
            {
                footstepsPlaying = false;
                fadeRoutine = StartCoroutine(FadeOutFootsteps());
            }
        }
    }

    IEnumerator FadeOutFootsteps()
    {
        float startVolume = audioSource.volume;
        float t = 0f;

        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeOutTime);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = footstepVolume;
    }
}