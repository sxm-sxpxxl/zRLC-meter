using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

[DisallowMultipleComponent]
public sealed class TabGroup : MonoBehaviour
{
    [SerializeField] private List<TabPage> tabPages = new List<TabPage>();
    [SerializeField, Range(0, 3)] private int defaultTabPageIndex = 0;
    [SerializeField] private bool activatePagesOnAwake = true;

    private TabPage _selectedTabPage;
    
    [Serializable]
    private class TabPage
    {
        public TabButton tab;
        public GameObject page;
    }

    private void OnValidate()
    {
        SetTabPage(defaultTabPageIndex);
    }

    private void Awake()
    {
        foreach (var tabPage in tabPages)
        {
            tabPage.page.SetActive(activatePagesOnAwake);
            tabPage.tab.OnClick += OnTabClick;
        }
    }

    private void Start()
    {
        if (activatePagesOnAwake)
        {
            tabPages.ForEach(x => x.page.SetActive(false));
        }

        SetTabPage(defaultTabPageIndex);
    }

    private void OnDestroy()
    {
        foreach (var tabPage in tabPages)
        {
            tabPage.tab.OnClick -= OnTabClick;
        }
    }

    private void OnTabClick(TabButton clickedTab)
    {
        if (_selectedTabPage.tab == clickedTab)
        {
            return;
        }

        int targetIndex = tabPages.FindIndex(0, x => x.tab == clickedTab);
        SetTabPage(targetIndex);
    }

    private void SetTabPage(int index)
    {
        Assert.IsTrue(index >= 0 && index < tabPages.Count, "Tab page index doesn't match known tab pages.");

        if (_selectedTabPage != null)
        {
            _selectedTabPage.tab.SetState(TabButton.State.Idle);
            _selectedTabPage.page.SetActive(false);
        }
        
        tabPages[index].tab.SetState(TabButton.State.Select);
        tabPages[index].page.SetActive(true);
        
        _selectedTabPage = tabPages[index];
    }
}
