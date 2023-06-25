using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using DatabaseViewer.Models;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System;
using System.Linq;

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
        private void AddItem(Item newItem)
        {
            try
            {
                using (var dbContext = new DatabaseContext(ConnectionString))
                {
                    dbContext.Database.OpenConnection();

                    // ƒобавление новой записи
                    dbContext.Items.Add(newItem);
                    dbContext.SaveChanges();

                    // ќбновление списка Items после добавлени€ записи
                    Items.Add(newItem);

                    dbContext.Database.CloseConnection();
                }

                MessageBox.Show("Item added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

       

        private void Connect()
        {
            try
            {
                using (var dbContext = new DatabaseContext(ConnectionString))
                {
                    dbContext.Database.OpenConnection();

                    // ¬ыполнение запроса на выборку всех записей из таблицы "Items"
                    var query = dbContext.Items.ToList();
                    Items = new ObservableCollection<Item>(query);
                    var newItem = new Item { Id = 1, Name = "John", Surname = "Doe", Year = 1990, NumContrakt = "ABC123", Pay = 1000 };
                    AddItem(newItem);
                    dbContext.Database.CloseConnection();
                }


                MessageBox.Show("Connected to the database.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to the database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
