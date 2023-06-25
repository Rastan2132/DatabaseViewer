using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using DatabaseViewer.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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

            // ������������� ������ ��������
            NewItem = new Item();

            // ������������� SortedItems
            SortedItems = new ObservableCollection<Item>();

            // ��������� ��������� ����������
            SortItems();
        }

        private void Add()
        {
            // ���������� ������ ��������
            AddItem(NewItem);

            // ������� ����� ��� �����
            NewItem = new Item();
        }

        private void Remove(Item itemToRemove)
        {
            // �������� ��������
            RemoveItem(itemToRemove);
        }

        private void AddItem(Item newItem)
        {
            try
            {
                using (var dbContext = new DatabaseContext(ConnectionString))
                {
                    dbContext.Database.OpenConnection();

                    // ���������� ����� ������
                    dbContext.Items.Add(newItem);
                    dbContext.SaveChanges();

                    // ���������� ������ Items ����� ���������� ������
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

        private void RemoveItem(Item itemToRemove)
        {
            try
            {
                using (var dbContext = new DatabaseContext(ConnectionString))
                {
                    dbContext.Database.OpenConnection();

                    // ����� ������ ��� ��������
                    var existingItem = dbContext.Items.FirstOrDefault(i => i.Id == itemToRemove.Id);

                    if (existingItem != null)
                    {
                        // �������� ������
                        dbContext.Items.Remove(existingItem);
                        dbContext.SaveChanges();

                        // ���������� ������ Items ����� �������� ������
                        Items.Remove(itemToRemove);
                    }

                    dbContext.Database.CloseConnection();
                }

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
                using (var dbContext = new DatabaseContext(ConnectionString))
                {
                    dbContext.Database.OpenConnection();

                    // �������� ������ �� ���� ������
                    Items = new ObservableCollection<Item>(dbContext.Items);

                    dbContext.Database.CloseConnection();
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
