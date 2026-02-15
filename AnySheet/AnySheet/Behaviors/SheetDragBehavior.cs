using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;
using Action = System.Action;

namespace AnySheet.Behaviors;

public class SheetDragBehavior : StyledElementBehavior<Control>
{
    private bool _dragging;
    private Point _lastPosition;
    private Control? _parent;
    private List<TranslateTransform> _transforms = [];
   
    public Action DragCompleted { get; set; } = () => {};
    public double ZoomScale = 1;
    
    public static readonly StyledProperty<IEnumerable<SheetModule.SheetModule>> ModulesProperty =
        AvaloniaProperty.Register<SheetDragBehavior, IEnumerable<SheetModule.SheetModule>>("Modules");

    public IEnumerable<SheetModule.SheetModule> Modules
    {
        get => GetValue(ModulesProperty);
        set => SetValue(ModulesProperty, value);
    }

    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.PointerPressed += Pressed;
            AssociatedObject.PointerReleased += Released;
            AssociatedObject.PointerMoved += Moved;
            AssociatedObject.PointerCaptureLost += CaptureLost;
            Console.WriteLine("Attached sheet drag behavior.");
        }
    }
    
    protected override void OnDetachedFromVisualTree()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.PointerPressed -= Pressed;
            AssociatedObject.PointerReleased -= Released;
            AssociatedObject.PointerMoved -= Moved;
            AssociatedObject.PointerCaptureLost -= CaptureLost;
        }
    }
    
    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (!properties.IsLeftButtonPressed || AssociatedObject?.Parent is not Control parent || !IsEnabled)
        {
            return;
        }
        
        _parent = parent;
        var startPos = e.GetPosition(parent);
        _lastPosition = startPos;
        
        foreach (var module in Modules)
        {
            if (module.RenderTransform is TranslateTransform transform)
            {
                _transforms.Add(transform);
            }
            else
            {
                var t = new TranslateTransform();
                module.RenderTransform = t;
                _transforms.Add(t);
            }
        }
            
        _dragging = true;
    }
    
    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (_dragging && IsEnabled)
        {
            EndDrag();
        }
    }
    
    private void CaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_dragging && IsEnabled)
        {
            EndDrag();
        }
    }
    
    private void Moved(object? sender, PointerEventArgs e)
    {
        var properties = e.GetCurrentPoint(AssociatedObject).Properties;
        if (!_dragging || !properties.IsLeftButtonPressed || _parent == null || !IsEnabled)
        {
            return;
        }
        
        var position = e.GetPosition(_parent);
        var dx = (position.X - _lastPosition.X) / ZoomScale;
        var dy = (position.Y - _lastPosition.Y) / ZoomScale;
        _lastPosition = position;
        foreach (var transform in _transforms)
        {
            transform.X += dx;
            transform.Y += dy;
        }
    }

    private void EndDrag()
    {
        DragCompleted();
        _dragging = false;
        _parent = null;
        _transforms.Clear();
    }
}