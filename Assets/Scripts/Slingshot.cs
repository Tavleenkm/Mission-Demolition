using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour {
    [Header("Inscribed")]
    public GameObject projectilePrefab; // Prefab for the projectile
    public float velocityMult = 10f;    // Multiplier for projectile velocity
    public GameObject projLinePrefab;   // Prefab for the projectile line
    public LineRenderer rubberBand;     // Reference to the Line Renderer for the rubberband
    public AudioClip snapSoundClip;     // Snapping sound clip

    [Header("Dynamic")]
    public GameObject launchPoint;      // The point from which the projectile is launched
    public Vector3 launchPos;           // Launch position
    public GameObject projectile;       // Current projectile instance
    public bool aimingMode;             // Flag for aiming mode

    private AudioSource audioSource;    // Reference to the AudioSource for playing sounds

    void Awake() {
    Transform launchPointTrans = transform.Find("LaunchPoint");
    launchPoint = launchPointTrans?.gameObject;
    if (launchPoint == null) {
        Debug.LogError("LaunchPoint not found!");
    }

    launchPos = launchPointTrans.position; // Store the launch position
    launchPoint.SetActive(false);          // Initially hide the launch point

    // Initialize the Line Renderer
   if (rubberBand == null) {
        Debug.LogError("RubberBand LineRenderer not assigned in the Inspector.");
    } 
    else
    {
        rubberBand.positionCount = 2;  // A line with two points: start and end
        rubberBand.enabled = false;    // Initially hide the rubberband
    }

        // Get or add an AudioSource component
    audioSource = GetComponent<AudioSource>();
    if (audioSource == null) {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    // Assign the snap sound clip to the AudioSource
    if (snapSoundClip != null) {
        audioSource.clip = snapSoundClip;
    } 
    else {
        Debug.LogError("Snap Sound Clip is not assigned.");
    }
    }

    void OnMouseEnter() { 
        launchPoint.SetActive(true); // Show launch point on mouse hover
    }

    void OnMouseExit() { 
        launchPoint.SetActive(false); // Hide launch point when not hovering
    }

    void OnMouseDown() {
        // The player has pressed the mouse button while over Slingshot
        aimingMode = true;
        // Instantiate a Projectile at the launch position
        projectile = Instantiate(projectilePrefab, launchPos, Quaternion.identity);
        // Set the projectile to isKinematic for now
        projectile.GetComponent<Rigidbody>().isKinematic = true;

        // Enable and initialize the Line Renderer (rubberband)
        rubberBand.enabled = true;
        rubberBand.SetPosition(0, launchPos);   // Start of the rubberband (slingshot's launch position)
        rubberBand.SetPosition(1, projectile.transform.position);  // End of the rubberband (projectile's position)
    }

    void Update() {
        // If Slingshot is not in aimingMode, don't run this code
        if (!aimingMode || projectile == null) return;

        // Get the current mouse position in 2D screen coordinates
        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z; // Set z to camera's negative z position
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D); // Convert to world position

        // Find the delta from the LaunchPos to the mousePos3D
        Vector3 mouseDelta = mousePos3D - launchPos;

        // Limit mouseDelta to the radius of the Slingshot SphereCollider
        float maxMagnitude = this.GetComponent<SphereCollider>().radius; // Get collider radius
        if (mouseDelta.magnitude > maxMagnitude) {
            mouseDelta.Normalize(); // Normalize to prevent exceeding the radius
            mouseDelta *= maxMagnitude; // Scale to maximum radius
        }

        // Move the projectile to this new position
        if (projectile != null) {
            Vector3 projPos = launchPos + mouseDelta;
            projectile.transform.position = projPos;
        }

        // Update the Line Renderer to reflect the new position of the projectile
        rubberBand.SetPosition(0, launchPos);          // Keep the start of the line at the launch position
        rubberBand.SetPosition(1, projectile.transform.position);  // End of the line follows the projectile

        if (Input.GetMouseButtonUp(0) && projectile != null) {  // If the mouse button is released
            // The mouse has been released
            audioSource.Play();
            aimingMode = false; // Exit aiming mod

            // Play the snapping sound
            // if (audioSource != null && snapSoundClip != null) {
            
            

            Rigidbody projRB = projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false; // Set projectile to non-kinematic for physics simulation
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous; // Set collision detection
            projRB.velocity = -mouseDelta * velocityMult; // Set the projectile's velocity
            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot); // Switch camera view
            FollowCam.POI = projectile; // Set the camera's point of interest
            
            // Add a ProjectileLine to the Projectile
            GameObject projLine = Instantiate(projLinePrefab, projectile.transform);
            
            // Ensure the local position and rotation are properly set to zero
            projLine.transform.localPosition = Vector3.zero;
            projLine.transform.localRotation = Quaternion.identity;

            // Disable the Line Renderer after the projectile is launched
            rubberBand.enabled = false;

            // Reset the projectile reference
            projectile = null;
            MissionDemolition.SHOT_FIRED(); // Notify that a shot has been fired
        }
    }
}
