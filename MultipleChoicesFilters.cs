/*using HarmonyLib;
using KSerialization;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace QualityOfLifeONI
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class MultiElementFilter : KMonoBehaviour
    {
        [MyCmpReq]
        private Building building;

        [MyCmpReq]
        private TreeFilterable treeFilterable;

        [MyCmpReq]
        private Operational operational;

        private int inputCell;
        private int outputCell;
        private int filteredCell;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            // Cache primary ports
            inputCell = building.GetUtilityInputCell();
            outputCell = building.GetUtilityOutputCell();

            // Cache secondary orange filter port cell
            ISecondaryOutput secondaryOutput = GetComponent<ISecondaryOutput>();
            if (secondaryOutput != null)
            {
                CellOffset offset = secondaryOutput.GetSecondaryConduitOffset(building.Def.InputConduitType);
                filteredCell = Grid.OffsetCell(Grid.PosToCell(transform.GetPosition()), offset);
            }
            else
            {
                filteredCell = outputCell;
            }

            // Register tick updater with the conduit network
            ConduitFlow flow = GetConduitFlow();
            if (flow != null)
            {
                flow.AddConduitUpdater(OnConduitTick, ConduitFlowPriority.Default);
            }
        }

        protected override void OnCleanUp()
        {
            ConduitFlow flow = GetConduitFlow();
            if (flow != null)
            {
                flow.RemoveConduitUpdater(OnConduitTick);
            }
            base.OnCleanUp();
        }

        private ConduitFlow GetConduitFlow()
        {
            switch (building.Def.InputConduitType)
            {
                case ConduitType.Gas:
                    return Game.Instance.gasConduitFlow;
                case ConduitType.Liquid:
                    return Game.Instance.liquidConduitFlow;
                default:
                    return null;
            }
        }

        private void OnConduitTick(float dt)
        {
            if (!operational.IsOperational)
                return;

            ConduitFlow flow = GetConduitFlow();
            if (flow == null)
                return;

            ConduitFlow.ConduitContents contents = flow.GetContents(inputCell);
            if (contents.mass <= 0f)
                return;

            // Check if element matches any allowed tags in TreeFilterable
            Element element = ElementLoader.FindElementByHash(contents.element);
            bool isMatch = false;

            if (element != null && treeFilterable.AcceptedTags != null)
            {
                Tag elementTag = element.tag;
                foreach (Tag tag in treeFilterable.AcceptedTags)
                {
                    if (tag == elementTag || element.HasTag(tag))
                    {
                        isMatch = true;
                        break;
                    }
                }
            }

            // Target the filtered cell if matched, otherwise standard output cell
            int targetCell = isMatch ? filteredCell : outputCell;
            ConduitFlow.ConduitContents targetContents = flow.GetContents(targetCell);

            // Move contents if destination conduit is clear/has room
            if (targetContents.mass <= 0f)
            {
                float movedAmount = flow.AddElement(
                    targetCell,
                    contents.element,
                    contents.mass,
                    contents.temperature,
                    contents.diseaseIdx,
                    contents.diseaseCount
                );

                if (movedAmount > 0f)
                {
                    flow.RemoveElement(inputCell, movedAmount);
                }
            }
        }
    }
}*/