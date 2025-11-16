using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace msdnus.Services
{
    public interface INavigationService
    {
        void NavigateTo(string pageKey);
        void NavigateTo(Type pageType);
        void NavigateTo(Type pageType, object parameter);
        void GoBack();
        bool CanGoBack { get; }
    }

    public class NavigationService : INavigationService
    {
        private readonly Dictionary<string, Type> _pages = new();
        private Frame _frame;

        public Frame Frame
        {
            get => _frame;
            set
            {
                _frame = value;
                _frame.Navigated += (s, e) => CanGoBackChanged?.Invoke();
            }
        }

        public event Action CanGoBackChanged;

        public bool CanGoBack => _frame?.CanGoBack ?? false;

        public void Configure(string key, Type pageType)
        {
            lock (_pages)
            {
                if (_pages.ContainsKey(key))
                {
                    _pages[key] = pageType;
                }
                else
                {
                    _pages.Add(key, pageType);
                }
            }
        }

        public void NavigateTo(string pageKey)
        {
            lock (_pages)
            {
                if (!_pages.ContainsKey(pageKey))
                {
                    throw new ArgumentException($"Page not found: {pageKey}. Did you forget to call Configure?");
                }

                var pageType = _pages[pageKey];
                NavigateTo(pageType);
            }
        }

        public void NavigateTo(Type pageType)
        {
            if (_frame != null)
            {
                _frame.Navigate(pageType);
            }
        }

        public void NavigateTo(Type pageType, object parameter)
        {
            if (_frame != null)
            {
                _frame.Navigate(pageType, parameter);
            }
        }

        public void GoBack()
        {
            if (_frame?.CanGoBack == true)
            {
                _frame.GoBack();
            }
        }
    }
}
