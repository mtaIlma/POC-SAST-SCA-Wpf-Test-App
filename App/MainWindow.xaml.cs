using TestWpfApplication;
using TestWpfApplication.Models;
using TestWpfApplication.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TestWpfApplication.Models;
using TestWpfApplication.Services;

namespace TestWpfApplication
{
    public partial class MainWindow : Window
    {
        private readonly UserService _userService;
        private List<User> _currentUsers;

        public MainWindow()
        {
            InitializeComponent();
            _userService = new UserService();                 
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
          
        }

       

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private void SqlSearchButton_Click(object sender, RoutedEventArgs e)
        {
            var sqlWindow = new SqlSearchWindow();
            sqlWindow.ShowDialog();
        }

        private void DynamicFieldsButton_Click(object sender, RoutedEventArgs e)
        {
            var xamlGeneratorWindow = new XamlGeneratorWindow();
            xamlGeneratorWindow.ShowDialog();
        }

        private void XmlSigningButton_Click(object sender, RoutedEventArgs e)
        {
            var xmlSigningWindow = new XmlSigningWindow();
            xmlSigningWindow.ShowDialog();
        }
    }
}