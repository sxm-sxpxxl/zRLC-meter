using System;
using System.Collections.Generic;
using UnityEngine;
using ChartAndGraph;

public sealed class ImpedanceChartFeedView : MonoBehaviour
{
    public event Action<float, float> OnImpedanceSelected = delegate { };

    [SerializeField] private GraphChartBase graph;
    [SerializeField] private ImpedanceMeasurer impedanceMeasurer;
    [SerializeField] private string chartCategoryName = "Player 1";

    private readonly List<float> _measuredImpedanceMagnitudes = new List<float>(capacity: 1000);

    private void Awake()
    {
        impedanceMeasurer.OnImpedanceMeasured += AddImpedanceMeasureToChart;
        impedanceMeasurer.OnImpedanceMeasuringStarted += ClearChartData;
    }

    private void OnDestroy()
    {
        impedanceMeasurer.OnImpedanceMeasured -= AddImpedanceMeasureToChart;
        impedanceMeasurer.OnImpedanceMeasuringStarted -= ClearChartData;
    }
    
    public void OnChartPointClicked(GraphChartBase.GraphEventArgs graphEventArgs)
    {
        OnImpedanceSelected.Invoke(_measuredImpedanceMagnitudes[graphEventArgs.Index], (float) graphEventArgs.Value.x);
    }

    private void AddImpedanceMeasureToChart(float impedanceMagnitude, float frequency)
    {
        _measuredImpedanceMagnitudes.Add(impedanceMagnitude);
        graph.DataSource.AddPointToCategoryRealtime(chartCategoryName, x: frequency, y: impedanceMagnitude);
    }

    private void ClearChartData()
    {
        _measuredImpedanceMagnitudes.Clear();
        graph.DataSource.ClearCategory(chartCategoryName);
    }
}
