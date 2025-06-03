using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 5000f;                // 이동 속도
    public Vector2 minMaxX = new Vector2(-10, 40); // 카메라 이동 범위

    private Vector3 dragOrigin;

    void Update()
    {
        // PC: 마우스 우클릭 드래그
        if (Input.GetMouseButtonDown(1))
            dragOrigin = Input.mousePosition;

        if (Input.GetMouseButton(1))
        {
            Vector3 diff = Camera.main.ScreenToViewportPoint(dragOrigin - Input.mousePosition);
            Vector3 move = new Vector3(diff.x * panSpeed * Time.deltaTime, 0, 0);
            transform.Translate(move, Space.World);

            // 범위 제한(Clamp)
            float clampedX = Mathf.Clamp(transform.position.x, minMaxX.x, minMaxX.y);
            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

            dragOrigin = Input.mousePosition;
        }
    }
}
