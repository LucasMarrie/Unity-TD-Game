using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10f;
    public float mouseSensitivity = 5f;
    public Transform cam;
    CharacterController controller;
    float verticalAngle = 0;
    float x;
    float z;
    float y;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        MovePlayer();
    }

    void GetInput(){
        x = Input.GetAxisRaw("Horizontal");
        z = Input.GetAxisRaw("Vertical");
        if(Input.GetButton("Jump")){
            y = 1;
        }else if(Input.GetButton("Crouch")){
            y = -1;
        }else{
            y = 0;
        }

        if(Input.GetButton("Fire3")){
            Cursor.lockState = CursorLockMode.Confined;
            transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivity);
            verticalAngle -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            verticalAngle = Mathf.Clamp(verticalAngle, -90f, 90f);
            cam.localRotation = Quaternion.Euler(Vector3.right * verticalAngle);
        }else{
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void MovePlayer()
    {
        Vector3 movementVector = (transform.forward * z + transform.up * y + transform.right * x) * speed; 
        controller.Move(movementVector * Time.deltaTime);
    }
}
