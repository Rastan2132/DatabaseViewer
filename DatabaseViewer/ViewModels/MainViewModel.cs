using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using DatabaseViewer.Models;
using System.Collections.ObjectModel;

namespace DatabaseViewer.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string connectionString;
        private ObservableCollection<Item> items;

        public string ConnectionString
        {
            get => connectionString;
            set => SetProperty(ref connectionString, value);
        }

        public ObservableCollection<Item> Items
        {
            get => items;
            set => SetProperty(ref items, value);
        }

        public IRelayCommand ConnectCommand { get; }

        public MainViewModel()
        {
            ConnectCommand = new RelayCommand(Connect);
        }

        private void Connect()
        {
            // Здесь разместите код для установки соединения с базой данных
            // Используйте ConnectionString для подключения к соответствующей базе данных

            // Получите данные из базы данных и сохраните их в Items
        }
    }
}
