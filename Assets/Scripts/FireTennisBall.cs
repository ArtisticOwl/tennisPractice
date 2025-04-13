using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FireTennisBall : MonoBehaviour
{
    public GameObject TennisBall;
    public float BallSpeed;
    public Transform playerTransform;
    public float minDistanceFromPlayer = 5f;
    public float maxDistanceFromPlayer = 10f;
    public float numBalls = 5;
    public float maxBalls = 5;
    public float reloadTime = 4f;
    public float timeBetweenShots = 7.0f;  // Base delay between each shot
    public float aimDuration = 4.0f; // How long the aim assist is shown before firing

    private bool isReloading = false;
    public GameObject aimAssistPrefab;
    private AudioSource shootingsfx;
    public Text ballsText;
    public GameManager gameManager; // Ensure this is assigned, ideally via Inspector or FindObjectOfType
    private static FireTennisBall instance;
    public static FireTennisBall Instance { get { return instance; } }

    // Keep track of active coroutines to stop them if needed
    private Coroutine fireSequenceCoroutine = null;
    private Coroutine reloadCoroutine = null;

    void Start()
    {
        instance = this;
        shootingsfx = GetComponent<AudioSource>(); // Safer way to get component on the same object

        // Attempt to find GameManager if not assigned
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("FireTennisBall could not find GameManager!", this);
                enabled = false; // Disable script if GameManager is essential and missing
                return;
            }
        }

        numBalls = maxBalls;
        UpdateBallsText(); // Use a helper function for consistency

        // Ensure playerTransform is found
        if (playerTransform == null)
        {
            VNectModel playerModel = FindObjectOfType<VNectModel>();
            if (playerModel != null)
            {
                playerTransform = playerModel.transform;
            }
            else
            {
                Debug.LogError("FireTennisBall could not find player transform (VNectModel)!", this);
                enabled = false; // Disable if player is essential
                return;
            }
        }

        // Example: Start firing when the game starts (adjust trigger as needed)
        // Make sure gameManager.gameStarted is set appropriately elsewhere
        // StartCoroutine(CheckGameStarted());
    }

    // Optional: Example of how to trigger the sequence based on GameManager
    // IEnumerator CheckGameStarted()
    // {
    //     // Wait until the game is actually marked as started
    //     yield return new WaitUntil(() => gameManager != null && gameManager.gameStarted);
    //     StartFiringSequence();
    // }

    // Public method to start the firing process
    public void StartFiringSequence()
    {
        // Stop any existing sequences before starting a new one
        StopFiringSequence();
        if (!isReloading)
        {
             fireSequenceCoroutine = StartCoroutine(FireBallsSequence());
        }
        else
        {
            // If currently reloading, start the reload process again (or wait)
            reloadCoroutine = StartCoroutine(Reloading());
        }
    }

    // Public method to stop firing (e.g., when game pauses or ends)
    public void StopFiringSequence()
    {
        isReloading = false; // Reset reload state if stopped externally
        if (fireSequenceCoroutine != null)
        {
            StopCoroutine(fireSequenceCoroutine);
            fireSequenceCoroutine = null;
        }
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }
        // Consider stopping individual FireBall coroutines if necessary,
        // though they check gameManager.gameStarted internally now.
        StopAllCoroutinesRelatedToFiring(); // More robust cleanup
    }

    // Helper to stop individual shots if needed
    void StopAllCoroutinesRelatedToFiring()
    {
        // This stops ALL coroutines on this MonoBehaviour, including FireBall instances.
        // Be careful if other unrelated coroutines run on this script.
        StopAllCoroutines();
        // Reset state if StopAllCoroutines is too broad
        isReloading = false;
        if (fireSequenceCoroutine != null) fireSequenceCoroutine = null;
        if (reloadCoroutine != null) reloadCoroutine = null;
        // If aim assist is visible when stopped, clean it up
        // This requires tracking the aim assist object potentially
    }


    // Manages the sequence of firing multiple balls then reloading
    public IEnumerator FireBallsSequence()
    {
        // No need for isReloading = true here, it's handled by Reloading coroutine

        // Check if game is running before starting the loop
        while (numBalls > 0 && gameManager != null && gameManager.gameStarted)
        {
            // Start the coroutine for a single shot, but don't wait for it here.
            // Let FireBall handle its own timing internally.
            StartCoroutine(FireBall());

            // Wait for the time between shots AFTER initiating the next shot
            // Add a check after waiting in case the game state changed
            yield return new WaitForSeconds(timeBetweenShots);
            if (gameManager == null || !gameManager.gameStarted) yield break; // Exit if game stopped during wait
        }

        // If the loop finished (either out of balls or game stopped),
        // check if we ran out of balls and need to reload.
        if (numBalls <= 0 && gameManager != null && gameManager.gameStarted)
        {
            reloadCoroutine = StartCoroutine(Reloading());
        }

        fireSequenceCoroutine = null; // Mark sequence as finished
    }


    // Handles the process for firing a single ball
    public IEnumerator FireBall()
    {
        // Initial check if the game is running - essential before doing anything
        if (gameManager == null || !gameManager.gameStarted || playerTransform == null)
        {
            yield break; // Exit immediately if game not running or player missing
        }

        // --- Aiming Phase ---
        // Calculate direction and target position relative to the player
        float distanceFromPlayer = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
        // Keep random direction logic as before
        float xDirection = Random.Range(-0.7f, 0.7f);
        float yDirection = Random.Range(1.5f, 2.25f);
        float zDirection = Random.Range(0f, 1.2f); // Note: Relative Z depends on camera/player orientation setup
        Vector3 randomOffset = new Vector3(xDirection, yDirection, zDirection);

        // Calculate the target position in world space
        Vector3 aimPosition = playerTransform.position + randomOffset; // Simplified: Aim near player + offset

        // Create aim assist prefab at the calculated target position
        GameObject aimAssist = Instantiate(aimAssistPrefab, aimPosition, Quaternion.identity);

        // Wait for aim duration - player adjusts during this time
        yield return new WaitForSeconds(aimDuration);

        // --- Firing Phase ---
        // CRITICAL CHECK: Ensure game is still running and aim assist still exists AFTER the wait
        if (gameManager == null || !gameManager.gameStarted || aimAssist == null)
        {
            // Cleanup if the game stopped or aim assist was somehow destroyed during the wait
            if (aimAssist != null) Destroy(aimAssist);
            yield break; // Exit without firing
        }

        // Play sound effect just before firing
        if (shootingsfx != null) shootingsfx.Play();

        // Spawn and fire the ball towards the aim assist position
        if (TennisBall != null) // Check if prefab exists
        {
             GameObject ball = Instantiate(TennisBall, transform.position, Quaternion.identity);
             BallScript ballScript = ball.GetComponent<BallScript>();
             if (ballScript != null)
             {
                 // Calculate direction from firing point to the target
                 Vector3 fireDirection = (aimPosition - transform.position).normalized;
                 ballScript.FireBall(fireDirection, BallSpeed, 0); // Assuming FireBall handles physics
             }
             else
             {
                 Debug.LogWarning("Spawned TennisBall is missing BallScript!", ball);
             }
             Destroy(ball, 7f); // Destroy ball after some time (consider pooling for performance)
        }
        else
        {
            Debug.LogError("TennisBall prefab is not assigned!", this);
        }

        // Cleanup aim assist
        Destroy(aimAssist);

        // Decrement balls ONLY after successful firing sequence part
        numBalls--;
        UpdateBallsText();

        // Check if we just ran out of balls and need to trigger reload *from here*
        // This overlaps slightly with FireBallsSequence logic but ensures reload starts promptly
        if (numBalls <= 0 && !isReloading && gameManager != null && gameManager.gameStarted)
        {
             reloadCoroutine = StartCoroutine(Reloading());
             // If FireBallsSequence is running, it might also try to reload.
             // The isReloading flag prevents duplicate reloads.
        }
        // The 'while (gameManager.gameStarted)' loop from the original FireBall was removed
        // as this coroutine should represent firing just ONE ball per call.
        // The loop control belongs in FireBallsSequence.
    }

    // Handles the reloading process
    IEnumerator Reloading()
    {
        // Prevent multiple reload coroutines
        if (isReloading) yield break;

        isReloading = true;
        ballsText.text = "Balls: reloading...";

        // Wait for the specified reload time
        yield return new WaitForSeconds(reloadTime);

        // Check if game is still running AFTER the wait
        if (gameManager == null || !gameManager.gameStarted)
        {
            isReloading = false; // Reset flag even if game stopped
            // Don't refill balls if the game ended during reload
            yield break;
        }

        // Reload complete
        numBalls = maxBalls;
        isReloading = false;
        reloadCoroutine = null; // Mark reload as finished
        UpdateBallsText();

        // Automatically restart the firing sequence after reloading if the game is still on
        if (gameManager.gameStarted)
        {
            // Ensure we don't have an old sequence coroutine reference somehow
            if (fireSequenceCoroutine == null)
            {
                fireSequenceCoroutine = StartCoroutine(FireBallsSequence());
            }
        }
    }

    // Helper function to update UI text
    void UpdateBallsText()
    {
        if (ballsText != null)
        {
            ballsText.text = $"Balls: {numBalls}";
        }
    }

    // Optional: Cleanup coroutines when the object is destroyed
    void OnDestroy()
    {
        StopAllCoroutines(); // Stop all coroutines running on this component
        // Clean up any persistent objects if necessary (like aim assist if it wasn't destroyed)
    }
}