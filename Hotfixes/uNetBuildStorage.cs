using EasyBuildSystem.Runtimes.Events;
using EasyBuildSystem.Runtimes.Internal.Managers;
using EasyBuildSystem.Runtimes.Internal.Part;
using EasyBuildSystem.Runtimes.Internal.Storage;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(NetworkIdentity))]
public class uNetBuildStorage : NetworkBehaviour
{
    #region Public Class

    [System.Serializable]
    public class EntityData
    {
        #region Private Fields

        public int Id;
        public int InstanceId;
        public int AppearanceIndex;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;

        #endregion Private Fields

        #region Public Methods

        public EntityData()
        {
        }

        public EntityData(int id, int instanceId, int appearanceIndex, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Id = id;
            InstanceId = instanceId;
            AppearanceIndex = appearanceIndex;
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        #endregion Public Methods
    }

    #endregion

    #region Public Fields

    public static uNetBuildStorage Instance;

    public BuildStorage Storage;

    #endregion Public Fields

    #region Private Fields

    private uNetHandlers Handlers;

    #endregion Private Fields

    #region Private Methods

    private void Awake()
    {
        Instance = this;

        Handlers = FindObjectOfType<uNetHandlers>();

        if (Storage != null)
        {
            Storage.LoadPrefabs = false;
            Storage.SavePrefabs = false;
        }
    }

    private void OnEnable()
    {
        EventHandlers.OnStorageLoadingDone += OnStorageLoadingDone;
    }

    private void OnDisable()
    {
        EventHandlers.OnStorageLoadingDone -= OnStorageLoadingDone;
    }

    private void Start()
    {
        if (BuildManager.Instance.PartsCollection == null)
        {
            enabled = false;

            return;
        }

        foreach (PartBehaviour Part in BuildManager.Instance.PartsCollection.Parts)
        {
            if (Part == null)
                return;

            ClientScene.RegisterPrefab(Part.gameObject);
        }

        if (isServer)
        {
            if (Storage != null)
            {
                Storage.LoadPrefabs = true;
                Storage.SavePrefabs = true;

                Storage.LoadStorageFile();
            }
        }
    }

    private void OnStorageLoadingDone(PartBehaviour[] parts)
    {
        if (!isServer)
        {
            return;
        }

        foreach (PartBehaviour Part in parts)
        {
            if (Handlers != null)
            {
                Handlers.AddNetworkEntity(new EntityData(Part.Id, Part.EntityInstanceId, Part.AppearanceIndex, Part.transform.position
                    , Part.transform.rotation, Part.transform.localScale));

                RpcPrefabPlacement(Part.Id, Part.EntityInstanceId, Part.transform.position, Part.transform.rotation, Part.transform.localScale);
            }
        }
    }

    #endregion Private Methods

    #region Public Methods

    #region Client

    [ClientRpc]
    public void RpcPrefabPlacement(int prefabId, int instanceId, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        if (isServer)
        {
            return;
        }

        PartBehaviour PrefabTemp = BuildManager.Instance.GetPart(prefabId);

        PrefabTemp.EntityInstanceId = instanceId;

        GameObject PrefabPlaced = Instantiate(PrefabTemp.gameObject, position, rotation);

        PrefabPlaced.transform.localScale = scale;
    }

    #endregion Client

    #endregion Public Methods
}