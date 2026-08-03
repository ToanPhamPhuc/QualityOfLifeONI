using System;
using KSerialization;
using UnityEngine;

[SerializationConfig(MemberSerialization.OptIn)]
[AddComponentMenu("KMonoBehaviour/scripts/SelfTimerDoor")]
public class SelfTimerDoor : Switch, ISaveLoadable, ISim200ms
{
    [MyCmpReq]
    private Door door;

    [MyCmpAdd]
    private CopyBuildingSettings copyBuildingSettings;

    // These variables match the native cycle sensor
    [Serialize]
    [SerializeField]
    public float startTime;

    [Serialize]
    [SerializeField]
    public float duration = 1f;

    // Handles copying settings between doors
    private static readonly EventSystem.IntraObjectHandler<SelfTimerDoor> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<SelfTimerDoor>(delegate (SelfTimerDoor component, object data)
    {
        component.OnCopySettings(data);
    });

    protected override void OnPrefabInit()
    {
        base.OnPrefabInit();
        base.Subscribe<SelfTimerDoor>(-905833192, SelfTimerDoor.OnCopySettingsDelegate); 
    }

    private void OnCopySettings(object data)
    {
        SelfTimerDoor component = ((GameObject)data).GetComponent<SelfTimerDoor>(); 
        if (component != null)
        {
            this.startTime = component.startTime;
            this.duration = component.duration; 
        }
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        // Subscribe to the toggle event so we can trigger the door
        base.OnToggle += this.OnSwitchToggled;
    }

    public void Sim200ms(float dt)
    {
        // Replicate the exact cycle calculation from LogicTimeOfDaySensor
        float currentCycleAsPercentage = GameClock.Instance.GetCurrentCycleAsPercentage();
        bool state = false; 
        
        if (currentCycleAsPercentage >= this.startTime && currentCycleAsPercentage < this.startTime + this.duration)
        {
            state = true; 
        }
        if (currentCycleAsPercentage < this.startTime + this.duration - 1f) 
        {
            state = true; 
        }

        this.SetState(state); 
    }

    private void OnSwitchToggled(bool toggled_on)
    {
        if (door != null)
        {
            // Translates the timer's "Active/Inactive" state into Door "Opened/Locked" states
            Door.ControlState newState = toggled_on ? Door.ControlState.Opened : Door.ControlState.Locked; 
            door.QueueStateChange(newState); 
        }
    }
}