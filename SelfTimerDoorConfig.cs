using TUNING;
using UnityEngine;

public class SelfTimerPneumaticDoorConfig : IBuildingConfig
{
    public const string ID = "SelfTimerPneumaticDoor";

    public override BuildingDef CreateBuildingDef()
    {
        string id = ID;
        int width = 1;
        int height = 2;
        string anim = "door_internal_kanim";
        int hitpoints = 30;
        float construction_time = 10f;
        float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER1;
        string[] metals = MATERIALS.ALL_METALS;
        float melting_point = 1600f;
        BuildLocationRule build_location_rule = BuildLocationRule.Tile;
        EffectorValues none = NOISE_POLLUTION.NONE;

        BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tier, metals, melting_point, build_location_rule, BUILDINGS.DECOR.NONE, none, 1f);

        buildingDef.Entombable = true;
        buildingDef.Floodable = false;
        buildingDef.IsFoundation = false;
        buildingDef.AudioCategory = "Metal";
        buildingDef.PermittedRotations = PermittedRotations.R90;
        buildingDef.SceneLayer = Grid.SceneLayer.Building;
        buildingDef.ForegroundLayer = Grid.SceneLayer.InteriorWall;

        // We do NOT add logic input/output ports here because this is internally controlled.
        return buildingDef;
    }

    public override void DoPostConfigureComplete(GameObject go)
    {
        // Standard Door setup
        Door door = go.AddOrGet<Door>();
        door.hasComplexUserControls = true; // Activates the default Door UI (Open/Auto/Lock)
        door.unpoweredAnimSpeed = 1f; 
        door.doorType = Door.DoorType.Internal; // Identifies it as a gas-permeable pneumatic door

        go.AddOrGet<ZoneTile>();
        go.AddOrGet<AccessControl>();
        go.AddOrGet<KBoxCollider2D>();

        Prioritizable.AddRef(go);

        go.AddOrGet<Workable>().workTime = 3f;
        go.AddOrGet<KBatchedAnimController>().initialAnim = "closed";

        // Inject our custom integrated timer
        go.AddOrGet<SelfTimerDoor>();
    }
}