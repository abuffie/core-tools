using System;
using System.Collections.Generic;

namespace Aarware.Services.DataStorage {
    /// <summary>
    /// Base class for game save data. Extend this for your game-specific save data.
    /// </summary>
    [Serializable]
    public class SaveData {
        public string saveId;
        public string saveName;
        public DateTime createdAt;
        public DateTime lastModified;
        public Dictionary<string, object> customData;

        public SaveData() {
            saveId = Guid.NewGuid().ToString();
            saveName = "Save";
            createdAt = DateTime.Now;
            lastModified = DateTime.Now;
            customData = new Dictionary<string, object>();
        }

        public SaveData(string saveName) : this() {
            this.saveName = saveName;
        }

        public virtual void OnBeforeSave() {
            lastModified = DateTime.Now;
        }

        public virtual void OnAfterLoad() {
            // Override in derived classes if needed
        }
    }

    /// <summary>
    /// Metadata about a save file without loading the full data.
    /// </summary>
    [Serializable]
    public class SaveMetadata {
        public string saveId;
        public string saveName;
        public DateTime createdAt;
        public DateTime lastModified;
        public long fileSize;

        public SaveMetadata(SaveData saveData, long fileSize = 0) {
            this.saveId = saveData.saveId;
            this.saveName = saveData.saveName;
            this.createdAt = saveData.createdAt;
            this.lastModified = saveData.lastModified;
            this.fileSize = fileSize;
        }
    }
}
