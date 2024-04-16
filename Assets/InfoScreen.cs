using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public class InfoScreen : MonoBehaviour
{
    public TMP_Text CamoCount;
    public TMP_Text TrapKitCount;
    public TMP_Text Keys;

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

    public void Refresh()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
