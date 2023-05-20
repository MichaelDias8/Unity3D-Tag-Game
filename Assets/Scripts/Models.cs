using System;
using UnityEngine;

public static class Models
{
    #region - Player -

    [Serializable]
    public class CameraSettingsModel
    {
        [Header("Camera Settings")]
        public float SensitivityX;
        public bool InvertedX;
        public float SensitivityY;
        public bool InvertedY;

        public float YClampMin = -40f;
        public float YClampMax = 40f;

        [Header("Character")]
        public float CharacterRotationSmoothdamp = 1f;
    }

    [Serializable]
    public class PlayerSettingsModel
    {
        public float CharacterRotationSmoothdamp = 0.6f;

        [Header("Movement Speeds")]
        public float WalkingSpeed;
        public float RunningSpeed;
        public float SprintingSpeed;

        public float WalkingBackwardSpeed;
        public float RunningBackwardSpeed;
        public float SprintingBackwardSpeed;

        public float WalkingStrafingSpeed;
        public float RunningStrafingSpeed;
        public float SprintingStrafingSpeed;

        [Header("Jumping")]
        public float JumpingForce;
    }

    [Serializable]
    public class PlayerInfoModel {
        public float boost;
        public float BoostDrain;
        public float BoostRegen;
        public int darts;
    }

    #endregion
}
