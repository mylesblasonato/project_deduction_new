using System;
using System.Collections.Generic;
using SQLite;

namespace AssetInventory
{
    [Serializable]
    public sealed class RelativeSequence
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        [Indexed] public string Key { get; set; }
        [Indexed] public string System { get; set; }
        [Indexed] public string Location { get; set; }

        // runtime only
        public List<string> otherLocations;

        public RelativeSequence()
        {
        }

        public RelativeSequence(string key, string system, string location) : this()
        {
            Key = key;
            System = system;
            Location = location;
        }

        public void SetLocation(string location)
        {
            Location = location?.Replace("\\", "/");
        }

        public override string ToString()
        {
            return $"Location '{Key}' ({Location})";
        }
    }
}
