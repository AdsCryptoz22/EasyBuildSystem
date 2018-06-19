using EasyBuildSystem.Runtimes;
using EasyBuildSystem.Runtimes.Internal.Addons;
using EasyBuildSystem.Runtimes.Internal.Builder;
using EasyBuildSystem.Runtimes.Internal.Part;
using EasyBuildSystem.Runtimes.Events;
using EasyBuildSystem.Runtimes.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[AddOn(ADDON_NAME, ADDON_AUTHOR, ADDON_DESCRIPTION, AddOnTarget.BasePart)]
public class AddonDetachChildrens : AddOnBehaviour
{
    #region AddOn Fields

    public const string ADDON_NAME = "Add-On Detach Childrens";
    public const string ADDON_AUTHOR = "R.Andrew";
    public const string ADDON_DESCRIPTION = "Spawn gameObject(s) when the placement/destruction/edition is performed.";

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

    [Tooltip("This allows to add a rigidobdy to each childs when destroyed.")]
    public bool AddDynamicRigidbody;

    [Tooltip("This allows to define the max depenetration velocity when destroyed.")]
    public float MaxDepenetrationVelocity = 2f;

    [Tooltip("This allows to add a box collider to each childs when destroyed.")]
    public bool AddDynamicBoxCollider;

    public float LifeTime = 10f;

    [HideInInspector]
    public PartBehaviour Part;

    #endregion

    #region Private Methods

    private void OnEnable()
    {
        EventHandlers.OnDestroyedPart += OnDestroyedPart;
    }

    private void OnDisable()
    {
        EventHandlers.OnDestroyedPart -= OnDestroyedPart;
    }
    
    private void Awake()
    {
        //Get the Base Part component in this object.
        Part = GetComponent<PartBehaviour>();
    }

    private bool IsExiting;

    private void OnApplicationQuit()
    {
        IsExiting = true;
    }

    private void OnDestroy()
    {
        //If exiting we return.
        if (IsExiting)
            return;

        //We make sure that the part has the state "Placed" otherwise we return.
        if (Part.CurrentState != StateType.Placed)
            return;

        Detach();
    }

    private void OnDestroyedPart(PartBehaviour part)
    {
        if (part != Part)
            return;

        if (!Part.CheckStability())
            Detach();
    }

    private void Detach()
    {
        //We get all the renderers in the children.
        List<Renderer> Renderers = GetComponentsInChildren<Renderer>(true).ToList();

        for (int i = 0; i < Renderers.Count; i++)
        {
            if (Renderers[i].gameObject == null || !Renderers[i].gameObject.activeSelf)
                return;

            if (AddDynamicRigidbody)
            {
                Renderers[i].gameObject.AddRigibody(true, false, 1f);

                Renderers[i].gameObject.GetComponent<Rigidbody>().maxDepenetrationVelocity = MaxDepenetrationVelocity;
            }

            if (AddDynamicBoxCollider)
                Renderers[i].gameObject.AddComponent<BoxCollider>();

            //Detach to parent.
            Renderers[i].transform.parent = null;

            //Destroy according life time.
            Destroy(Renderers[i].gameObject, LifeTime);
        }
    }

    #endregion
}