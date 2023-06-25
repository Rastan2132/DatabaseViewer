using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
        private string sortBy;
        private MySqlConnection connection; // Äîáàâëåíî ïîëå äëÿ õðàíåíèÿ ñîåäèíåíèÿ ñ áàçîé äàííûõ

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

            // Èíèöèàëèçàöèÿ íîâîãî ýëåìåíòà
            NewItem = new Item();

            // Èíèöèàëèçàöèÿ êîëëåêöèè Items
            Items = new ObservableCollection<Item>();

            // Èíèöèàëèçàöèÿ êîëëåêöèè SortedItems
            SortedItems = new ObservableCollection<Item>();

            // Óñòàíîâêà íà÷àëüíîé ñîðòèðîâêè
            SortItems();
        }

        private void Add()
        {
            // Äîáàâëåíèå íîâîãî ýëåìåíòà
            AddItem(NewItem);

            // Î÷èñòêà ïîëåé äëÿ ââîäà
            NewItem = new Item();
        }

        private void Remove(Item itemToRemove)
        {
            // Óäàëåíèå ýëåìåíòà
            RemoveItem(itemToRemove);
        }

        private void AddItem(Item newItem)
        {
            try
            {
                OpenConnection();

                var command = new MySqlCommand("INSERT INTO user (Name, Surname, Year, NumContrakt, Pay) VALUES (@Name, @Surname, @Year, @NumContrakt, @Pay)", connection);
                command.Parameters.AddWithValue("@Name", newItem.Name);
                command.Parameters.AddWithValue("@Surname", newItem.Surname);
                command.Parameters.AddWithValue("@Year", newItem.Year);
                command.Parameters.AddWithValue("@NumComtrakt", newItem.NumContrakt);
                command.Parameters.AddWithValue("@Pay", newItem.Pay);

                command.ExecuteNonQuery();

                Items.Add(newItem);
                SortItems();

                MessageBox.Show("Item added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Error adding item: {ex.Message}\nError code: {ex.ErrorCode}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CloseConnection();
            }
        }


        private void RemoveItem(Item itemToRemove)
        {
            try
            {
                OpenConnection();

                var command = new MySqlCommand("DELETE FROM user WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", itemToRemove.Id);

                command.ExecuteNonQuery();

                Items.Remove(itemToRemove);
                SortItems();

                MessageBox.Show("Item removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing item: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CloseConnection();
            }
        }

        private void Connect()
        {
            try
            {
                OpenConnection();

                // Çàãðóçêà äàííûõ èç áàçû äàííûõ
                Items.Clear();
                var command = new MySqlCommand("SELECT * FROM user", connection);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var item = new Item
                    {
                        Id = (int)reader["Id"],
                        Name = (string)reader["Name"],
                        Surname = (string)reader["Surname"],
                        Year = ((DateTime)reader["Year"]).Year,
                        NumContrakt = ((int)reader["NumComtrakt"]).ToString(),
                        Pay = (int)reader["Pay"]
                    };

                    Items.Add(item);
                }

                SortItems();

                MessageBox.Show("Connected to the database.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to the database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CloseConnection();
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
                    SortedItems = new ObservableCollection<Item>(Items);
                    break;
            }
        }

        private void OpenConnection()
        {
            if (connection == null)
                connection = new MySqlConnection(ConnectionString);

            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();
        }

        private void CloseConnection()
        {
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
                connection.Close();
        }
    }
}