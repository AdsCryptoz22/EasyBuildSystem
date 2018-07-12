#if VIS2K

using EasyBuildSystem.Runtimes.Internal.Builder;
using EasyBuildSystem.Runtimes.Internal.Part;
using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName = "uSurvival Item/(Easy Build System) Building", order = 999)]
public class BuildingItem : WeaponItem
{
    #region Public Fields

    [Header("Building")]
    public PartBehaviour buildingPart;

    #endregion

    #region Public Methods

    public override Usability CanUse(PlayerInventory inventory, int inventoryIndex)
    {
        return Usability.Usable;
    }

    public override Usability CanUse(PlayerHotbar hotbar, int hotbarIndex, Vector3 lookAt)
    {
        return Usability.Usable;
    }

    public override void Use(PlayerInventory inventory, int inventoryIndex)
    {
        inventory.GetComponent<PlayerBuilding>().ChangeMode(inventory, inventoryIndex, BuildMode.Placement, buildingPart);
    }

    public override void Use(PlayerHotbar hotbar, int hotbarIndex, Vector3 lookAt)
    {
        hotbar.GetComponent<PlayerBuilding>().ChangeMode(hotbar, hotbarIndex, BuildMode.Placement, buildingPart);
    }

    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        tip.Replace("{BUILDING_NAME}", buildingPart.Name.ToString());
        return tip.ToString();
    }

    #endregion
}

#endif