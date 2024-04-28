using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Controls InfoScreen UI Panel. Displayes player information about keys and side objectives.
/// </summary>
public class InfoScreen : MonoBehaviour
{
    public TMP_Text CamoCount;
    public TMP_Text TrapKitCount;
    public TMP_Text Keys;
    public TMP_Text SideObjectives;

    private List<DoorKey> _keys = new();

    public void SetCamoCount(int count)
    {
        CamoCount.text = count.ToString();
    }

    public void SetTrapKitCount(int count)
    {
        TrapKitCount.text = count.ToString();
    }

    public void AddKey(DoorKey key)
    {
        _keys.Add(key);
    }

    public void ClearKeys()
    {
        _keys.Clear();
    }

    /// <summary>
    /// Updates text displaying number of side objectives found.
    /// </summary>
    /// <param name="count"></param>
    /// <param name="found"></param>
    public void SetSideObjectives(int count, int found)
    {
        SideObjectives.text = $"Side Objectives Found:\n{found}/{count}";
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Refreshes text that displays door keys.
    /// </summary>
    public void RefreshKeys()
    {
        StringBuilder sb = new();
        foreach (DoorKey key in _keys)
        {
            foreach (DoorLock @lock in key.Locks.Cast<DoorLock>())
            {
                foreach (Door door in @lock.Instances.Cast<Door>())
                {
                    sb.AppendLine(door.name);
                }
            }
        }

        Keys.text = sb.ToString();
    }
}
