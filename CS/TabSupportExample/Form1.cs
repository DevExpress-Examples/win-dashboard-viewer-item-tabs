﻿using DevExpress.DashboardCommon;
using DevExpress.DashboardWin;
using DevExpress.XtraEditors;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace TabSupportExample
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        private Timer tabTimer = new Timer();
        string timedTabContainerName;
        public Form1()
        {
            InitializeComponent();
            dashboardViewer1.CustomizeDashboardItemCaption += DashboardViewer1_CustomizeDashboardItemCaption;
            dashboardViewer1.Dashboard = CreateSimpleDashboard();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            Dashboard dashboard = new Dashboard();
            DashboardObjectDataSource dataSource = new DashboardObjectDataSource(DataGenerator.GenerateTestData());
            dashboard.DataSources.Add(dataSource);

            dashboard.Items.Add(DashboardItemGenerator.GenerateCardItem(dataSource, "card1"));
            dashboard.Items.Add(DashboardItemGenerator.GenerateGridItem(dataSource, "grid1"));
            dashboard.Items.Add(DashboardItemGenerator.GeneratePieItem(dataSource, "pie1"));
            dashboard.Items.Add(DashboardItemGenerator.GenerateListBoxItem(dataSource, "list1"));

            TabContainerDashboardItem tabContainer = new TabContainerDashboardItem();
            tabContainer.ComponentName = "tabContainer1";
            tabContainer.TabPages.Add(new DashboardTabPage() { Name = "Tab Page One", ComponentName = "page1" });
            tabContainer.TabPages["page1"].AddRange(dashboard.Items["grid1"], dashboard.Items["pie1"]);

            DashboardTabPage secondTabPage = tabContainer.CreateTabPage();
            secondTabPage.Name = "Tab Page Two";
            secondTabPage.Add(dashboard.Items["list1"]);
            secondTabPage.ShowItemAsTabPage = true;

            dashboard.Items.Add(tabContainer);

            dashboard.RebuildLayout();
            // Adjust the dashboard layout.
            dashboard.LayoutRoot.FindRecursive(dashboard.Items["grid1"]).Weight = 40;
            dashboard.LayoutRoot.FindRecursive(dashboard.Items["pie1"]).Weight = 60;
            DashboardLayoutGroup rootGroup = dashboard.LayoutRoot.ChildNodes[0] as DashboardLayoutGroup;
            rootGroup.Orientation = DashboardLayoutGroupOrientation.Horizontal;

            dashboardViewer1.Dashboard = dashboard;

            btnModify.Enabled = true;
            toggleSwitchTimer.Enabled = true;
        }

        private void btnModify_Click(object sender, EventArgs e)
        {
            Dashboard dashboard = dashboardViewer1.Dashboard;

            // Move the card1 item to a tab page containing the list1 item.
            dashboard.Items["card1"].ParentContainer = dashboard.Items["list1"].ParentContainer;
            // Reorder tab pages.
            TabContainerDashboardItem tabContainer = dashboard.Items["tabContainer1"] as TabContainerDashboardItem;
            DashboardTabPage tabPage = tabContainer.TabPages[0];
            tabContainer.TabPages.Remove(tabPage);
            tabContainer.TabPages.Insert(1, tabPage);

            dashboard.Items.ForEach(delegate (DashboardItem item)
            {
                if (item is PieDashboardItem)
                {
                    ((PieDashboardItem)item).PieType = PieType.Donut;
                }
            });
        }


        private void TabTimer_Tick(object sender, EventArgs e)
        {
            if (timedTabContainerName != null)
            {
                int selectedIndex = dashboardViewer1.GetSelectedTabPageIndex(timedTabContainerName);
                int pageCount = ((TabContainerDashboardItem)dashboardViewer1.Dashboard.Items[timedTabContainerName]).TabPages.Count;
                dashboardViewer1.SetSelectedTabPage(timedTabContainerName, ++selectedIndex % pageCount);
            }
        }

        private void toggleSwitchTimer_Toggled(object sender, EventArgs e)
        {
            if (((ToggleSwitch)sender).IsOn)
            {
                timedTabContainerName = dashboardViewer1.Dashboard.Items.FindFirst(x => x is TabContainerDashboardItem).ComponentName;
                tabTimer.Interval = 2000;
                tabTimer.Tick += TabTimer_Tick;
                tabTimer.Start();
            }
            else
                tabTimer.Stop();
        }

        private Dashboard CreateSimpleDashboard()
        {
            Dashboard dashboard = new Dashboard();
            DashboardObjectDataSource dataSource = new DashboardObjectDataSource(DataGenerator.GenerateTestData());
            dashboard.DataSources.Add(dataSource);

            GridDashboardItem gridItem = new GridDashboardItem() { ComponentName = "grid1" };
            gridItem.DataSource = dataSource;
            gridItem.Columns.Add(new GridDimensionColumn(new Dimension("Country")));
            gridItem.Columns.Add(new GridMeasureColumn(new Measure("Sales")));

            PieDashboardItem pieItem = new PieDashboardItem() { ComponentName = "pie1" };
            pieItem.DataSource = dataSource;
            pieItem.Values.Add(new Measure("Sales"));
            pieItem.Arguments.Add(new Dimension("Country"));

            dashboard.Items.AddRange(gridItem, pieItem);
            return dashboard;
        }

        private void DashboardViewer1_CustomizeDashboardItemCaption(object sender, CustomizeDashboardItemCaptionEventArgs e)
        {
            Dashboard dashboard = ((DashboardViewer)sender).Dashboard;

            e.Items.Add(new DashboardToolbarItem(string.Empty,
            new Action<DashboardToolbarItemClickEventArgs>((args) =>
            {
                MessageBox.Show(e.DashboardItemName, "Dashboard Item Component Name");
                ((DashboardViewer)sender).SaveDashboardLayout("test_dashboard_layout.xml");
            }))
            { ButtonImage = Image.FromFile("Support_16x16.png") });
        }


    }
}
