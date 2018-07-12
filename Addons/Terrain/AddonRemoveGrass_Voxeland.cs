#if VOXELAND
using EasyBuildSystem.Runtimes.Events;
using EasyBuildSystem.Runtimes.Internal.Addons;
using EasyBuildSystem.Runtimes.Internal.Part;
using EasyBuildSystem.Runtimes.Internal.Socket;
using System.Collections;
using UnityEngine;
using Voxeland5;

[AddOn(ADDON_NAME, ADDON_AUTHOR, ADDON_DESCRIPTION, AddOnTarget.PartBehaviour)]
public class AddonRemoveGrass_Voxeland : AddOnBehaviour
{
    #region AddOn Fields

    public const string ADDON_NAME = "(AdsStudio12) Remove Grass (Voxeland)";
    public const string ADDON_AUTHOR = "R.Andrew";

    public const string ADDON_DESCRIPTION = "This add-on allow to remove the grass at the position where the placement is performed.\n" +
                "Require : Voxeland integration.\n";

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

    [Header("(Voxeland) Remove Grass Settings")]

    [Tooltip("Allows to remove the grass at placement.")]
    public bool RemoveGrass = true;

    [Tooltip("Allows to remove the grass at scene starting (useful for the pre-placed prefabs).")]
    public bool RemoveOnAwake = true;

    [Tooltip("Allows to define the grass removing radius.")]
    public float RemoveGrassRadius = 5.0f;

    #endregion Public Fields

    #region Private Methods

    private Voxeland Terrain;

    #endregion Private Methods

    #region Private Methods

    private void Awake()
    {
        Terrain = FindObjectOfType<Voxeland>();
    }

    private void Start()
    {
        if (GetComponent<PartBehaviour>().CurrentState == StateType.Placed)
            if (RemoveGrass)
                StartCoroutine(RemoveGrassToPosition(transform.position, RemoveGrassRadius));
    }

    private void OnEnable()
    {
        EventHandlers.OnPlacedPart += OnPartPlaced;
    }

    private void OnDisable()
    {
        EventHandlers.OnPlacedPart -= OnPartPlaced;
    }

    private void OnPartPlaced(PartBehaviour part, SocketBehaviour socket)
    {
        if (part.GetComponent<AddonRemoveGrass>() == null)
            return;

        if (RemoveGrass)
            StartCoroutine(RemoveGrassToPosition(part.transform.position, RemoveGrassRadius));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(transform.position, RemoveGrassRadius);
    }

    #endregion Private Methods

    #region Public Methods

    public IEnumerator RemoveGrassToPosition(Vector3 position, float radius)
    {
        if (Terrain == null)
            yield break;

        CoordDir Dir = Terrain.PointOut(new Ray(position, Vector3.down * 2));

        Terrain.brush.form = Brush.Form.blob;
        Terrain.brush.round = false;
        Terrain.brush.extent = Mathf.RoundToInt(radius / 2);
        Terrain.standardEditMode = Voxeland.EditMode.dig;
        Terrain.grassTypes.selected = 1;

        Terrain.Alter(Dir, Terrain.brush, Voxeland.EditMode.dig,
                            landType: Terrain.landTypes.selected,
                            objectType: Terrain.objectsTypes.selected,
                            grassType: Terrain.grassTypes.selected);

        yield break;
    }

    #endregion Public Methods
}
#endif