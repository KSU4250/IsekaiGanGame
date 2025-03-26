using UnityEngine;

public class TestTransform : MonoBehaviour
{
    public Transform target;

    private void Start()
    {
        transform.position = target.position + target.forward * 2f;
        transform.rotation = target.localRotation;
    }
}
