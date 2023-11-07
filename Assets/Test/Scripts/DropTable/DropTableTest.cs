using System.Linq;
using TrickModule.DropTable;
using UnityEngine;

public class DropTableTest : MonoBehaviour
{
    [SerializeField] private UnityDropTable<int> _test;
    private void Awake()
    {
        int loop = 100;
        int[] results = new int[loop];
        for (int i = 0; i < loop; i++)
        {
            results[i] = _test.RandomItem() as int? ?? 0;
        }
        Debug.Log($"Results: {string.Join(", ", results)}");
        Debug.Log($"Average: {results.Average()}");
        foreach (UnityDropTable<int>.Entry entry in _test.Items)
        {
            Debug.Log($"Encountered '{entry.Object}' {results.Count(x => x == entry.Object)} times");            
        }
    }
}
