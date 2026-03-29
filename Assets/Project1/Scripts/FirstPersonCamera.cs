using UnityEngine;
using Unity.Netcode;

public class FirstPersonCamera : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float cameraDistance = 0f;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0.6f, 0);
    
    [Header("Camera Limits")]
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;

    //syncs horizontal rotation across network
    private NetworkVariable<float> networkYRotation = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;

    void Awake()
    {
        if (cameraHolder == null)
        {
            GameObject holder = new GameObject("CameraHolder");
            cameraHolder = holder.transform;
            cameraHolder.SetParent(transform);
            cameraHolder.localPosition = cameraOffset;
        }

        if (playerCamera == null)
        {
            GameObject camObj = new GameObject("PlayerCamera");
            camObj.transform.SetParent(cameraHolder);
            camObj.transform.localPosition = Vector3.zero;
            playerCamera = camObj.AddComponent<Camera>();
            playerCamera.enabled = false;
            
            AudioListener listener = camObj.AddComponent<AudioListener>();
            listener.enabled = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //only local player has active camera
        if (IsLocalPlayer)
        {
            if (playerCamera != null)
            {
                playerCamera.enabled = true;
                playerCamera.gameObject.tag = "MainCamera";
                
                AudioListener listener = playerCamera.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = true;
                }
                
                Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                foreach (Camera cam in allCameras)
                {
                    if (cam != playerCamera && cam.gameObject.scene.name != null)
                    {
                        cam.gameObject.SetActive(false);
                    }
                }

            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            if (playerCamera != null)
            {
                playerCamera.enabled = false;
                playerCamera.gameObject.tag = "Untagged";
                
            }
            
            AudioListener listener = GetComponentInChildren<AudioListener>();
            if (listener != null)
            {
                listener.enabled = false;
            }
        }

        //subscribe to networked rotation changes
        networkYRotation.OnValueChanged += OnRotationChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        networkYRotation.OnValueChanged -= OnRotationChanged;
    }

    //callback when rotation changes on network
    private void OnRotationChanged(float previousValue, float newValue)
    {
        //non owners update their rotation from network
        if (!IsOwner)
        {
            horizontalRotation = newValue;
            transform.rotation = Quaternion.Euler(0, horizontalRotation, 0);
        }
    }

    void Update()
    {
        if (!IsLocalPlayer) return;

        HandleMouseLook();
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleCursorLock();
        }
    }

    void LateUpdate()
    {
        if (!IsLocalPlayer) return;

        UpdateCameraPosition();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        horizontalRotation += mouseX;
        transform.rotation = Quaternion.Euler(0, horizontalRotation, 0);

        //owner updates network variable for other clients to see
        if (IsOwner)
        {
            networkYRotation.Value = horizontalRotation;
        }

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);
        
        if (cameraHolder != null)
        {
            cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        }
    }

    private void UpdateCameraPosition()
    {
        if (cameraHolder == null) return;

        cameraHolder.localPosition = cameraOffset;

        if (playerCamera != null)
        {
            playerCamera.transform.localPosition = new Vector3(0, 0, -cameraDistance);
        }
    }

    private void ToggleCursorLock()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public Camera GetCamera()
    {
        return playerCamera;
    }


}
