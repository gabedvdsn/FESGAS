using System;
using System.Collections.Generic;
using System.Reflection;

namespace FESGameplayAbilitySystem
{
    public static class Tags
    {
        /// <summary>
        /// Tags channels are used to reserve tags for dedicated system wide use. Common applications are payload-related tags.
        ///
        /// To create your own tags, follow the template shown below. Adhering to the naming convention is suggested.
        ///
        ///     private const int _TAG_DESCRIPTION = -***_***_***;
        ///     public static IntegerTag TAG_DESCRIPTION => ITag.Get(_TAG_DESCRIPTION);
        ///
        /// These persistent tags should be defined as negative numbers, as dynamically created tags are always positive.
        /// 
        /// </summary>
        ///

        #region Validation

        private static HashSet<int> reservedChannels;
        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (isInitialized) return;

            var type = typeof(Tags);
            var constFields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic);

            foreach (var field in constFields)
            {
                if (!field.IsLiteral) continue;
                if (field.FieldType != typeof(int)) continue;

                reservedChannels.Add((int)field.GetRawConstantValue());
            }

            isInitialized = true;
        }

        public static bool TagIsAvailable(int tag)
        {
            if (!isInitialized) Initialize();
            return !reservedChannels.Contains(tag);
        }
        
        #endregion
        
        #region Affiliation Tags
        
        /*
         * Affiliation tags are used to indicate affiliations between GAS components.
         */
        
        #region User
        
        private const int _AFFILIATION_GREEN = -405_254_019;
        public static IntegerTag AFFILIATION_GREEN => ITag.GetUnsafe(_AFFILIATION_GREEN);
        
        private const int _AFFILIATION_RED = -405_254_020;
        public static IntegerTag AFFILIATION_RED => ITag.GetUnsafe(_AFFILIATION_RED);
        
        private const int _AFFILIATION_GRAY = -405_254_021;
        public static IntegerTag AFFILIATION_GRAY => ITag.GetUnsafe(_AFFILIATION_GRAY);
        
        #endregion
        
        #region Default
        
        private const int _AFFILIATION_ROOT = -405_254_018;
        public static IntegerTag AFFILIATION_ROOT => ITag.GetUnsafe(_AFFILIATION_ROOT);
        
        #endregion
        
        #endregion
        
        #region Payload Tags
        
        /*
         * Payload tags are used in data packets to associate data with their cast-types and use case.
         */
        
        #region Ability Packets
        
        private const int _PAYLOAD_GAS = -505_254_019;
        public static IntegerTag PAYLOAD_GAS => ITag.GetUnsafe(_PAYLOAD_GAS);
        
        private const int _PAYLOAD_TRANSFORM = -505_254_020;
        public static IntegerTag PAYLOAD_TRANSFORM => ITag.GetUnsafe(_PAYLOAD_TRANSFORM);
        
        private const int _PAYLOAD_POSITION = -505_254_021;
        public static IntegerTag PAYLOAD_POSITION => ITag.GetUnsafe(_PAYLOAD_POSITION);
        
        private const int _PAYLOAD_ROTATION = -505_254_022;
        public static IntegerTag PAYLOAD_ROTATION => ITag.GetUnsafe(_PAYLOAD_ROTATION);
        
        private const int _PAYLOAD_DERIVATION = -505_254_023;
        public static IntegerTag PAYLOAD_DERIVATION => ITag.GetUnsafe(_PAYLOAD_DERIVATION);
        
        private const int _PAYLOAD_AFFILIATION = -505_254_024;
        public static IntegerTag PAYLOAD_AFFILIATION => ITag.GetUnsafe(_PAYLOAD_AFFILIATION);
        
        private const int _PAYLOAD_SOURCE = -605_254_024;
        public static IntegerTag PAYLOAD_SOURCE => ITag.GetUnsafe(_PAYLOAD_SOURCE);
        
        private const int _PAYLOAD_TARGET = -605_254_025;
        public static IntegerTag PAYLOAD_TARGET => ITag.GetUnsafe(_PAYLOAD_TARGET);
        
        private const int _PAYLOAD_DATA = -605_254_026;
        public static IntegerTag PAYLOAD_DATA => ITag.GetUnsafe(_PAYLOAD_DATA);
        
        #endregion
        
        #region GAS Store (Coffer)
        
        private const int _STORE_DISJOINTABLE = -705_254_026;
        public static IntegerTag STORE_DISJOINTABLE => ITag.GetUnsafe(_STORE_DISJOINTABLE);
        
        #endregion
        
        #endregion
    }
}
