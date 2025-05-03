using Microsoft.Maui.Controls;

namespace SlopeGuard;

public class BaseContentPage : ContentPage
{
    public BaseContentPage()
    {
        var backgroundImage = new Image
        {
            Source = "background.png",
            Aspect = Aspect.AspectFill,
            Opacity = 0.2,
            IsOpaque = false
        };

        var layout = new Grid();
        layout.Children.Add(backgroundImage);

        // This will hold your actual page content
        var contentHolder = new ContentView();
        layout.Children.Add(contentHolder);

        base.Content = layout;

        // Let derived pages access this container to add their content
        ContentHolder = contentHolder;
    }

    protected ContentView ContentHolder { get; }

    // Override Content to redirect to inner holder
    public new View Content
    {
        get => ContentHolder.Content;
        set => ContentHolder.Content = value;
    }
}
