using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using DatabaseViewer.Models;
using System.Data.SqlClient;

namespace DatabaseViewer.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string connectionString;
        private ObservableCollection<Item> items;
        private ObservableCollection<Item> sortedItems;
        private Item newItem;
        private string sortBy;

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

        public ObservableCollection<Item> SortedItems
        {
            get => sortedItems;
            set => SetProperty(ref sortedItems, value);
        }

        public Item NewItem
        {
            get => newItem;
            set => SetProperty(ref newItem, value);
        }

        public string SortBy
        {
            get => sortBy;
            set
            {
                if (SetProperty(ref sortBy, value))
                {
                    SortItems();
                }
            }
        }

        public IRelayCommand ConnectCommand { get; }
        public IRelayCommand AddCommand { get; }
        public IRelayCommand RemoveCommand { get; }

        public MainViewModel()
        {
            ConnectCommand = new RelayCommand(Connect);
            AddCommand = new RelayCommand(Add);
            RemoveCommand = new RelayCommand<Item>(Remove);

            // Инициализация нового элемента
            NewItem = new Item();

            // Инициализация SortedItems
            SortedItems = new ObservableCollection<Item>();

            // Установка начальной сортировки
            SortItems();
        }

        private void Add()
        {
            // Добавление нового элемента
            AddItem(NewItem);

            // Очистка полей для ввода
            NewItem = new Item();
        }

        private void Remove(Item itemToRemove)
        {
            // Удаление элемента
            RemoveItem(itemToRemove);
        }

        private void AddItem(Item newItem)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    var command = new SqlCommand("INSERT INTO Items (Name, Surname, Year, NumContrakt, Pay) VALUES (@Name, @Surname, @Year, @NumContrakt, @Pay)", connection);
                    command.Parameters.AddWithValue("@Name", newItem.Name);
                    command.Parameters.AddWithValue("@Surname", newItem.Surname);
                    command.Parameters.AddWithValue("@Year", newItem.Year);
                    command.Parameters.AddWithValue("@NumContrakt", newItem.NumContrakt);
                    command.Parameters.AddWithValue("@Pay", newItem.Pay);

                    command.ExecuteNonQuery();

                    connection.Close();
                }

                Items.Add(newItem);
                SortItems();

                MessageBox.Show("Item added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveItem(Item itemToRemove)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    var command = new SqlCommand("DELETE FROM Items WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", itemToRemove.Id);

                    command.ExecuteNonQuery();

                    connection.Close();
                }

                Items.Remove(itemToRemove);
                SortItems();

                MessageBox.Show("Item removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Connect()
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Загрузка данных из базы данных
                    Items.Clear();
                    var command = new SqlCommand("SELECT * FROM Items", connection);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var item = new Item
                        {
                            Id = (int)reader["Id"],
                            Name = (string)reader["Name"],
                            Surname = (string)reader["Surname"],
                            Year = (int)reader["Year"],
                            NumContrakt = (string)reader["NumContrakt"],
                            Pay = (decimal)reader["Pay"]
                        };

                        Items.Add(item);
                    }

                    connection.Close();
                }

                SortItems();

                MessageBox.Show("Connected to the database.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to the database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SortItems()
        {
            if (Items == null)
                return;

            switch (SortBy)
            {
                case "Id":
                    SortedItems = new ObservableCollection<Item>(Items.OrderBy(item => item.Id));
                    break;
                case "Name":
                    SortedItems = new ObservableCollection<Item>(Items.OrderBy(item => item.Name));
                    break;
                case "Surname":
                    SortedItems = new ObservableCollection<Item>(Items.OrderBy(item => item.Surname));
                    break;
                case "Year":
                    SortedItems = new ObservableCollection<Item>(Items.OrderBy(item => item.Year));
                    break;
                case "NumContrakt":
                    SortedItems = new ObservableCollection<Item>(Items.OrderBy(item => item.NumContrakt));
                    break;
                case "Pay":
                    SortedItems = new ObservableCollection<Item>(Items.OrderBy(item => item.Pay));
                    break;
                default:
                    SortedItems = Items;
                    break;
            }
        }
    }
}
