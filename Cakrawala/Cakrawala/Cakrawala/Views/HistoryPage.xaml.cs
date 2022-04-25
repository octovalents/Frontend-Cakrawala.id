﻿using Cakrawala.Models;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Cakrawala.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HistoryPage : ContentPage
    {
        public static List<TransactionHistory> transListView { get; set; }
        public ObservableRangeCollection<Grouping<DateTime, TransactionHistory>> transHistoryGroup { get; set; }
        public int lenTransListView;

        public HistoryPage()
        {
            InitializeComponent();
            transHistoryGroup = new ObservableRangeCollection<Grouping<DateTime, TransactionHistory>>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RetrieveUserData();
            resetState();
        }

        public async void RetrieveUserData()
        {
            string userId = Application.Current.Properties["userId"].ToString();

            List<TransferHistory> listTransferHistory = await App.transactionHistoryService.TransferHistoryAsync(userId);

            List<TopupHistory> listTopupHistory = await App.transactionHistoryService.TopupHistoryAsync(userId);

            // renew data list transaction from backend
            HistoryPage.transListView = new List<TransactionHistory>();
            int idTrans = 0;

            foreach (TransferHistory transferHistory in listTransferHistory)
            {
                TransactionType transactionType = TransactionType.TERIMAUSER;
                string transactionNote = "Diterima dari User";
                string urlImage = "payinuser.png";
                string idFrom = transferHistory.from;
                string idTo = transferHistory.to;

                if (idFrom == userId)
                {
                    transactionType = TransactionType.BAYARUSER;
                    transactionNote = "Dibayar ke User";
                    urlImage = "payoutuser.png";
                }

                User userFrom = await App.profileService.ViewProfileAsync(idFrom);
                User userTo = await App.profileService.ViewProfileAsync(idTo);

                HistoryPage.transListView.Add(new TransactionHistory(
                    (idTrans).ToString(),
                    transactionType,
                    transactionNote,
                    urlImage,
                    transferHistory.createdAt.ToString("ddd, dd MMM yyyy"),
                    transferHistory.createdAt,
                    transferHistory.updatedAt,
                    userFrom,
                    userTo,
                    transferHistory.baseValue,
                    transferHistory.status));
                idTrans += 1;
            }

            foreach (TopupHistory topupHistory in listTopupHistory)
            {
                TransactionType transactionType = TransactionType.ISIULANG;
                string transactionNote = "Isi Ulang Saldo";
                string urlImage = "topup.png";

                HistoryPage.transListView.Add(new TransactionHistory(
                    (idTrans).ToString(),
                    transactionType,
                    transactionNote,
                    urlImage,
                    topupHistory.createdAt.ToString("ddd, dd MMM yyyy"),
                    topupHistory.createdAt,
                    topupHistory.updatedAt,
                    topupHistory.value,
                    topupHistory.method,
                    topupHistory.status));
                idTrans += 1;
            }
            HistoryPage.transListView.OrderBy(x => x.createdDate);
            HistoryListView.ItemsSource = HistoryPage.transListView;
        }

        private void resetState()
        {
            var todayDate = DateTime.Today;

            // radio button week ago
            todayDateWeek.Text = todayDate.ToString("dd MMM yyyy");
            oneWeekDate.Text = todayDate.AddDays(-7).ToString("dd MMM yyyy");

            // radio button month ago
            todayDateMonth.Text = todayDate.ToString("dd MMM yyyy");
            oneMonthDate.Text = todayDate.AddMonths(-1).ToString("dd MMM yyyy");

            todayDateAllTime.Text = todayDate.ToString("dd MMM yyyy");
        }

        private async void DetailHistoryPage_Tapped(object sender, ItemTappedEventArgs e)
        {
            string transaksiId = (e.Item as TransactionHistory).transactionId;
            Debug.WriteLine("[TRANSAKSI ID]");
            Debug.WriteLine(transaksiId);
            await Shell.Current.GoToAsync($"//detailHistory?historyId={transaksiId}");
        }

        public void changeHistoryTransactionsWeekAgo(object sender, EventArgs args)
        {
            var todayDate = DateTime.Today;

            // radio button week ago
            DateTime from = todayDate.AddDays(-7);
            DateTime to = todayDate;

            List<TransactionHistory> list = new List<TransactionHistory>();
            list = GetListTransactionbyDate(from, to);
            if(list.Count > 0)
            {
                HistoryListView.ItemsSource = GetListTransactionbyDate(from, to);
            } 
            else
            {
                Debug.WriteLine("Listview is null or empty");
            }

            Debug.WriteLine("Week ago");
        }

        public void changeHistoryTransactionsMonthAgo(object sender, EventArgs args)
        {
            var todayDate = DateTime.Today;

            DateTime from = todayDate.AddMonths(-1);
            DateTime to = todayDate;

            List<TransactionHistory> list = new List<TransactionHistory>();
            list = GetListTransactionbyDate(from, to);
            if (list.Count > 0)
            {
                HistoryListView.ItemsSource = GetListTransactionbyDate(from, to);
            }
            else
            {
                Debug.WriteLine("Listview is null or empty");
            }

            Debug.WriteLine("Month ago");
        }

        public void changeHistoryTransactionsAllTime(object sender, EventArgs args)
        {
            HistoryListView.ItemsSource = HistoryPage.transListView;
        }

        private List<TransactionHistory> GetListTransactionbyDate(DateTime from, DateTime to)
        {
            List<TransactionHistory> list = new List<TransactionHistory>();

            foreach (TransactionHistory transactionHistory in HistoryPage.transListView)
            {
                int dateFromState = DateTime.Compare(from, transactionHistory.createdDate);
                int dateToState = DateTime.Compare(to, transactionHistory.createdDate);
                Debug.Write("DATE: ");
                Debug.Write(from);
                Debug.WriteLine(to);
                Debug.WriteLine(transactionHistory.createdDate);

                Debug.Write("STATE: ");
                Debug.Write(dateFromState);
                Debug.WriteLine(dateToState);

                if (dateFromState > 0 &&
                    dateToState < 0)
                {
                    list.Add(transactionHistory);
                }
            }

            return list;
        }
    }
}