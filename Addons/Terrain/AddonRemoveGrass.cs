using EasyBuildSystem.Runtimes.Events;
using EasyBuildSystem.Runtimes.Internal.Addons;
using EasyBuildSystem.Runtimes.Internal.Managers;
using EasyBuildSystem.Runtimes.Internal.Part;
using EasyBuildSystem.Runtimes.Internal.Socket;
using EasyBuildSystem.Runtimes.Internal.Terrain;
using System.Collections;
using UnityEngine;

[AddOn(ADDON_NAME, ADDON_AUTHOR, ADDON_DESCRIPTION, AddOnTarget.PartBehaviour)]
public class AddonRemoveGrass : AddOnBehaviour
{
    #region AddOn Fields

    public const string ADDON_NAME = "(AdsStudio12) Remove Grass";
    public const string ADDON_AUTHOR = "R.Andrew";

    public const string ADDON_DESCRIPTION = "This add-on allow to remove the grass at the position where the placement is performed.\n" +
                "This operation require a lot of performances according to terrain size.\n" +
                "This use the TerrainManager who allows to restore the modifications on the terrain on closing scene.";

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

    [Header("Remove Grass Settings")]

    [Tooltip("Allows to remove the grass at placement.")]
    public bool RemoveGrass = true;

    [Tooltip("Allows to remove the grass at scene starting (useful for the pre-placed prefabs).")]
    public bool RemoveOnAwake = true;

    [Tooltip("Allows to define the grass removing radius.")]
    public float RemoveGrassRadius = 5.0f;

    #endregion Public Fields

    #region Private Fields

    private TerrainManager Terrain;

    #endregion Private Fields

    #region Private Methods

    private void Awake()
    {
        if (BuildManager.Instance.BuildingSupport == SupportType.Terrain)
            TerrainManager.Initialize();
    }

    private void Start()
    {
        if (BuildManager.Instance.BuildingSupport != SupportType.Terrain)
            return;

        if (GetComponent<PartBehaviour>().CurrentState == StateType.Placed)
        {
            if (RemoveGrass)
            {
                if (!TerrainManager.Instance.CheckDetailtAt(transform.position, RemoveGrassRadius))
                    return;

                StartCoroutine(RemoveGrassToPosition(transform.position, RemoveGrassRadius));
            }
        }
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
        if (BuildManager.Instance.BuildingSupport != SupportType.Terrain)
            return;

        if (part.GetComponent<AddonRemoveGrass>() == null)
            return;

        if (RemoveGrass)
        {
            if (!TerrainManager.Instance.CheckDetailtAt(transform.position, RemoveGrassRadius))
                return;

            StartCoroutine(RemoveGrassToPosition(part.transform.position, RemoveGrassRadius));
        }
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
        if (TerrainManager.Instance == null)
            yield break;

        Terrain ActiveTerrain = TerrainManager.Instance.ActiveTerrain;

        if (ActiveTerrain == null)
            yield break;

        for (int Layer = 0; Layer < ActiveTerrain.terrainData.detailPrototypes.Length; Layer++)
        {
            int TerrainDetailMapSize = ActiveTerrain.terrainData.detailResolution;

            if (ActiveTerrain.terrainData.size.x != ActiveTerrain.terrainData.size.z)
                yield break;

            float DetailSize = TerrainDetailMapSize / ActiveTerrain.terrainData.size.x;

            Vector3 TexturePoint3D = position - ActiveTerrain.transform.position;

            TexturePoint3D = TexturePoint3D * DetailSize;

            float[] Matrix = new float[4];
            Matrix[0] = TexturePoint3D.z + radius;
            Matrix[1] = TexturePoint3D.z - radius;
            Matrix[2] = TexturePoint3D.x + radius;
            Matrix[3] = TexturePoint3D.x - radius;

            int[,] Data = ActiveTerrain.terrainData.GetDetailLayer(0, 0, ActiveTerrain.terrainData.detailWidth, ActiveTerrain.terrainData.detailHeight, Layer);

            for (int y = 0; y < ActiveTerrain.terrainData.detailHeight; y++)
            {
                for (int x = 0; x < ActiveTerrain.terrainData.detailWidth; x++)
                {
                    if (Matrix[0] > x && Matrix[1] < x && Matrix[2] > y && Matrix[3] < y)
                    {
                        Data[x, y] = 0;
                    }
                }
            }

            ActiveTerrain.terrainData.SetDetailLayer(0, 0, Layer, Data);
        }
    }

    #endregion Public Methods
}