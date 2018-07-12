using EasyBuildSystem.Runtimes.Internal.Addons;
using UnityEngine;

[AddOn(ADDON_NAME, ADDON_AUTHOR, ADDON_DESCRIPTION, AddOnTarget.None)]
public class AddonTemplate : AddOnBehaviour
{
    #region AddOn Fields

    public const string ADDON_NAME = "Add-On Template";
    public const string ADDON_AUTHOR = "R.Andrew";
    public const string ADDON_DESCRIPTION = "This Add-On is just a template.";

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
}