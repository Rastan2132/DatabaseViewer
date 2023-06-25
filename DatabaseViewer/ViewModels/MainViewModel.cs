using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using DatabaseViewer.Models;
using MySql.Data.MySqlClient;

namespace DatabaseViewer.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private string connectionString;
        private ObservableCollection<Item> items;
        private ObservableCollection<Item> sortedItems;
        private Item newItem;
        private ComboBoxItem? sortBy;

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

        public ComboBoxItem? SortBy
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

        private int NewId
        {
            get
            {
                var lastId = Items.Select(item => item.Id).DefaultIfEmpty().Max();
                return lastId + 1;
            }
        }

        public MainViewModel()
        {
            ConnectCommand = new RelayCommand(Connect);
            AddCommand = new RelayCommand(Add);
            RemoveCommand = new RelayCommand(Remove);

            // Инициализация нового элемента
            NewItem = new Item();

            // Инициализация SortedItems
            Items = new ObservableCollection<Item>();
            SortedItems = new ObservableCollection<Item>();

            // Установка начальной сортировки
            SortItems();
        }

        private void Add()
        {
            // Добавление нового элемента
            AddItem(NewItem);

            // Очистка полей для ввода
            NewItem = new Item()
            {
                Id = NewId
            };
        }

        private void Remove()
        {
            if (NewItem == null)
            {
                MessageBox.Show("No item selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Удаление элемента
            RemoveItem();
        }

        private void AddItem(Item newItem)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    var command = new MySqlCommand("INSERT INTO Items (Name, Surname, Year, NumComtrakt, Pay) VALUES (@Name, @Surname, @Year, @NumComtrakt, @Pay)", connection);
                    command.Parameters.AddWithValue("@Name", newItem.Name);
                    command.Parameters.AddWithValue("@Surname", newItem.Surname);

                    var yearString = new DateOnly(newItem.Year, 1, 1).ToString("yyyy-MM-dd");
                    command.Parameters.AddWithValue("@Year", yearString);

                    command.Parameters.AddWithValue("@NumComtrakt", newItem.NumContrakt);
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

        private void RemoveItem()
        {
            var itemToRemove = NewItem;

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    var command = new MySqlCommand("DELETE FROM Items WHERE Id = @Id", connection);
                    command.Parameters.AddWithValue("@Id", itemToRemove.Id);

                    command.ExecuteNonQuery();

                    connection.Close();
                }

                Items.Remove(itemToRemove);
                SortItems();
                NewItem = new Item()
                {
                    Id = NewId
                };


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
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Загрузка данных из базы данных
                    Items.Clear();
                    var command = new MySqlCommand("SELECT * FROM Items", connection);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var item = new Item
                        {
                            Id = (int)(uint)reader["Id"],
                            Name = (string)reader["Name"],
                            Surname = (string)reader["Surname"],
                            Year = ((DateTime)reader["Year"]).Year,
                            NumContrakt = ((uint)reader["NumComtrakt"]).ToString(),
                            Pay = new decimal((int)reader["Pay"])
                        };

                        Items.Add(item);
                    }

                    connection.Close();
                }

                SortItems();
                NewItem = new Item()
                {
                    Id = NewId
                };

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

            var itemContent = SortBy is not null ? SortBy.Content : "";

            switch (itemContent)
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
                case "NumComtrakt":
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
