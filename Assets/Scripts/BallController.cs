using UnityEngine;

public class BallController : MonoBehaviour
{
    public float moveSpeed = 10f;

    private Transform m_transform;
    private Vector3 m_inputAxis;
    private Vector3 m_position;

    private void Awake()
    {
        m_transform = transform;
    }

    private void Update()
    {
        m_inputAxis.x = Input.GetAxis("Horizontal");
        m_inputAxis.z = Input.GetAxis("Vertical");
        m_inputAxis.Normalize();
        m_transform.Translate(m_inputAxis * moveSpeed * Time.deltaTime);
        m_position = m_transform.position;
        m_transform.position = m_position;
    }
}
