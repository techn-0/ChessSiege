using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 5000f;                // 이동 속도
    public Vector2 minMaxX = new Vector2(-10, 40); // 카메라 이동 범위

    private Vector3 dragOrigin;
    private bool isDragging = false;

    void Update()
    {
        // PC: 마우스 우클릭 드래그
        if (Input.GetMouseButtonDown(1))
        {
            dragOrigin = Input.mousePosition;
            isDragging = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }
        if (isDragging && Input.GetMouseButton(1))
        {
            Vector3 diff = Camera.main.ScreenToViewportPoint(dragOrigin - Input.mousePosition);
            Vector3 move = new Vector3(diff.x * panSpeed * Time.deltaTime, 0, 0);
            transform.Translate(move, Space.World);

            float clampedX = Mathf.Clamp(transform.position.x, minMaxX.x, minMaxX.y);
            transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

            dragOrigin = Input.mousePosition;
        }

        // 모바일: 한 손가락 드래그
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                dragOrigin = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector3 diff = Camera.main.ScreenToViewportPoint(dragOrigin - (Vector3)touch.position);
                Vector3 move = new Vector3(diff.x * panSpeed * Time.deltaTime, 0, 0);
                transform.Translate(move, Space.World);

                float clampedX = Mathf.Clamp(transform.position.x, minMaxX.x, minMaxX.y);
                transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

                dragOrigin = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
    }
}
