using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33169, "[iOS] Missing Refreshing handler behaves inconsistently when LargePageTitles are enabled", PlatformAffected.iOS)]
public partial class Issue33169 : ContentPage
{
	private bool _useLargeTitles = false;
	private bool _hasHandler = false;
	private int _refreshCount = 0;

	public Issue33169()
	{
		InitializeComponent();

		// Initialize with sample data
		var items = new ObservableCollection<string>();
		for (int i = 1; i <= 20; i++)
		{
			items.Add($"Item {i}");
		}
		TestCollectionView.ItemsSource = items;

		UpdateConfigLabel();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		
		Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
		{
			CaptureState("OnAppearing");
		});
	}

	private void OnToggleLargeTitles(object sender, EventArgs e)
	{
		_useLargeTitles = !_useLargeTitles;
		
		Console.WriteLine($"=== TOGGLING LARGE TITLES: {_useLargeTitles} ===");
		
		if (_useLargeTitles)
		{
			this.On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Always);
		}
		else
		{
			this.On<iOS>().SetLargeTitleDisplay(LargeTitleDisplayMode.Never);
		}

		UpdateConfigLabel();
		CaptureState($"AfterToggleLargeTitles_{_useLargeTitles}");
	}

	private void OnToggleHandler(object sender, EventArgs e)
	{
		_hasHandler = !_hasHandler;
		
		Console.WriteLine($"=== TOGGLING HANDLER: {_hasHandler} ===");
		
		if (_hasHandler)
		{
			TestRefreshView.Refreshing += OnRefreshing;
		}
		else
		{
			TestRefreshView.Refreshing -= OnRefreshing;
		}

		UpdateConfigLabel();
		CaptureState($"AfterToggleHandler_{_hasHandler}");
	}

	private void OnRefreshing(object sender, EventArgs e)
	{
		_refreshCount++;
		Console.WriteLine($"=== REFRESHING EVENT FIRED (Count: {_refreshCount}) ===");
		
		// Simulate async work
		Dispatcher.DispatchDelayed(TimeSpan.FromSeconds(2), () =>
		{
			Console.WriteLine($"=== COMPLETING REFRESH (Count: {_refreshCount}) ===");
			TestRefreshView.IsRefreshing = false;
			StatusLabel.Text = $"Status: Refreshed {_refreshCount} time(s)";
			CaptureState($"AfterRefresh_{_refreshCount}");
		});
	}

	private void UpdateConfigLabel()
	{
		var largeTitleText = _useLargeTitles ? "Always" : "Never";
		var handlerText = _hasHandler ? "ON" : "OFF";
		ConfigLabel.Text = $"LargeTitles: {largeTitleText} | Handler: {handlerText}";
		
		Console.WriteLine($"=== CONFIG UPDATED: LargeTitles={largeTitleText}, Handler={handlerText} ===");
	}

	private void CaptureState(string context)
	{
		Console.WriteLine($"=== STATE CAPTURE: {context} ===");
		Console.WriteLine($"  Page.LargeTitleDisplay: {this.On<iOS>().LargeTitleDisplay()}");
		Console.WriteLine($"  RefreshView.IsRefreshing: {TestRefreshView.IsRefreshing}");
		Console.WriteLine($"  Handler attached: {_hasHandler}");
		Console.WriteLine($"  Refresh count: {_refreshCount}");
		
		if (TestRefreshView.Handler?.PlatformView != null)
		{
			Console.WriteLine($"  RefreshView.Handler present: Yes");
		}
		else
		{
			Console.WriteLine($"  RefreshView.Handler present: No");
		}
		
		Console.WriteLine("=== END STATE CAPTURE ===");
	}
}
