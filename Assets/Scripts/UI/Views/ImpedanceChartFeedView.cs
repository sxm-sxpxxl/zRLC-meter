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
    [SerializeField] private MeasurementProcessController measurementProcessController;
    
    [Header("Settings")]
    [SerializeField] private GraphChartBase magnitudeGraph;
    [SerializeField] private string magnitudeChartCategoryName = "ImpedanceMagnitude";
    [Space]
    [SerializeField] private GraphChartBase phaseGraph;
    [SerializeField] private string phaseChartCategoryName = "ImpedancePhase";

    private readonly List<ImpedanceMeasureData> _measuredImpedanceMagnitudes = new List<ImpedanceMeasureData>(capacity: 1000);

    private void Awake()
    {
        measurementProcessController.OnImpedanceMeasured += AddImpedanceMeasureToChart;
        measurementProcessController.OnImpedanceMeasuringStarted += ClearChartData;
    }

    private void OnDestroy()
    {
        measurementProcessController.OnImpedanceMeasured -= AddImpedanceMeasureToChart;
        measurementProcessController.OnImpedanceMeasuringStarted -= ClearChartData;
    }
    
    public void OnChartPointClicked(GraphChartBase.GraphEventArgs graphEventArgs)
    {
        ImpedanceMeasureData data = _measuredImpedanceMagnitudes[graphEventArgs.Index];
        OnImpedanceSelected.Invoke(data);
    }
    
    public void ClearChartData()
    {
        _measuredImpedanceMagnitudes.Clear();
        magnitudeGraph.DataSource.ClearCategory(magnitudeChartCategoryName);
        phaseGraph.DataSource.ClearCategory(phaseChartCategoryName);
    }

    private void AddImpedanceMeasureToChart(ImpedanceMeasureData data)
    {
        _measuredImpedanceMagnitudes.Add(data);
        
        magnitudeGraph.DataSource.AddPointToCategoryRealtime(magnitudeChartCategoryName, x: data.frequency, y: data.impedance.Magnitude);
        phaseGraph.DataSource.AddPointToCategoryRealtime(phaseChartCategoryName, x: data.frequency, y: data.impedance.AngleInRad * Mathf.Rad2Deg);

        magnitudeGraph.DataSource.AutomaticcHorizontaViewGap = 1f;
        magnitudeGraph.DataSource.AutomaticVerticallViewGap = 0.1f;
        phaseGraph.DataSource.AutomaticcHorizontaViewGap = 1f;
    }
}
