using System;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using msdnus.ViewModel;
using Windows.System;

namespace msdnus
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel Vm { get; }

        public MainPage()
        {
            InitializeComponent();

            Vm = App.Services.GetRequiredService<MainViewModel>();
            DataContext = Vm;

            var version = typeof(App).Assembly.GetName().Version;
            VersionText.Text = $"v{version?.ToString(3) ?? "?"}";

            RegisterAccelerator(VirtualKey.F, () => { if (Vm.IsUnlocked) SearchBox.Focus(FocusState.Programmatic); });
            RegisterAccelerator(VirtualKey.O, () => { if (Vm.IsUnlocked) Vm.ImportFromFileCommand.Execute(null); });
            RegisterAccelerator(VirtualKey.L, () => { if (Vm.IsUnlocked) Vm.LockCommand.Execute(null); });

            Loaded += (_, _) => PinBox.Focus(FocusState.Programmatic);
            Vm.PropertyChanged += OnVmPropertyChanged;
        }

        private void RegisterAccelerator(VirtualKey key, Action action)
        {
            var accelerator = new KeyboardAccelerator { Key = key, Modifiers = VirtualKeyModifiers.Control };
            accelerator.Invoked += (_, e) =>
            {
                action();
                e.Handled = true;
            };
            KeyboardAccelerators.Add(accelerator);
        }

        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Vm.IsLocked)) return;

            if (Vm.IsLocked)
            {
                PinBox.Password = string.Empty;
                PinBox.Focus(FocusState.Programmatic);
            }
            else
            {
                PinBox.Password = string.Empty;
                SearchBox.Focus(FocusState.Programmatic);
            }
        }

        private void PinBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            Vm.CheckPasswordCommand.Execute(PinBox.Password);
        }

        private void SearchBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape)
            {
                Vm.SearchText = string.Empty;
                e.Handled = true;
            }
        }

        private async void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            NewPin1.Password = string.Empty;
            NewPin2.Password = string.Empty;
            await SettingsDialog.ShowAsync();
        }

        private void SettingsDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Theme is applied live via the TwoWay binding; only the PIN needs handling.
            if (string.IsNullOrEmpty(NewPin1.Password) && string.IsNullOrEmpty(NewPin2.Password)) return;

            args.Cancel = !Vm.TryChangePin(NewPin1.Password, NewPin2.Password);
        }
    }
}
