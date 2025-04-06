using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 10, -2);

    private bool shouldFollow = false;

    private void OnEnable()
    {
        GameManager.OnGameStart += EnableFollowing;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= EnableFollowing;
    }

    private void EnableFollowing() => shouldFollow = true;

    private void Update()
    {
        if (!shouldFollow || target == null) return;
        
        Vector3 pos = target.position + offset;
        Vector3 smoothedPos = Vector3.Lerp(transform.position, pos, smoothSpeed);
        transform.position = smoothedPos;
    }
}
