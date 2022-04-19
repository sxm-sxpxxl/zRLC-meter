using UnityEngine;
using ChartAndGraph;

[RequireComponent(typeof(GraphChartBase))]
public sealed class GraphChartFeed : MonoBehaviour
{
    [SerializeField, Range(1, 100)] private int pointsQuantity = 50;
    [SerializeField] private string chartCategoryName = "Player 1";
    
	private void Start ()
    {
        GraphChartBase graph = GetComponent<GraphChartBase>();
        
        graph.DataSource.StartBatch();
        graph.DataSource.ClearCategory(chartCategoryName);
            
        for (int i = 0; i < pointsQuantity; i++)
        {
            graph.DataSource.AddPointToCategory(chartCategoryName, Random.value * 10f, Random.value * 10f + 20f);
        }

        graph.DataSource.EndBatch();
    }
}
