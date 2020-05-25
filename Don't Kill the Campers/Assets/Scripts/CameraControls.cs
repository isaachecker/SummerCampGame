using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    private new Camera camera;
    public float moveSpeed = 5;
    public float sizeAdjustIncrement = 5;
    public float maxCamDistance = 25;

    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow)) SetFurther();
        if (Input.GetKeyDown(KeyCode.UpArrow)) SetCloser();
        MoveScreen();
    }

    private void SetFurther()
    {
        camera.orthographicSize = Mathf.Min(camera.orthographicSize + sizeAdjustIncrement, maxCamDistance);
    }

    private void SetCloser()
    {
        camera.orthographicSize = Mathf.Max(camera.orthographicSize - sizeAdjustIncrement, 5);
    }

    private void MoveScreen()
    {
        Vector3 pos = camera.transform.position;
        float speed = moveSpeed * Time.deltaTime;
        if (Input.mousePosition.y >= Screen.height * 0.98)
        {
            pos.y += speed;
        }
        if (Input.mousePosition.y <= Screen.height * 0.02)
        {
            pos.y -= speed;
        }
        if (Input.mousePosition.x >= Screen.width * 0.98)
        {
            pos.x += speed;
        }
        if (Input.mousePosition.x <= Screen.width * 0.02)
        {
            pos.x -= speed;
        }
        transform.position = pos;
    }
}
