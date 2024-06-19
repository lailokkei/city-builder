using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 10f;
    [SerializeField]
    float zoomSpeed = 10f;
    
    Camera cam;
    
    void Awake() {
        cam = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        var dx = Input.GetAxisRaw("Horizontal");
        var dz = Input.GetAxisRaw("Vertical");
        transform.localPosition += new Vector3(dx, 0, dz) * moveSpeed * cam.orthographicSize * Time.deltaTime;
        

        // Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        // 
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + Input.GetAxisRaw("Mouse ScrollWheel") * zoomSpeed, 1f, float.MaxValue);
        
        
    }
}
