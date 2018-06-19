using EasyBuildSystem.Runtimes.Internal.Addons;
using EasyBuildSystem.Runtimes.Internal.Builder;
using EasyBuildSystem.Runtimes.Events;
using UnityEngine;
using EasyBuildSystem.Runtimes.Internal.Area;
using EasyBuildSystem.Runtimes.Internal.Socket;
using EasyBuildSystem.Runtimes.Internal.Managers;

[AddOn(ADDON_NAME, ADDON_AUTHOR, ADDON_DESCRIPTION, AddOnTarget.BaseBuilder)]
public class AddonAreaOfEffect : AddOnBehaviour
{
    #region AddOn Fields

    public const string ADDON_NAME = "Add-On Area Of Effect";
    public const string ADDON_AUTHOR = "Ads Cryptoz 22";
    public const string ADDON_DESCRIPTION = "Disable all the sockets/areas if out of range, this allow to improve the performances of your scene.";

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

    [Tooltip("This allows to define the radius area effect.")]
    public float Radius = 30f;

    [Tooltip("This allows to define the refresh interval (Default:0.5f).")]
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
        //Show the aoe radius when the component is selected.
        Gizmos.DrawWireSphere(transform.position, Radius);
    }

    private void OnModeChanged(BuildMode mode)
    {        
        //We make sure that the builder behaviour instance is not null.
        if (BuilderBehaviour.Instance == null)
            return;

        //If the current mode is None then we disable all the sockets also that the areas.
        if (BuilderBehaviour.Instance.CurrentMode == BuildMode.None)
        {
            foreach (AreaBehaviour Area in BuildManager.Instance.Areas)
                Area.gameObject.SetActive(false);

            foreach (SocketBehaviour Socket in BuildManager.Instance.Sockets)
                if (Socket != null)
                    Socket.DisableCollider();
        }
        //If the current mode is Placement/Edition then we active only the socket of same type that the current selected preview type.
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
                            Socket.EnableColliderByType(BuilderBehaviour.Instance.SelectedPrefab.Type);
                    }
                    else
                        Socket.DisableCollider();
                }
            }
        }
    }

    #endregion
}