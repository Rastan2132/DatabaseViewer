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
        private Item newItem;

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

        public Item NewItem
        {
            get => newItem;
            set => SetProperty(ref newItem, value);
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
                using (var dbContext = new DatabaseContext(ConnectionString))
                {
                    dbContext.Database.OpenConnection();

                    // Добавление новой записи
                    dbContext.Items.Add(newItem);
                    dbContext.SaveChanges();

                    // Обновление списка Items после добавления записи
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

                    // Поиск записи для удаления
                    var existingItem = dbContext.Items.FirstOrDefault(i => i.Id == itemToRemove.Id);

                    if (existingItem != null)
                    {
                        // Удаление записи
                        dbContext.Items.Remove(existingItem);
                        dbContext.SaveChanges();

                        // Обновление списка Items после удаления записи
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

                    // Выполнение запроса на выборку всех записей из таблицы "Items"
                    var query = dbContext.Items.ToList();
                    Items = new ObservableCollection<Item>(query);
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
