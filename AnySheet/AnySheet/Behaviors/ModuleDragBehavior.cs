using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;
using System;
using System.Windows.Input;

namespace AnySheet.Behaviors;

public class ModuleDragBehavior : Behavior<Control>
{
    public class DragCompletedCommandParameters
    {
        public Control Control { get; set; }
        public Point RawPosition { get; set; }
        public Point GridPosition { get; set; }
    }
    
    public static readonly StyledProperty<ICommand?> DragCompletedCommandProperty =
        AvaloniaProperty.Register<ModuleDragBehavior, ICommand?>(nameof(DragCompletedCommand));
    public static readonly StyledProperty<int> GridWidthProperty =
        AvaloniaProperty.Register<ModuleDragBehavior, int>(nameof(GridWidth), defaultValue: 1);
    public static readonly StyledProperty<int> GridHeightProperty =
        AvaloniaProperty.Register<ModuleDragBehavior, int>(nameof(GridHeight), defaultValue: 1);

    public ICommand? DragCompletedCommand
    {
        get => GetValue(DragCompletedCommandProperty);
        set => SetValue(DragCompletedCommandProperty, value);
    }

    public int GridWidth
    {
        get => GetValue(GridWidthProperty);
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            SetValue(GridWidthProperty, value);
        }
    }

    public int GridHeight
    {
        get => GetValue(GridHeightProperty);
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            SetValue(GridHeightProperty, value);
        }
    }

    private bool _dragging;
    private double _gridDx;
    private double _gridDy;
    private Point _lastPosition;
    private Control? _parent;
    private TranslateTransform? _transform;
    
    protected override void OnAttachedToVisualTree()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.PointerPressed += Pressed;
            AssociatedObject.PointerReleased += Released;
            AssociatedObject.PointerMoved += Moved;
            AssociatedObject.PointerCaptureLost += CaptureLost;
            Console.WriteLine($"Attached module drag behavior with grid size {GridWidth}x{GridHeight}.");
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
        _gridDx = 0;
        _gridDy = 0;
    
        if (AssociatedObject.RenderTransform is TranslateTransform transform)
        {
            _transform = transform;
        }
        else
        {
            _transform = new TranslateTransform();
            AssociatedObject.RenderTransform = _transform;
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
        if (!_dragging || !properties.IsLeftButtonPressed || _parent == null || _transform == null || !IsEnabled)
        {
            return;
        }
        
        var position = e.GetPosition(_parent);
        var dx = position.X - _lastPosition.X;
        var dy = position.Y - _lastPosition.Y;
        _lastPosition = position;

        _gridDx += dx;
        _gridDy += dy;

        while (_gridDx <= -GridWidth)
        {
            _gridDx += GridWidth;
            _transform.X -= GridWidth;
        }

        while (_gridDx >= GridWidth)
        {
            _gridDx -= GridWidth;
            _transform.X += GridWidth;
        }
        
        while (_gridDy <= -GridHeight)
        {
            _gridDy += GridHeight;
            _transform.Y -= GridHeight;
        }

        while (_gridDy >= GridHeight)
        {
            _gridDy -= GridHeight;
            _transform.Y += GridHeight;
        }
    }

    private void EndDrag()
    {
        DragCompletedCommand?.Execute(new DragCompletedCommandParameters
        {
            Control = AssociatedObject!,
            RawPosition = new Point(_transform!.X, _transform.Y),
            GridPosition = new Point(Math.Floor(_transform.X / GridWidth), Math.Floor(_transform.Y / GridHeight))
        });
        _dragging = false;
        _parent = null;
        _transform = null;
    }
}