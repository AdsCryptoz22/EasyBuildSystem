#if VIS2K
using EasyBuildSystem.Runtimes.Internal.Addons;
using EasyBuildSystem.Runtimes.Internal.Builder;
using UnityEngine;

[AddOn(ADDON_NAME, ADDON_AUTHOR, ADDON_DESCRIPTION, AddOnTarget.BuilderBehaviour)]
public class AddonUSurvival : AddOnBehaviour
{
    #region AddOn Fields

    public const string ADDON_NAME = "Add-On uSurvival";
    public const string ADDON_AUTHOR = "AdsCryptoz22";
    public const string ADDON_DESCRIPTION = "This add-on allow to switch of Builder Behaviour according the uSurvival camera.";

    [HideInInspector]
    public string _Name = ADDON_NAME;

    public override string Name
    {
        get
        {
            return _Name;
        }

        protected set
        {
            _Name = value;
        }
    }

    [HideInInspector]
    public string _Author = ADDON_AUTHOR;

    public override string Author
    {
        get
        {
            return _Author;
        }

        protected set
        {
            _Author = value;
        }
    }

    [HideInInspector]
    public string _Description = ADDON_DESCRIPTION;

    public override string Description
    {
        get
        {
            return _Description;
        }

        protected set
        {
            _Description = value;
        }
    }

    #endregion AddOn Fields

    #region Public Fields

    [Header("First Person uSurvival Settings")]
    public RayType FirstCameraType = RayType.FirstPerson;

    public float FirstPlacementDistance = 6;
    public float FirstOverlapDetection = 45;

    [Header("Third Person uSurvival Settings")]
    public RayType ThirdCameraType = RayType.ThirdPerson;

    public float ThirdPlacementDistance = 10;
    public float ThirdOverlapDetection = 10;
    public Transform ThirdOriginTransform;

    #endregion Public Fields

    #region Private Fields

    private PlayerMovement Movement;

    #endregion Private Fields

    #region Private Methods

    private void Update()
    {
        if (transform.parent != null)
            Movement = GetComponentInParent<PlayerMovement>();

        if (Movement == null) return;

        bool IsFirstPerson = Movement.look.distance == 0;

        if (IsFirstPerson)
        {
            BuilderBehaviour.Instance.CameraType = FirstCameraType;
            BuilderBehaviour.Instance.ActionDistance = FirstPlacementDistance;
            BuilderBehaviour.Instance.OverlapAngles = FirstOverlapDetection;
        }
        else
        {
            BuilderBehaviour.Instance.CameraType = ThirdCameraType;
            BuilderBehaviour.Instance.ActionDistance = ThirdPlacementDistance;
            BuilderBehaviour.Instance.OverlapAngles = ThirdOverlapDetection;
            BuilderBehaviour.Instance.RaycastOriginTransform = ThirdOriginTransform;
        }
    }

    #endregion Private Methods
}
#endif