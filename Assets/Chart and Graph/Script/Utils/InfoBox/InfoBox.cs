using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

namespace ChartAndGraph
{
    /// <summary>
    /// this class demonstrates the use of chart events
    /// </summary>
    public class InfoBox : MonoBehaviour
    {
        public GraphChartBase[] GraphChart;
        public Text infoText; 
         
        void GraphClicked(GraphChartBase.GraphEventArgs args)
        {
            if (args.Magnitude < 0f)
                infoText.text = string.Format("{0} : {1},{2} Clicked", args.Category, args.XString, args.YString);
            else
                infoText.text = string.Format("{0} : {1},{2} : Sample Size {3} Clicked", args.Category, args.XString, args.YString, args.Magnitude);
        }

        void GraphHoverd(GraphChartBase.GraphEventArgs args)
        {
            if (args.Magnitude < 0f)
                infoText.text = string.Format("{0} : {1},{2}", args.Category, args.XString, args.YString);
            else
                infoText.text = string.Format("{0} : {1},{2} : Sample Size {3}", args.Category, args.XString, args.YString, args.Magnitude);
        }

        void NonHovered()
        {
            infoText.text = "";
        }

        public void HookChartEvents()
        {
            if(GraphChart  != null)
            {
                foreach(GraphChartBase graph in GraphChart)
                {
                    if (graph == null)
                        continue;
                    graph.PointClicked.AddListener(GraphClicked);
                    graph.PointHovered.AddListener(GraphHoverd);
                    graph.NonHovered.AddListener(NonHovered);
                }
            }
        }

        // Use this for initialization
        void Start()
        {
            HookChartEvents();
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}