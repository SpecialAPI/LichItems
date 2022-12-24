using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LichItems
{
    [Serializable]
    public class AdvancedCompanionTransformSynergy : ScriptableObject
    {
        [LongNumericEnum]
        public CustomSynergyType RequiredSynergy;
        [EnemyIdentifier]
        public string SynergyCompanionGuid;
        public bool UseStringSynergyDetectionInstead;
        public string RequiredStringSynergy;
    }
}
