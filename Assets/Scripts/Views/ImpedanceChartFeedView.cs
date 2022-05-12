using System;
using System.Collections.Generic;
using UnityEngine;
using ChartAndGraph;

/// <summary>
/// Отслеживает процесс измерений и заполняет графики значениями амплитуды и фазы импеданса по мере завершения измерений.
/// </summary>
public sealed class ImpedanceChartFeedView : MonoBehaviour
{
    public event Action<ImpedanceMeasureData> OnImpedanceSelected = delegate { };

    [Header("Dependencies")]
    [SerializeField] private ImpedanceMeasurer impedanceMeasurer;
    
    [Header("Settings")]
    [SerializeField] private GraphChartBase magnitudeGraph;
    [SerializeField] private string magnitudeChartCategoryName = "ImpedanceMagnitude";
    [Space]
    [SerializeField] private GraphChartBase phaseGraph;
    [SerializeField] private string phaseChartCategoryName = "ImpedancePhase";

    private readonly List<ImpedanceMeasureData> _measuredImpedanceMagnitudes = new List<ImpedanceMeasureData>(capacity: 1000);

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
        ImpedanceMeasureData data = _measuredImpedanceMagnitudes[graphEventArgs.Index];
        OnImpedanceSelected.Invoke(data);
    }

    private void AddImpedanceMeasureToChart(ImpedanceMeasureData data)
    {
        _measuredImpedanceMagnitudes.Add(data);
        
        magnitudeGraph.DataSource.AddPointToCategoryRealtime(magnitudeChartCategoryName, x: data.frequency, y: data.magnitude);
        phaseGraph.DataSource.AddPointToCategoryRealtime(phaseChartCategoryName, x: data.frequency, y: data.phaseInDeg);
    }

    private void ClearChartData()
    {
        _measuredImpedanceMagnitudes.Clear();
        magnitudeGraph.DataSource.ClearCategory(magnitudeChartCategoryName);
        phaseGraph.DataSource.ClearCategory(phaseChartCategoryName);
    }
}
