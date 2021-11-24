﻿using CinemaManagement.DTOs;
using CinemaManagement.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaManagement.Models.Services
{
    public partial class OverviewStatisticService
    {
        private OverviewStatisticService() { }
        private static OverviewStatisticService _ins;
        public static OverviewStatisticService Ins
        {
            get
            {
                if (_ins == null)
                {
                    _ins = new OverviewStatisticService();
                }
                return _ins;
            }
            private set => _ins = value;
        }

        #region Overview
        public int GetBillQuantity(int year, int month = 0)
        {
            var context = DataProvider.Ins.DB;
            try
            {
                if (month == 0)
                {
                    return context.Bills.Where(b => b.CreatedAt.Year == year).Count();
                }
                else
                {
                    return context.Bills.Where(b => b.CreatedAt.Year == year && b.CreatedAt.Month == month).Count();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion

        #region Revenue


        
        public (List<decimal>, decimal ProductRevenue,  decimal TicketRevenue, string TicketRateStr) GetRevenueByYear(int year)
        {
            var context = DataProvider.Ins.DB;
            List<decimal> MonthlyRevenueList = new List<decimal>(new decimal[12]);

            try
            {
                var billList = context.Bills
                    .Where(b => b.CreatedAt.Year == year);

                (decimal ProductRevenue, decimal TicketRevenue) = GetFullRevenue(year);

                var MonthlyRevenue = billList
                         .GroupBy(b => b.CreatedAt.Month)
                         .Select(gr => new
                         {
                             Month = gr.Key,
                             Income = gr.Sum(b => (decimal?)b.TotalPrice) ?? 0
                         }).ToList();

                foreach (var re in MonthlyRevenue)
                {
                    MonthlyRevenueList[re.Month - 1] = decimal.Truncate(re.Income);
                }

                (decimal lastProdReve, decimal lastTicketReve) = GetFullRevenue(year - 1);
                decimal lastRevenueTotal = lastProdReve + lastTicketReve;
                string RevenueRateStr;
                if (lastRevenueTotal == 0)
                {
                    RevenueRateStr = "-2";
                }
                else
                {
                    RevenueRateStr = Helper.ConvertDoubleToPercentageStr((double)((ProductRevenue + TicketRevenue) / lastRevenueTotal) - 1);
                }
               
                return (MonthlyRevenueList, decimal.Truncate(ProductRevenue), decimal.Truncate(TicketRevenue), RevenueRateStr);
            }
            catch (Exception e)
            {

                throw e;
            }
        }


        public (List<decimal>, decimal ProductRevenue,  decimal TicketRevenue, string RevenueRate) GetRevenueByMonth(int year, int month)
        {
            var context = DataProvider.Ins.DB;
            int days = DateTime.DaysInMonth(year, month);
            List<decimal> DailyReveList = new List<decimal>(new decimal[days]);

            try
            {
                var billList = context.Bills
                     .Where(b => b.CreatedAt.Year == year && b.CreatedAt.Month == month);

                (decimal ProductRevenue, decimal TicketRevenue) = GetFullRevenue(year, month);

                var dailyRevenue = billList
                            .GroupBy(b => b.CreatedAt.Day)
                             .Select(gr => new
                             {
                                 Day = gr.Key,
                                 Income = gr.Sum(b => b.TotalPrice),
                                 DiscountPrice = gr.Sum(b => (decimal?)b.DiscountPrice) ?? 0,
                             }).ToList();

                foreach (var re in dailyRevenue)
                {
                    DailyReveList[re.Day - 1] = decimal.Truncate(re.Income);
                }

                if (month == 1)
                {
                    year--;
                    month = 13;
                }
                (decimal lastProdReve, decimal lastTicketReve) = GetFullRevenue(year, month - 1);
                decimal lastRevenueTotal = lastProdReve + lastTicketReve;
                string RevenueRateStr;
                if (lastRevenueTotal == 0)
                {
                    RevenueRateStr = "-2";
                }
                else
                {
                    RevenueRateStr = Helper.ConvertDoubleToPercentageStr((double)((ProductRevenue + TicketRevenue) / lastRevenueTotal) - 1);
                }

                return (DailyReveList, decimal.Truncate(ProductRevenue), decimal.Truncate(TicketRevenue), RevenueRateStr);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Lấy doanh thu của sản phẩm và vé, truyền 1 tham số thì đó sẽ là tìm theo năm, 2 tham số là theo năm và tháng
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public (decimal, decimal) GetFullRevenue(int year, int month = 0)
        {
            try
            {

                var context = DataProvider.Ins.DB;
                if (month != 0)
                {

                    decimal ProductRevenue = context.ProductBillInfoes.Where(pB => pB.Bill.CreatedAt.Year == year && pB.Bill.CreatedAt.Month == month)
                                                    .Sum(pB => (decimal?)(pB.PricePerItem * pB.Quantity)) ?? 0;
                    decimal TicketRevenue = context.Tickets.Where(t => t.Bill.CreatedAt.Year == year && t.Bill.CreatedAt.Month == month)
                                                    .Sum(t => (decimal?)t.Price) ?? 0;

                    return (ProductRevenue, TicketRevenue);

                }
                else
                {
                    decimal ProductRevenue = context.ProductBillInfoes.Where(pB => pB.Bill.CreatedAt.Year == year)
                                                    .Sum(pB => (decimal?)(pB.PricePerItem * pB.Quantity)) ?? 0;
                    decimal TicketRevenue = context.Tickets.Where(t => t.Bill.CreatedAt.Year == year)
                                                    .Sum(t => (decimal?)t.Price) ?? 0;
                    return (ProductRevenue, TicketRevenue);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion



        #region Expense
        private decimal GetFullExpenseLastTime(int year, int month = 0)
        {
            var context = DataProvider.Ins.DB;
            try
            {
                if (month == 0)
                {
                    //Product Receipt
                    decimal LastYearProdExpense = context.ProductReceipts
                             .Where(pr => pr.CreatedAt.Year == year)
                             .Sum(pr => (decimal?)pr.ImportPrice) ?? 0;

                    //Repair Cost
                    var LastYearRepairCost = LastYearProdExpense * 2;
                    return (LastYearProdExpense + LastYearRepairCost);
                }
                else
                {
                    //Product Receipt
                    decimal LastMonthProdExpense = context.ProductReceipts
                             .Where(pr => pr.CreatedAt.Year == year && pr.CreatedAt.Month == month)
                             .Sum(pr => (decimal?)pr.ImportPrice) ?? 0;
                    //Repair Cost
                    var LastMonthRepairCost = LastMonthProdExpense * 2;
                    return (LastMonthProdExpense + LastMonthRepairCost);
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        public (List<decimal> MonthlyExpense, decimal ProductExpense, decimal RepairCost, string ExpenseRate) GetExpenseByYear(int year)
        {
            var context = DataProvider.Ins.DB;
            List<decimal> MonthlyExpense = new List<decimal>(new decimal[12]);
            decimal ProductExpenseTotal = 0;
            decimal RepairCostTotal = 0;

            //Product Receipt
            try
            {
                var MonthlyProdExpense = DataProvider.Ins.DB.ProductReceipts
                     .Where(b => b.CreatedAt.Year == year)
                     .GroupBy(b => b.CreatedAt.Month)
                     .Select(gr => new
                     {
                         Month = gr.Key,
                         Outcome = gr.Sum(b => (decimal?)b.ImportPrice) ?? 0
                     }).ToList();

                //Repair Cost
                var MonthlyRepairCost = MonthlyProdExpense.Select(p => new { Month = p.Month, Outcome = p.Outcome * 2 }).ToList();


                //Accumulate
                foreach (var ex in MonthlyProdExpense)
                {
                    MonthlyExpense[ex.Month - 1] += decimal.Truncate(ex.Outcome);
                    ProductExpenseTotal += ex.Outcome;
                }

                foreach (var ex in MonthlyRepairCost)
                {
                    MonthlyExpense[ex.Month - 1] += decimal.Truncate(ex.Outcome);
                    RepairCostTotal += ex.Outcome;
                }
                decimal lastProductExpenseTotal = GetFullExpenseLastTime(year - 1);
                string ExpenseRateStr;
                //check mẫu  = 0
                if (lastProductExpenseTotal == 0)
                {
                    ExpenseRateStr = "-2";
                }
                else
                {
                    ExpenseRateStr = Helper.ConvertDoubleToPercentageStr(((double)(ProductExpenseTotal / lastProductExpenseTotal) - 1));
                }


                return (MonthlyExpense, decimal.Truncate(ProductExpenseTotal), decimal.Truncate(RepairCostTotal), ExpenseRateStr);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public (List<decimal> DailyExpense, decimal ProductExpense, decimal RepairCost, string RepairRateStr) GetExpenseByMonth(int year, int month)
        {
            var context = DataProvider.Ins.DB;

            int days = DateTime.DaysInMonth(year, month);
            List<decimal> DailyExpense = new List<decimal>(new decimal[days]);
            decimal ProductExpenseTotal = 0;
            decimal RepairCostTotal = 0;

            try
            {
                //Product Receipt
                var MonthlyProdExpense = DataProvider.Ins.DB.ProductReceipts
                         .Where(b => b.CreatedAt.Year == year && b.CreatedAt.Month == month)
                         .GroupBy(b => b.CreatedAt.Day)
                         .Select(gr => new
                         {
                             Day = gr.Key,
                             Outcome = gr.Sum(b => (decimal?)b.ImportPrice) ?? 0
                         }).ToList();

                //Repair Cost
                var MonthlyRepairCost = MonthlyProdExpense.Select(p => new { Day = p.Day, Outcome = p.Outcome * 2 }).ToList();

                //Accumulate
                foreach (var ex in MonthlyProdExpense)
                {
                    DailyExpense[ex.Day - 1] += decimal.Truncate(ex.Outcome);
                    ProductExpenseTotal += ex.Outcome;
                }
                foreach (var ex in MonthlyRepairCost)
                {
                    DailyExpense[ex.Day - 1] += decimal.Truncate(ex.Outcome);
                    RepairCostTotal += ex.Outcome;
                }
                if (month == 1)
                {
                    year--;
                    month = 13;
                }

                decimal lastProductExpenseTotal = GetFullExpenseLastTime(year, month - 1);
                string ExpenseRateStr;
                //check mẫu  = 0
                if (lastProductExpenseTotal == 0)
                {
                    ExpenseRateStr = "-2";
                }
                else
                {
                    ExpenseRateStr = Helper.ConvertDoubleToPercentageStr(((double)(ProductExpenseTotal / lastProductExpenseTotal) - 1));
                }

                return (DailyExpense, ProductExpenseTotal, RepairCostTotal, ExpenseRateStr);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        //public static T GetPropertyValue<T>(object obj, string propName)
        //{
        //    return (T)obj.GetType().GetProperty(propName).GetValue(obj, null);
        //}
    }
}