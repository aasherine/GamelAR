using System.Collections.Generic;
using UnityEngine;

namespace GamelAR
{
    /// <summary>Daftar pusat 15 instrumen. Disimpan di Assets/Resources/InstrumentDatabase.asset.</summary>
    [CreateAssetMenu(menuName = "GamelAR/Instrument Database", fileName = "InstrumentDatabase")]
    public class InstrumentDatabase : ScriptableObject
    {
        public List<InstrumentData> instruments = new List<InstrumentData>();

        static InstrumentDatabase cached;
        Dictionary<string, InstrumentData> byId;
        Dictionary<string, InstrumentData> byImage;

        public static InstrumentDatabase Load()
        {
            if (cached == null) cached = Resources.Load<InstrumentDatabase>("InstrumentDatabase");
            return cached;
        }

        void BuildIndex()
        {
            byId = new Dictionary<string, InstrumentData>();
            byImage = new Dictionary<string, InstrumentData>();
            foreach (var i in instruments)
            {
                if (i == null) continue;
                if (!string.IsNullOrEmpty(i.id)) byId[i.id] = i;
                if (!string.IsNullOrEmpty(i.referenceImageName)) byImage[i.referenceImageName] = i;
            }
        }

        public InstrumentData GetById(string id)
        {
            if (byId == null) BuildIndex();
            return id != null && byId.TryGetValue(id, out var d) ? d : null;
        }

        public InstrumentData GetByImageName(string imageName)
        {
            if (byImage == null) BuildIndex();
            return imageName != null && byImage.TryGetValue(imageName, out var d) ? d : null;
        }

        void OnValidate() { byId = null; byImage = null; }
    }
}
