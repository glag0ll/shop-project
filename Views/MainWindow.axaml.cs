using Avalonia.Controls;
using System;

namespace AvaloniaApplication3.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            InitializeComponent();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing main window: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}