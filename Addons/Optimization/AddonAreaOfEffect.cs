using EasyBuildSystem.Runtimes.Internal.Addons;
using EasyBuildSystem.Runtimes.Internal.Builder;
using EasyBuildSystem.Runtimes.Events;
using UnityEngine;
using EasyBuildSystem.Runtimes.Internal.Area;
using EasyBuildSystem.Runtimes.Internal.Socket;
using EasyBuildSystem.Runtimes.Internal.Managers;

[AddOn(ADDON_NAME, ADDON_AUTHOR, ADDON_DESCRIPTION, AddOnTarget.BuilderBehaviour)]
public class AddonAreaOfEffect : AddOnBehaviour
{
    #region AddOn Fields

    public const string ADDON_NAME = "(AdsStudio12) Area Of Effect";
    public const string ADDON_AUTHOR = "AdsCryptoz22";
    public const string ADDON_DESCRIPTION = "This add-on allow to disable all the sockets/areas if out of range, this allow to improve the performances of your scene.";

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

    [Header("Area Of Effects Settings")]

    [Tooltip("Define here the radius area effect.")]
    public float Radius = 30f;

    [Tooltip("Define here the refresh interval (Default:0.5f).")]
    public float RefreshInterval = 0.5f;

    #endregion

    #region Private Methods

    private void OnEnable()
    {
        EventHandlers.OnBuildModeChanged += OnModeChanged;
    }

    private void OnDisable()
    {
        EventHandlers.OnBuildModeChanged -= OnModeChanged;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, Radius);
    }

    private void OnModeChanged(BuildMode mode)
    {        
        if (BuilderBehaviour.Instance == null)
            return;

        if (BuilderBehaviour.Instance.CurrentMode == BuildMode.None)
        {
            foreach (AreaBehaviour Area in BuildManager.Instance.Areas)
                Area.gameObject.SetActive(false);

            foreach (SocketBehaviour Socket in BuildManager.Instance.Sockets)
                if (Socket != null)
                    Socket.DisableCollider();
        }
        else if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Placement || BuilderBehaviour.Instance.CurrentMode == BuildMode.Edition)
        {
            foreach (AreaBehaviour Area in BuildManager.Instance.Areas)
                Area.gameObject.SetActive((Vector3.Distance(transform.position, Area.transform.position) <= Radius));

            foreach (SocketBehaviour Socket in BuildManager.Instance.Sockets)
            {
                if (Socket != null)
                {
                    if (Vector3.Distance(transform.position, Socket.transform.position) <= Radius)
                    {
                        if (Socket.AttachedPart != null)
                            Socket.EnableColliderByPartType(BuilderBehaviour.Instance.SelectedPrefab.Type);
                    }
                    else
                        Socket.DisableCollider();
                }
            }
        }
    }

    #endregion
}