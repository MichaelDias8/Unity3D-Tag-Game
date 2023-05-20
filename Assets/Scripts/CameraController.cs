using static Models;
using Unity.Netcode;    
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    private Vector3 targetRotation;
    public GameObject yGimbal;
    private Vector3 yGibalRotation;

    [Header("Settings")]
    public CameraSettingsModel settings;

    #region - Update -

    private void Update()
    {
        CameraRotation();
        FollowPlayerCameraTarget();
    }

    #endregion

    #region - Position / Rotation -

    private void CameraRotation()
    {
        var viewInput = playerController.input_View;

        targetRotation.y += (settings.InvertedX ? -(viewInput.x * settings.SensitivityX) : (viewInput.x * settings.SensitivityX)) * Time.deltaTime;
        transform.rotation = Quaternion.Euler(targetRotation);

        yGibalRotation.x += (settings.InvertedY ? (viewInput.y * settings.SensitivityY) : -(viewInput.y * settings.SensitivityY)) * Time.deltaTime;
        yGibalRotation.x = Mathf.Clamp(yGibalRotation.x, settings.YClampMin, settings.YClampMax);

        yGimbal.transform.localRotation = Quaternion.Euler(yGibalRotation);

        
        var currentRotation = playerController.transform.rotation;
        var newRotation = currentRotation.eulerAngles;
        newRotation.y = targetRotation.y;
        currentRotation = Quaternion.Lerp(currentRotation, Quaternion.Euler(newRotation), settings.CharacterRotationSmoothdamp);
        playerController.transform.rotation = currentRotation;
        
    }

    private void FollowPlayerCameraTarget()
    {
        transform.position = playerController.cameraTarget.position;   
    }

    #endregion
}
