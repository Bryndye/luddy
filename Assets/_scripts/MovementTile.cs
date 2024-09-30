using System.Collections;
using UnityEngine;

public class MovementTile : MonoBehaviour
{
    public bool isMoving = false;
    Vector2 startPosition;
    Vector2 targetPosition;
    public float moveSpeed = 5f;  // Adjust speed for animation

    void Awake()
    {
        SetPosition(new Vector2(0, 0));  // Initial position
    }

    void Update()
    {
        if (!isMoving)
        {
            HandleMovementInput();
        }
    }

    private void SetPosition(Vector2 newPosition)
    {
        transform.position = newPosition;
    }

    void HandleMovementInput()
    {
        Vector2 direction = Vector2.zero;

        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            direction = new Vector2(Input.GetAxisRaw("Horizontal"), 0); // Right/Left movement
        }
        else if (Input.GetAxisRaw("Vertical") != 0)
        {
            direction = new Vector2(0, Input.GetAxisRaw("Vertical")); // Up/Down movement
        }

        if (direction != Vector2.zero)
        {
            // Set the new target position
            startPosition = transform.position;
            targetPosition = startPosition + direction;
            Debug.Log(targetPosition);
            StartCoroutine(SmoothMovement());
        }
    }

    IEnumerator SmoothMovement()
    {
        isMoving = true;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            // Move from start to target over time
            transform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * moveSpeed;
            yield return new WaitForSeconds(1);  // Wait until the next frame
        }

        // Ensure precise snap to target
        transform.position = targetPosition;
        isMoving = false;
    }
}
