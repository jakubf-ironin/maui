using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33169 : _IssuesUITest
{
	public override string Issue => "Issue33169";

	public Issue33169(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.RefreshView)]
	public void RefreshViewWithoutHandlerShouldBehaveConsistentlyRegardlessOfLargeTitles()
	{
		// Wait for page to load
		App.WaitForElement("ToggleLargeTitlesButton");
		App.WaitForElement("ToggleHandlerButton");
		App.WaitForElement("ConfigLabel");
		
		// Note: Since we cannot reliably automate pull-to-refresh gesture in automated tests,
		// this test focuses on verifying the UI elements are present and configuration can be toggled.
		// Manual testing is required to verify the actual refresh behavior with different configurations.
		
		var configLabel = App.FindElement("ConfigLabel").GetText();
		Assert.That(configLabel, Does.Contain("LargeTitles: Never"));
		Assert.That(configLabel, Does.Contain("Handler: OFF"));

		// Toggle to Large Titles Always
		App.Tap("ToggleLargeTitlesButton");
		Task.Delay(1000).Wait();
		
		configLabel = App.FindElement("ConfigLabel").GetText();
		Assert.That(configLabel, Does.Contain("LargeTitles: Always"));

		// Toggle handler on
		App.Tap("ToggleHandlerButton");
		Task.Delay(1000).Wait();
		
		configLabel = App.FindElement("ConfigLabel").GetText();
		Assert.That(configLabel, Does.Contain("Handler: ON"));

		// Toggle back to no handler
		App.Tap("ToggleHandlerButton");
		Task.Delay(1000).Wait();
		
		configLabel = App.FindElement("ConfigLabel").GetText();
		Assert.That(configLabel, Does.Contain("Handler: OFF"));

		// Verify UI is still responsive
		App.WaitForElement("TestCollectionView");
	}
}
