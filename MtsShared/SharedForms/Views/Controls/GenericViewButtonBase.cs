#region License

// MIT License
//
// Copyright (c) 2018 Marcus Technical Services, Inc. http://www.marcusts.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion License

using SharedUtils.Interfaces;
using SharedUtils.Utils;

namespace SharedForms.Views.Controls
{
   #region Imports

   using System;
   using System.Diagnostics;
   using Common.Interfaces;
   using Common.Utils;
   using PropertyChanged;
   using Xamarin.Forms;

   #endregion Imports

   public interface IGenericViewButtonBase<T> : IHaveButtonState, IDisposable
     where T : View
   {
      Command ButtonCommand { get; set; }

      string ButtonCommandBindingName { get; set; }

      IValueConverter ButtonCommandConverter { get; set; }

      object ButtonCommandConverterParameter { get; set; }

      object ButtonCommandSource { get; set; }

      Style SelectedButtonStyle { get; set; }

      Style DeselectedButtonStyle { get; set; }

      Style DisabledButtonStyle { get; set; }

      T InternalView { get; set; }

      double? CornerRadiusFixed { get; set; }

      double? CornerRadiusFactor { get; set; }

      bool CanSelect { get; set; }

      bool ToggleSelection { get; set; }

      Color BackColor { get; set; }
   }

   /// <summary>
   /// A button that takes a view to lay on top of it -- can be a label image, etc. NOTE that we need
   /// our own property change handling, since we have no view model. ISSUES:
   /// * On any setting or style change, we need to re-render !!!
   /// * Rendering needs to take the various appearance settings into account, especially radius,
   ///   background color, etc. NEEDS:
   /// * Tap effects -- to grow or shake or vibrate on tap -- ?
   /// * Border Thickness
   /// * Border Color
   /// * Shadow Properties
   /// </summary>
   [AddINotifyPropertyChangedInterface]
   public abstract class GenericViewButtonBase<T> : ShapeView, IGenericViewButtonBase<T>
     where T : View
   {
      public static readonly BindableProperty ButtonStateProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(ButtonState),
          default(ButtonStates),
          BindingMode.TwoWay,
          (viewButton, oldVal, newVal) =>
          {
             viewButton.ButtonState = newVal;
             viewButton.HandleButtonStateChanged();
          }
        );

      public static readonly BindableProperty ButtonCommandProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(ButtonCommand),
          default(Command),
          BindingMode.OneWay,
          (viewButton, oldVal, newVal) => { viewButton.ButtonCommand = newVal; }
        );

      public static readonly BindableProperty ButtonCommandBindingNameProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(ButtonCommandBindingName),
          default(string),
          BindingMode.OneWay,
          (viewButton, oldVal, newVal) => { viewButton.ButtonCommandBindingName = newVal; }
        );

      public static readonly BindableProperty ButtonCommandConverterProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(ButtonCommandConverter),
          default(IValueConverter),
          BindingMode.OneWay,
          (viewButton, oldVal, newVal) => { viewButton.ButtonCommandConverter = newVal; }
        );

      public static readonly BindableProperty ButtonCommandConverterParameterProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(ButtonCommandConverterParameter),
          default(object),
          BindingMode.OneWay,
          (viewButton, oldVal, newVal) => { viewButton.ButtonCommandConverterParameter = newVal; }
        );

      public static readonly BindableProperty SelectedButtonStyleProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(SelectedButtonStyle),
          default(Style),
          BindingMode.OneWay,
          (viewButton, oldVal, newVal) => { viewButton.SelectedButtonStyle = newVal; }
        );

      public static readonly BindableProperty DeselectedButtonStyleProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(DeselectedButtonStyle),
          default(Style),
          BindingMode.OneWay,
          (viewButton, oldVal, newVal) => { viewButton.DeselectedButtonStyle = newVal; }
        );

      public static readonly BindableProperty DisabledButtonStyleProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(DisabledButtonStyle),
          default(Style),
          BindingMode.OneWay,
          (viewButton, oldVal, newVal) => { viewButton.DisabledButtonStyle = newVal; }
        );

      public static readonly BindableProperty CornerRadiusFixedProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(CornerRadiusFixed),
          default(double?),
          BindingMode.OneWay,
          (viewButton, oldVal, newVal) => { viewButton.CornerRadiusFixed = newVal; }
        );

      public static readonly BindableProperty CornerRadiusFactorProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(CornerRadiusFactor),
          default(double?),
          BindingMode.OneWay,
          (viewButton, oldVal, newVal) => { viewButton.CornerRadiusFactor = newVal; }
        );

      public static readonly BindableProperty BackColorProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(BackColor),
          default(Color),
          BindingMode.OneWay,
          (viewButton, oldVal, newVal) => { viewButton.BackColor = newVal; }
        );

      public static readonly BindableProperty CanSelectProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(CanSelect),
          default(bool),
          BindingMode.OneWay,
          (viewButton, oldVal, newVal) => { viewButton.CanSelect = newVal; }
        );

      public static readonly BindableProperty ToggleSelectionProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(ToggleSelection),
          default(bool),
          BindingMode.OneWay,
          (viewButton, oldVal, newVal) => { viewButton.ToggleSelection = newVal; }
        );

      public static readonly BindableProperty SelectionGroupProperty =
        CreateGenericViewButtonBindableProperty
        (
          nameof(SelectionGroup),
          default(int),
          BindingMode.OneWay,
          (viewButton, oldVal, newVal) => { viewButton.SelectionGroup = newVal; }
        );

      //---------------------------------------------------------------------------------------------------------------
      // VARIABLES
      //---------------------------------------------------------------------------------------------------------------

      private readonly TapGestureRecognizer _tapGesture = new TapGestureRecognizer();

      private Command _buttonCommand;

      private string _buttonCommandBindingName;

      private IValueConverter _buttonCommandConverter;

      private object _buttonCommandConverterParameter;

      private object _buttonCommandSource;

      private double? _cornerRadiusFactor;

      private double? _cornerRadiusFixed;

      private Style _deselectedStyle;

      private Style _disabledStyle;

      private volatile bool _internalViewEntered;

      private Style _selectedStyle;

      private int _selectionGroup;

      private volatile bool _tappedListenerEntered;

      private volatile bool _isReleasing;

      //---------------------------------------------------------------------------------------------------------------
      // CONSTRUCTOR
      //---------------------------------------------------------------------------------------------------------------

      protected GenericViewButtonBase()
      {
         IAmSelectedStatic += HandleStaticSelectionChanges;

         GestureRecognizers.Add(_tapGesture);
         _tapGesture.Tapped += HandleTapGestureTapped;

         ShapeType = ShapeType.Box;

         SetStyle();

         // We could specify the properties, but there are *many* that affect style
         PropertyChanged += (sender, args) => { SetStyle(); };
      }

      public event EventUtils.NoParamsDelegate ViewButtonPressed;

      //---------------------------------------------------------------------------------------------------------------
      // PROPERTIES (Public)
      //---------------------------------------------------------------------------------------------------------------

      public bool CanSelect { get; set; }

      public bool ToggleSelection { get; set; }

      public Color BackColor
      {
         get => base.Color;
         set => base.Color = value;
      }

      public Command ButtonCommand
      {
         get => _buttonCommand;
         set
         {
            RemoveButtonCommandEventListener();

            _buttonCommand = value;

            if (ButtonCommand != null)
            {
               ButtonCommand.CanExecuteChanged += HandleButtonCommandCanExecuteChanged;

               // Force-fire the initial state
               ButtonCommand.ChangeCanExecute();
            }
         }
      }

      public string ButtonCommandBindingName
      {
         get => _buttonCommandBindingName;
         set
         {
            _buttonCommandBindingName = value;

            SetUpCompleteViewButtonCommandBinding();
         }
      }

      public IValueConverter ButtonCommandConverter
      {
         get => _buttonCommandConverter;
         set
         {
            _buttonCommandConverter = value;
            SetUpCompleteViewButtonCommandBinding();
         }
      }

      public object ButtonCommandConverterParameter
      {
         get => _buttonCommandConverterParameter;
         set
         {
            _buttonCommandConverterParameter = value;
            SetUpCompleteViewButtonCommandBinding();
         }
      }

      public object ButtonCommandSource
      {
         get => _buttonCommandSource;
         set
         {
            _buttonCommandSource = value;
            SetUpCompleteViewButtonCommandBinding();
         }
      }

      public Style DeselectedButtonStyle
      {
         get => _deselectedStyle;
         set
         {
            _deselectedStyle = value;
            SetStyle();
         }
      }

      public Style DisabledButtonStyle
      {
         get => _disabledStyle;
         set
         {
            _disabledStyle = value;
            SetStyle();
         }
      }

      public T InternalView
      {
         get => Content as T;
         set
         {
            if (_internalViewEntered || _isReleasing)
            {
               return;
            }

            _internalViewEntered = true;

            try
            {
               Content = value;

               AfterInternalViewSet();
            }
            catch (Exception e)
            {
               Debug.WriteLine("INTERNAL VIEW ASSIGNMENT ERROR ->" + e.Message + "<-");
            }

            _internalViewEntered = false;
         }
      }

      public ButtonStates ButtonState
      {
         get => (ButtonStates)GetValue(ButtonStateProperty);
         set => SetValue(ButtonStateProperty, value);
      }

      public event EventHandler<ButtonStates> ButtonStateChanged;

      public Style SelectedButtonStyle
      {
         get => _selectedStyle;
         set
         {
            _selectedStyle = value;
            SetStyle();
         }
      }

      /// <summary>
      /// Leave at 0 if multiple selection is OK
      /// </summary>
      public int SelectionGroup
      {
         get => _selectionGroup;
         set
         {
            _selectionGroup = value;
            BroadcastIfSelected();
         }
      }

      public double? CornerRadiusFixed
      {
         get => _cornerRadiusFixed;
         set
         {
            _cornerRadiusFixed = value;

            SetCornerRadius();
         }
      }

      public double? CornerRadiusFactor
      {
         get => _cornerRadiusFactor;
         set
         {
            _cornerRadiusFactor = value;

            SetCornerRadius();
         }
      }

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      //---------------------------------------------------------------------------------------------------------------
      // EVENTS
      //---------------------------------------------------------------------------------------------------------------

      protected static event EventUtils.GenericDelegate<IGenericViewButtonBase<T>> IAmSelectedStatic;

      //---------------------------------------------------------------------------------------------------------------
      // METHODS - Protected
      //---------------------------------------------------------------------------------------------------------------

      protected virtual void AfterInternalViewSet()
      {
      }

      protected virtual void SetStyle()
      {
         Style newStyle;

         // Set the style based on being enabled/disabled
         if (ButtonState == ButtonStates.Disabled)
         {
            newStyle = DisabledButtonStyle ?? DeselectedButtonStyle;
         }
         else if (ButtonState == ButtonStates.Selected)
         {
            newStyle = SelectedButtonStyle ?? DeselectedButtonStyle;
         }
         else
         {
            newStyle = DeselectedButtonStyle;
         }

         // Cannot compare lists of objects using Equal
         //if (newStyle != null && (Style == null || Style.IsNotAnEqualObjectTo(newStyle)))
         //{
#if MERGE_STYLES
       Style = Style.MergeStyle<GenericViewButtonBase<T>>(newStyle);
#else
         Style = newStyle;
#endif

         // This library is not working well with styles, so forcing all settings manually
         this.ForceStyle(Style);
         //}
      }

      private void SetUpCompleteViewButtonCommandBinding()
      {
         if (ButtonCommandBindingName.IsEmpty())
         {
            RemoveBinding(ButtonCommandProperty);
         }
         else
         {
            this.SetUpBinding
            (
               ButtonCommandProperty,
               ButtonCommandBindingName,
               BindingMode.OneWay,
               ButtonCommandConverter,
               ButtonCommandConverterParameter,
               null,
               ButtonCommandSource
            );
         }
      }

      private void SetCornerRadius()
      {
         if (CornerRadiusFactor.HasNoValue())
         {
            CornerRadiusFactor = FormsUtils.BUTTON_RADIUS_FACTOR;
         }

         if (CornerRadiusFactor.HasValue)
         {
            CornerRadius = Convert.ToSingle(Math.Min(Bounds.Width, Bounds.Height) * _cornerRadiusFactor);
         }
         else if (CornerRadiusFixed.HasValue)
         {
            CornerRadius = Convert.ToSingle(CornerRadiusFixed);
         }
         else
         {
            CornerRadius = Convert.ToSingle(FormsUtils.BUTTON_RADIUS_FACTOR);
         }
      }

      //---------------------------------------------------------------------------------------------------------------
      // EVENT HANDLERS
      //---------------------------------------------------------------------------------------------------------------

      private void BroadcastIfSelected()
      {
         if (ButtonState == ButtonStates.Selected && SelectionGroup > 0)
         {
            // Raise a static event to notify others in this selection group that they should be *deselected*
            IAmSelectedStatic?.Invoke(this);
         }
      }

      private void HandleButtonCommandCanExecuteChanged(object sender, EventArgs e)
      {
         var newCanExecute = sender is Command senderAsCommand && senderAsCommand.CanExecute(this);

         IsEnabled = newCanExecute;

         // The control is not issuing a property change when we manually set IsEnabled, so handling
         // that case here. Cannot listen to property changes generally in this case.
         SetStyle();
      }

      private void HandleStaticSelectionChanges(IGenericViewButtonBase<T> sender)
      {
         // Do not recur onto our own broadcast; also only respond to the same selection group.
         if (sender.SelectionGroup == SelectionGroup && !ReferenceEquals(sender, this) &&
            ButtonState == ButtonStates.Selected)
         {
            ButtonState = ButtonStates.Deselected;
         }
      }

      protected void HandleTapGestureTapped(object sender, EventArgs e)
      {
         if (_tappedListenerEntered || ButtonState == ButtonStates.Disabled)
         {
            return;
         }

         _tappedListenerEntered = true;

         ViewButtonPressed?.Invoke();

         if (CanSelect)
         {
            if (ToggleSelection)
            {
               ButtonState = ButtonState != ButtonStates.Selected ? ButtonStates.Selected : ButtonStates.Deselected;
            }
            else
            {
               ButtonState = ButtonStates.Selected;
            }
         }

         // If a command exists, fire it and reset our selected status to false; otherwise, leave the
         // selected state as it is.
         if (ButtonCommand != null)
         {
            Device.BeginInvokeOnMainThread
            (
#if ANIMATE
            async
#endif
            () =>
            {
#if ANIMATE
                if (InternalView != null)
                {
                  await InternalView.ScaleTo(0.95, 50, Easing.CubicOut);
                  await InternalView.ScaleTo(1, 50, Easing.CubicIn);
                }
#endif

               ButtonCommand.Execute(this);

               // This means that we do not intend to maintain the button state.
               if (SelectionGroup == 0)
               {
                  // Revert the state to its default setting.
                  ButtonState = ButtonStates.Deselected;
               }
            }
            );
         }

         _tappedListenerEntered = false;
      }

      protected override void OnSizeAllocated(double width, double height)
      {
         base.OnSizeAllocated(width, height);

         SetCornerRadius();
      }

      private void RemoveButtonCommandEventListener()
      {
         if (ButtonCommand != null)
         {
            ButtonCommand.CanExecuteChanged -= HandleButtonCommandCanExecuteChanged;
         }
      }

      private void HandleButtonStateChanged()
      {
         SetStyle();
         BroadcastIfSelected();
         ButtonStateChanged?.Invoke(this, ButtonState);
      }

      //---------------------------------------------------------------------------------------------------------------
      // STATIC READ ONLY VARIABLES & METHODS
      //---------------------------------------------------------------------------------------------------------------

      public static Style CreateViewButtonStyle
      (
        Color backColor,
        double? borderWidth = null,
        Color borderColor = default(Color)
      ) => new Style(typeof(GenericViewButtonBase<T>))
      {
         Setters =
          {
            // Use the text color as the border color
            new Setter { Property = BorderColorProperty, Value = borderColor },
            new Setter { Property = BorderWidthProperty, Value = borderWidth.GetValueOrDefault() },

            // The deselected background is set for the underlying view, not the label
            new Setter { Property = ColorProperty, Value = backColor }
          }
      };

      //---------------------------------------------------------------------------------------------------------------
      // BINDABLE PROPERTIES
      //---------------------------------------------------------------------------------------------------------------

      public static BindableProperty CreateGenericViewButtonBindableProperty<PropertyTypeT>
      (
        string localPropName,
        PropertyTypeT defaultVal = default(PropertyTypeT),
        BindingMode bindingMode = BindingMode.OneWay,
        Action<GenericViewButtonBase<T>, PropertyTypeT, PropertyTypeT> callbackAction = null
      ) => BindableUtils.CreateBindableProperty(localPropName, defaultVal, bindingMode, callbackAction);

      //---------------------------------------------------------------------------------------------------------------
      // D I S P O S A L
      //---------------------------------------------------------------------------------------------------------------

      private void ReleaseUnmanagedResources()
      {
         _isReleasing = true;

         // Global static, so remove the handler
         IAmSelectedStatic -= HandleStaticSelectionChanges;

         _tapGesture.Tapped -= HandleTapGestureTapped;

         RemoveButtonCommandEventListener();
      }

      protected virtual void Dispose(bool disposing)
      {
         ReleaseUnmanagedResources();
         if (disposing)
         {
         }
      }

      ~GenericViewButtonBase()
      {
         Dispose(false);
      }
   }
}