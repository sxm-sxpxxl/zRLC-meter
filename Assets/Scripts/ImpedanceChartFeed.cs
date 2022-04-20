using System;
using System.Collections.Generic;
using UnityEngine;
using ChartAndGraph;

public sealed class ImpedanceChartFeed : MonoBehaviour
{
    public event Action<ComplexDouble, float> OnImpedanceSelected = delegate { };

    [SerializeField] private GraphChartBase graph;
    [SerializeField] private ImpedanceMeasurer impedanceMeasurer;
    [SerializeField] private string chartCategoryName = "Player 1";

    private readonly List<ComplexDouble> _measuredImpedances = new List<ComplexDouble>(capacity: 1000);
    
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
        OnImpedanceSelected.Invoke(_measuredImpedances[graphEventArgs.Index], (float) graphEventArgs.Value.x);
    }

    private void AddImpedanceMeasureToChart(ComplexDouble impedance, float frequency)
    {
        _measuredImpedances.Add(impedance);
        graph.DataSource.AddPointToCategoryRealtime(chartCategoryName, x: frequency, y: impedance.Magnitude);
    }

    private void ClearChartData()
    {
        _measuredImpedances.Clear();
        graph.DataSource.ClearCategory(chartCategoryName);
    }
}
