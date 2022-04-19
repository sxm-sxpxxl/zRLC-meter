using System;
using UnityEngine;
using ChartAndGraph;

[RequireComponent(typeof(GraphChartBase))]
public sealed class WaveformChartFeed : MonoBehaviour
{
    [SerializeField] private WaveformGenerator waveformGenerator;
    [SerializeField] private string chartCategoryName = "Player 1";

    private GraphChartBase _graph;
    
    private void Awake()
    {
        _graph = GetComponent<GraphChartBase>();
        waveformGenerator.OnSamplesChunkReady += AddSamplesChunkToGraphChart;
    }

    private void OnDestroy()
    {
        waveformGenerator.OnSamplesChunkReady -= AddSamplesChunkToGraphChart;
    }

    private void AddSamplesChunkToGraphChart(float[] samplesChunk)
    {
        _graph.DataSource.StartBatch();
        // _graph.DataSource.ClearCategory(chartCategoryName);
            
        for (int i = 0; i < samplesChunk.Length; i++)
        {
            _graph.DataSource.AddPointToCategoryRealtime(chartCategoryName, x: i, y: samplesChunk[i]);
        }

        _graph.DataSource.EndBatch();
    }
}
