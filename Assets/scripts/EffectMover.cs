using UnityEngine;

public class EffectMover : MonoBehaviour
{
    public float speed = 10f;
    public Vector3 direction;
    public float lifetime = 3f;
    public float maxDistance = 5f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }
}