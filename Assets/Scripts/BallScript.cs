using UnityEngine;

public class BallScript : MonoBehaviour
{
    public Rigidbody rb;
    public float speed = 4;
    public GameManager gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<GameManager>();
    }

    public void FireBall(Vector3 direction, float power, float maxOffset)
    {
        rb.AddForce(direction * power, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other)
    {
        //if the Ball hits the Racket
        if (other.gameObject.CompareTag("Player"))
        {
            gameManager.AddPoint();
            Destroy(this.gameObject);
        }
        else
        {
            gameManager.Miss();
        }
    }
}
